from core.db_manager import get_db_connection, insert_observation_log, update_relationships, fetch_all_relationships, fetch_all_player_ids
from core.extractor import extract_relationships
from logic.sorter import analyze_and_rank, build_weighted_graph
from logic.result_formatter import format_ranking, assign_rank_and_print # 👈 ここをインポート

def run_data_ingestion_test():
    """観測データを処理し、データベースに書き込み、順序推定を行う統合テスト"""

    # 観測データ (P3 > P1 > P4, P3 > P2 > P1)
    test_observation_list_1 = "Player3, Player1, Player4"
    test_observation_list_2 = "Player3, Player2, Player1"

    # Player5, Player6 を追加 (順序に関与しない/データなしのプレイヤー)
    test_observation_list_3 = "Player5, Player3, Player6"

    conn = None
    try:
        conn = get_db_connection()
        print("--- 統合テスト開始: 新規観測データの投入 ---")

        # --- データのクリアと初期登録 ---
        # 開発中のテストを容易にするため、毎回DBをクリアして初期データを投入します。
        cursor = conn.cursor()

        # MySQLではTRUNCATE TABLEが最も速いクリア方法
        cursor.execute("TRUNCATE TABLE Observations")
        cursor.execute("TRUNCATE TABLE Relationship")

        # 全プレイヤーをPlayersテーブルに登録（IGNOREで重複登録を防止）
        cursor.execute("""
            INSERT IGNORE INTO Players (player_id)
            VALUES ('Player1'), ('Player2'), ('Player3'), ('Player4'), ('Player5'), ('Player6')
        """)
        conn.commit()

        # 観測1回目
        observation_id_1 = insert_observation_log(conn, test_observation_list_1)
        relationships_1 = extract_relationships(test_observation_list_1, observation_id_1)
        update_relationships(conn, relationships_1)

        # 観測2回目
        observation_id_2 = insert_observation_log(conn, test_observation_list_2)
        relationships_2 = extract_relationships(test_observation_list_2, observation_id_2)
        update_relationships(conn, relationships_2)

        # 観測3回目
        observation_id_3 = insert_observation_log(conn, test_observation_list_3)
        relationships_3 = extract_relationships(test_observation_list_3, observation_id_3)
        update_relationships(conn, relationships_3)

        print(f"✅ データ投入完了。3つの観測ログを処理しました。")

        # --- 順序推定の実行 ---
        print("\n--- 順序推定 (トポロジカルソート) の実行 ---")

        # グラフ構築に必要なデータを取得
        relationships = fetch_all_relationships(conn)
        G = build_weighted_graph(relationships)

        ranking_result = analyze_and_rank(conn)

        if isinstance(ranking_result, list):
            print(f"✅ トポロジカルソート順 (整形前): {ranking_result}")

            # --- 結果の整形とグループ化 ---
            # グラフに存在しないプレイヤーを特定
            all_player_ids = set(fetch_all_player_ids(conn))
            nodes_in_graph = set(G.nodes)

            # グラフに含まれていないプレイヤーは、ソート結果の末尾に追加（最も低い順位グループ）
            missing_players = list(all_player_ids - nodes_in_graph)

            # トポロジカルソートの結果に欠けているノードを追加
            final_sorted_list = ranking_result + missing_players

            # 整形ロジックを実行し、結果を表示
            ranked_groups = format_ranking(final_sorted_list, G)
            assign_rank_and_print(ranked_groups) 

        elif isinstance(ranking_result, tuple):
            print(f"⚠️ 矛盾検出エラー: {ranking_result[0]}")

    except Exception as e:
        print(f"\n❌ 統合テスト失敗: {e}")

    finally:
        if conn and conn.is_connected():
            conn.close()

if __name__ == '__main__':
    run_data_ingestion_test()