# order_tool/main.py

from core.db_manager import (
    get_db_connection,
    insert_observation_log,
    update_relationships,
    fetch_all_relationships,
    fetch_all_player_ids,
    setup_database # 開発中はこの関数もインポートしておく
)
from core.extractor import extract_relationships
from logic.sorter import analyze_and_rank, build_weighted_graph
from logic.result_formatter import format_ranking, assign_rank_and_print

# --- データのクリアと初期登録 (開発用) ---
# order_tool/main.py (setup_test_environment の修正箇所)

# ... (中略)
def setup_test_environment(conn):
    """開発中のテストを容易にするため、DBをクリアし初期データを投入する"""
    cursor = conn.cursor()
    print("--- 開発環境セットアップ: DBクリアと初期プレイヤー登録 ---")
    try:
        # テーブル構造が存在しない可能性に備えて、setup_databaseを呼び出す
        setup_database()

        # MySQLではTRUNCATE TABLEでデータをクリア
        cursor.execute("TRUNCATE TABLE Observations")
        cursor.execute("TRUNCATE TABLE Relationship")
        # プレイヤーマスターテーブルもクリアする (前回のバグ修正)
        cursor.execute("TRUNCATE TABLE Players")

        # テストプレイヤーをPlayersテーブルに登録
        cursor.execute("""
            INSERT IGNORE INTO Players (player_id)
            VALUES ('A'), ('B'), ('C'), ('D'), ('P1'), ('P2'), ('P3'), ('P4')
        """)
        conn.commit()
        print("✅ DBクリアとテストプレイヤー(A, B, C, D, P1-P4)登録完了。")
    except Exception as e:
        print(f"⚠️ 環境セットアップ中にエラー: {e}")
        conn.rollback()
        # ここでは例外を再送出しない（処理を継続させるため）

# --- CLI機能: 1. 新規観測データの投入 ---
def handle_new_observation(conn):
    """ユーザーから観測リストを受け取り、DBに投入する"""
    print("\n--- 1. 新規観測データの入力 ---")
    observation_list_str = input("並び順をカンマ区切りで入力してください (例: P1, P3, P2): ").strip()

    if not observation_list_str:
        print("入力がありませんでした。")
        return

    try:
        # 1. 観測ログを Observations テーブルに挿入
        observation_id = insert_observation_log(conn, observation_list_str)

        # 2. 観測リストから優劣関係を抽出
        relationships = extract_relationships(observation_list_str, observation_id)

        # 3. Relationship テーブルに優劣関係を挿入/更新
        update_relationships(conn, relationships)

        print(f"\n✅ 観測ログ ID:{observation_id} を処理し、{len(relationships)} 件の関係を更新しました。")

    except Exception as e:
        print(f"\n❌ データ処理中にエラーが発生しました: {e}")

# --- CLI機能: 2. ランキングの表示 ---
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

    if isinstance(ranking_result, list):
        # 順序が推定できた場合 (トポロジカルソート成功)

        # グラフに存在しないプレイヤーを特定し、リストに追加（最下位グループとする）
        all_player_ids = set(fetch_all_player_ids(conn))
        nodes_in_graph = set(G.nodes)
        missing_players = list(all_player_ids - nodes_in_graph)

        # トポロジカルソートの結果に欠けているノードを追加
        final_sorted_list = ranking_result + missing_players

        # 整形ロジックを実行し、結果を表示
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

# --- メイン処理 (CLIループ) ---
def main():
    conn = None
    try:
        conn = get_db_connection()
        print("--- プレイヤー順序推定ツール (CLI) ---")

        # 開発中のため、起動時にテスト環境をセットアップ
        setup_test_environment(conn)

        while True:
            print("\n何をしますか？")
            print("1: 新規観測データの入力 (Player1, Player3, ...)")
            print("2: 現在の推定ランキングを表示")
            print("3: 終了")
            print("0: DBをクリアして初期化 (テスト用)")

            choice = input("選択してください (1/2/3/0): ").strip()

            if choice == '1':
                handle_new_observation(conn)
            elif choice == '2':
                handle_show_ranking(conn)
            elif choice == '3':
                print("ツールを終了します。")
                break
            elif choice == '0':
                setup_test_environment(conn)
            else:
                print("無効な選択です。1, 2, 3, 0のいずれかを入力してください。")

    except Exception as e:
        print(f"\n致命的な接続エラーが発生しました。アプリケーションを終了します: {e}")
        # 詳細なトレースバックを表示
        import traceback
        traceback.print_exc()

    finally:
        if conn and conn.is_connected():
            conn.close()

if __name__ == '__main__':
    main()