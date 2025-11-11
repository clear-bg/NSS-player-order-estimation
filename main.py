# order_tool/main.py

from core.db_manager import get_db_connection, insert_observation_log, update_relationships
from core.extractor import extract_relationships

def run_data_ingestion_test():
    """観測データを処理し、データベースに書き込む統合テスト"""

    # 観測データ (Player3 が Player1, Player4 より上)
    test_observation_list_1 = "Player3, Player1, Player4"

    # 接続の確立
    conn = None
    try:
        conn = get_db_connection()
        print("--- 統合テスト開始: 新規観測データの投入 ---")

        # 1. 観測ログを Observations テーブルに挿入
        observation_id = insert_observation_log(conn, test_observation_list_1)
        print(f"ログ挿入成功。観測ID: {observation_id}")

        # 2. 観測リストから優劣関係を抽出
        relationships = extract_relationships(test_observation_list_1, observation_id)
        print(f"抽出された関係数: {len(relationships)} 件")

        # 3. Relationship テーブルに優劣関係を挿入/更新
        update_relationships(conn, relationships)

        print("\n✅ データ投入テスト成功。MySQLを確認してください。")

    except Exception as e:
        print(f"\n❌ 統合テスト失敗: {e}")

    finally:
        if conn and conn.is_connected():
            conn.close()

if __name__ == '__main__':
    run_data_ingestion_test()