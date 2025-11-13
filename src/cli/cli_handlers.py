# 必要なモジュールをインポートし直す
from src.core.db_manager import (
    insert_observation_log,
    update_relationships,
    fetch_all_relationships,
    fetch_all_player_ids,
    setup_test_environment # setup_test_environmentはdb_manager.pyに移動したと仮定
)
from src.core.extractor import extract_relationships
from src.logic.sorter import analyze_and_rank, build_weighted_graph
from src.logic.result_formatter import format_ranking, assign_rank_and_print

# --- handle_new_observation 関数 (main.pyから移動) ---
def handle_new_observation(conn):
    """ユーザーから観測リストを受け取り、DBに投入する"""
    print("\n--- 1. 新規観測データの入力 ---")
    observation_list_str = input("並び順をカンマ区切りで入力してください (例: P1, P3, P2): ").strip()

    if not observation_list_str:
        print("入力がありませんでした。")
        return

    try:
        # ... (中略：ロジックはmain.pyから変更なし) ...
        # 1. 観測ログを Observations テーブルに挿入
        observation_id = insert_observation_log(conn, observation_list_str)

        # 2. 観測リストから優劣関係を抽出
        relationships = extract_relationships(observation_list_str, observation_id)

        # 3. Relationship テーブルに優劣関係を挿入/更新
        update_relationships(conn, relationships)

        # Playerマスターテーブルにも登場したプレイヤーを追加（既存ならIGNORE）
        player_ids = [p.strip() for p in observation_list_str.split(',') if p.strip()]
        player_values = ", ".join([f"('{p}')" for p in player_ids])
        cursor = conn.cursor()
        cursor.execute(f"INSERT IGNORE INTO Players (player_id) VALUES {player_values}")
        conn.commit()

        print(f"\n✅ 観測ログ ID:{observation_id} を処理し、{len(relationships)} 件の関係を更新しました。")

    except Exception as e:
        print(f"\n❌ データ処理中にエラーが発生しました: {e}")

# --- handle_show_ranking 関数 (main.pyから移動) ---
def handle_show_ranking(conn):
    """現在の優劣関係に基づき、ランキングを推定し表示する"""
    print("\n--- 2. ランキングの表示 ---")

    # グラフ構築に必要なデータを取得
    relationships = fetch_all_relationships(conn)
    if not relationships:
        print("データが不足しています。観測データを入力してください。")
        return

    G = build_weighted_graph(relationships)
    ranking_result = analyze_and_rank(conn)

    # ... (中略：ロジックはmain.pyから変更なし) ...
    if isinstance(ranking_result, list):
        # 順序が推定できた場合 (トポロジカルソート成功)
        all_player_ids = set(fetch_all_player_ids(conn))
        nodes_in_graph = set(G.nodes)
        missing_players = list(all_player_ids - nodes_in_graph)
        final_sorted_list = ranking_result + missing_players

        ranked_groups = format_ranking(final_sorted_list, G)
        assign_rank_and_print(ranked_groups)

    elif isinstance(ranking_result, tuple):
        # 矛盾が検出された場合 (analyze_and_rankがTupleを返した時)
        error_type, conflicting_nodes = ranking_result
        print("---------------------------------------")
        print(f"⚠️ {error_type} が検出されました。")
        print(f"影響ノード: {conflicting_nodes}")
        print("順序を確定できませんでした。矛盾を解消する観測データが必要です。")
        print("---------------------------------------")