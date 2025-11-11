from core.db_manager import get_db_connection, insert_observation_log, update_relationships, fetch_all_relationships
from core.extractor import extract_relationships
from logic.sorter import analyze_and_rank # 👈 sorterモジュールをインポート

def run_data_ingestion_test():
    """観測データを処理し、データベースに書き込み、順序推定を行う統合テスト"""

    # 観測データ (Player3 > Player1 > Player4)
    test_observation_list_1 = "Player3, Player1, Player4"

    # Player2 を同率の可能性を持たせるために追加 (Player2は順序に関与しない)
    test_observation_list_2 = "Player3, Player2, Player1" 

    conn = None
    try:
        conn = get_db_connection()
        print("--- 統合テスト開始: 新規観測データの投入 ---")

        # データの投入 (観測1回目)
        observation_id_1 = insert_observation_log(conn, test_observation_list_1)
        relationships_1 = extract_relationships(test_observation_list_1, observation_id_1)
        update_relationships(conn, relationships_1)

        # データの投入 (観測2回目 - Player2が途中に入る)
        observation_id_2 = insert_observation_log(conn, test_observation_list_2)
        relationships_2 = extract_relationships(test_observation_list_2, observation_id_2)
        update_relationships(conn, relationships_2)

        print(f"✅ データ投入完了 (合計 {len(relationships_1) + len(relationships_2)} 件の関係を挿入/更新)。")

        # --- 順序推定の実行 ---
        print("\n--- 順序推定 (トポロジカルソート) の実行 ---")
        ranking_result = analyze_and_rank(conn)

        if isinstance(ranking_result, list):
            print(f"✅ 推定された全体の順序: {ranking_result}")
        elif isinstance(ranking_result, tuple):
            print(f"⚠️ 矛盾検出エラー: {ranking_result[0]}")

    except Exception as e:
        print(f"\n❌ 統合テスト失敗: {e}")

    finally:
        if conn and conn.is_connected():
            conn.close()

if __name__ == '__main__':
    run_data_ingestion_test()