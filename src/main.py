import traceback
from src.core.db_manager import get_db_connection, setup_test_environment
from src.cli.cli_handlers import handle_new_observation, handle_show_ranking # 👈 新しいハンドラをインポート

def main():
    conn = None
    try:
        conn = get_db_connection()
        print("--- プレイヤー順序推定ツール (CLI - 本番モード) ---")

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
        traceback.print_exc()

    finally:
        if conn and conn.is_connected():
            conn.close()

if __name__ == '__main__':
    main()