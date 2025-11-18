# update_security_group.py

import boto3
import requests
import sys

# --- 設定 ---
# 🚨 控えた情報に置き換えてください 🚨
AWS_REGION = 'ap-southeast-2'  # 例: 東京リージョン
SECURITY_GROUP_ID = 'sg-0356c9a6980b05e17'  # ターゲットのセキュリティグループID
DB_PORT = 3306
# -----------

def get_current_public_ip():
    """外部サービスを使って現在のパブリックIPアドレスを取得する"""
    try:
        # IPアドレス確認サービス
        response = requests.get('https://api.ipify.org')
        response.raise_for_status() # HTTPエラーチェック
        current_ip = response.text.strip()
        # CIDR形式に変換
        return f"{current_ip}/32"
    except requests.exceptions.RequestException as e:
        print(f"エラー: 外部IPアドレスの取得に失敗しました: {e}", file=sys.stderr)
        return None

def update_security_group_rule(ec2_client, security_group_id, new_cidr_ip):
    """
    セキュリティグループのポート3306ルールを更新する。
    既存のIPアドレスルールを探し、新しいIPアドレスに置き換える。
    """

    # 既存のルールを取得
    response = ec2_client.describe_security_groups(GroupIds=[security_group_id])
    ip_permissions = response['SecurityGroups'][0]['IpPermissions']

    old_cidr_to_revoke = None

    # 3306ポートのルールを探す
    for rule in ip_permissions:
        # ルールがMySQLポート(3306)で、かつIPアドレスで制限されているか確認
        if rule.get('FromPort') == DB_PORT and rule.get('ToPort') == DB_PORT:
            for ip_range in rule.get('IpRanges', []):
                cidr = ip_range.get('CidrIp')
                # 0.0.0.0/0 (全開放) ではない、有効なCIDRを探す
                if cidr and cidr != '0.0.0.0/0' and cidr != new_cidr_ip:
                    old_cidr_to_revoke = cidr
                    break
        if old_cidr_to_revoke:
            break

    # --- 1. 古いルールの削除 (Revoke) ---
    if old_cidr_to_revoke:
        try:
            ec2_client.revoke_security_group_ingress(
                GroupId=security_group_id,
                IpProtocol='tcp',
                FromPort=DB_PORT,
                ToPort=DB_PORT,
                CidrIp=old_cidr_to_revoke
            )
            print(f"✅ 古いルールを削除: {old_cidr_to_revoke}")
        except Exception as e:
            # 既に削除されている場合のAWSエラーは無視する
            if 'does not exist' not in str(e):
                print(f"⚠️ 古いルールの削除に失敗: {e}")

    # --- 2. 新しいルールの追加 (Authorize) ---
    try:
        ec2_client.authorize_security_group_ingress(
            GroupId=security_group_id,
            IpPermissions=[{
                'IpProtocol': 'tcp',
                'FromPort': DB_PORT,
                'ToPort': DB_PORT,
                'IpRanges': [{'CidrIp': new_cidr_ip, 'Description': 'Current PC IP Access'}]
            }]
        )
        print(f"✅ 新しいルールを追加: {new_cidr_ip}")
        return True
    except Exception as e:
        if 'already exists' in str(e):
            print(f"✅ ルールは既に最新です: {new_cidr_ip}")
            return True
        else:
            print(f"❌ 新しいルールの追加に失敗: {e}", file=sys.stderr)
            return False

if __name__ == "__main__":
    new_cidr = get_current_public_ip()
    if new_cidr:
        print(f"現在の外部 IP: {new_cidr}")
        try:
            # AWSクライアントを初期化
            ec2_client = boto3.client('ec2', region_name=AWS_REGION)

            # セキュリティグループを更新
            success = update_security_group_rule(ec2_client, SECURITY_GROUP_ID, new_cidr)

            if success:
                print("--- セキュリティグループの更新が完了しました。---")
            else:
                print("--- セキュリティグループの更新に失敗しました。---")

        except Exception as e:
            print(f"❌ AWS認証または処理エラー: {e}", file=sys.stderr)
            print("aws configureを実行し、認証情報とリージョン設定を確認してください。")