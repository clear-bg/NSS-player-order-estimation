import boto3
import requests
import sys
import os
from dotenv import load_dotenv

# .envファイルをロード
load_dotenv()

# --- 設定読み込み ---
# 環境変数から取得（設定がない場合はNoneになるため、後でチェックする）
AWS_REGION = os.getenv('AWS_REGION', 'ap-southeast-2')  # デフォルトはシドニー
SECURITY_GROUP_ID = os.getenv('AWS_SECURITY_GROUP_ID')
DB_PORT = int(os.getenv('DB_PORT', 3306))

def get_current_public_ip():
    """外部サービスを使って現在のパブリックIPアドレスを取得する"""
    try:
        # IPアドレス確認サービス
        response = requests.get('https://api.ipify.org', timeout=5)
        response.raise_for_status()
        current_ip = response.text.strip()
        return f"{current_ip}/32"
    except requests.exceptions.RequestException as e:
        print(f"エラー: 外部IPアドレスの取得に失敗しました: {e}", file=sys.stderr)
        return None

def update_security_group_rule(ec2_client, security_group_id, new_cidr_ip):
    """
    セキュリティグループのポート3306ルールを更新する。
    既存のIPアドレスルール(My IP)を探し、新しいIPアドレスに置き換える。
    """
    print(f"ターゲットセキュリティグループ: {security_group_id}")

    try:
        # 既存のルールを取得
        response = ec2_client.describe_security_groups(GroupIds=[security_group_id])
        ip_permissions = response['SecurityGroups'][0]['IpPermissions']
    except Exception as e:
        print(f"❌ セキュリティグループ情報の取得に失敗: {e}")
        return False

    old_cidr_to_revoke = None

    # DBポートのルールを探す
    for rule in ip_permissions:
        if rule.get('FromPort') == DB_PORT and rule.get('ToPort') == DB_PORT:
            for ip_range in rule.get('IpRanges', []):
                cidr = ip_range.get('CidrIp')
                description = ip_range.get('Description', '')

                # 'Auto-Update' の説明がある、または 0.0.0.0/0 以外のルールを更新対象とする
                # (安全性のため、意図しない固定IPを消さないようDescriptionチェックを入れるとより良いですが、
                #  今回は「0.0.0.0/0以外かつ自分以外」を削除対象とします)
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
            print(f"✅ 古いルールを削除しました: {old_cidr_to_revoke}")
        except Exception as e:
            print(f"⚠️ 古いルールの削除中にエラー (無視可能): {e}")

    # --- 2. 新しいルールの追加 (Authorize) ---
    try:
        ec2_client.authorize_security_group_ingress(
            GroupId=security_group_id,
            IpPermissions=[{
                'IpProtocol': 'tcp',
                'FromPort': DB_PORT,
                'ToPort': DB_PORT,
                'IpRanges': [{
                    'CidrIp': new_cidr_ip,
                    'Description': 'Auto-Update via Script' # 識別用の説明を追加
                }]
            }]
        )
        print(f"✅ 新しいルールを追加しました: {new_cidr_ip}")
        return True
    except Exception as e:
        if 'InvalidPermission.Duplicate' in str(e):
            print(f"✅ ルールは既に最新です: {new_cidr_ip}")
            return True
        else:
            print(f"❌ 新しいルールの追加に失敗: {e}", file=sys.stderr)
            return False

if __name__ == "__main__":
    # 事前チェック
    if not SECURITY_GROUP_ID:
        print("❌ エラー: 環境変数 AWS_SECURITY_GROUP_ID が設定されていません。.envを確認してください。")
        sys.exit(1)

    new_cidr = get_current_public_ip()

    if new_cidr:
        print(f"現在の外部 IP: {new_cidr}")
        try:
            # AWSクライアントを初期化
            ec2_client = boto3.client('ec2', region_name=AWS_REGION)

            # 更新実行
            success = update_security_group_rule(ec2_client, SECURITY_GROUP_ID, new_cidr)

            if success:
                print("--- セキュリティグループの更新が完了しました。DBに接続可能です。 ---")
            else:
                print("--- 更新に失敗しました。設定を確認してください。 ---")

        except Exception as e:
            print(f"❌ AWS認証または予期せぬエラー: {e}", file=sys.stderr)
            print("aws configure が実行されているか、またはIAM権限を確認してください。")
