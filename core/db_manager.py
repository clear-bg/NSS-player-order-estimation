# order_tool/core/db_manager.py

import mysql.connector
from mysql.connector import Error
import os
# 環境変数 (.env) をロードするためのライブラリ
from dotenv import load_dotenv

# プロジェクトルートにある.envファイルをロード
load_dotenv()

# 接続情報の設定: 環境変数から取得。取得できなければデフォルト値（Docker Composeの設定）を使用
DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'db'),
    'port': os.getenv('DB_PORT', 3306),
    'database': os.getenv('DB_NAME', 'order_ranking_db'),
    'user': os.getenv('DB_USER', 'app_user'),
    # パスワードは必ず環境変数から取得
    'password': os.getenv('DB_PASSWORD'), 
}

# --- テーブル作成用のSQL ---
# MySQL の構文で記述します。VARCHAR(50) などの型を使用。
SQL_CREATE_TABLES = """
-- 1. Players テーブル (プレイヤー情報)
CREATE TABLE IF NOT EXISTS Players (
    player_id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(100),
    first_seen DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Observations テーブル (観測ログ)
CREATE TABLE IF NOT EXISTS Observations (
    observation_id INT AUTO_INCREMENT PRIMARY KEY,
    observation_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    ordered_list TEXT
);

-- 3. Relationship テーブル (優劣関係の集計)
CREATE TABLE IF NOT EXISTS Relationship (
    superior_player_id VARCHAR(50),
    inferior_player_id VARCHAR(50),
    frequency INT DEFAULT 0,
    PRIMARY KEY (superior_player_id, inferior_player_id)
);
"""

def get_db_connection():
    """MySQLデータベースへの接続オブジェクトを返す"""
    # パスワードが設定されていない場合はエラーを出す
    if not DB_CONFIG['password']:
        raise ValueError("DB_PASSWORDが設定されていません。")

    try:
        # DB_CONFIG 辞書の内容を展開して接続
        conn = mysql.connector.connect(**DB_CONFIG)
        if conn.is_connected():
            return conn
    except Error as e:
        print(f"MySQL接続エラー: {e}")
        # 接続エラーが発生した場合は、プログラムを停止させるため例外を再送出
        raise

def setup_database():
    """データベースに接続し、すべてのテーブルを作成する"""
    conn = None
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        # SQL文を一つずつ実行
        for sql in SQL_CREATE_TABLES.split(';'):
            sql = sql.strip()
            if sql:
                print(f"Executing: {sql[:50]}...")
                cursor.execute(sql)

        conn.commit()
        print("✅ MySQLテーブル作成/確認完了。")

    except Exception as e:
        print(f"データベースセットアップ中に致命的なエラーが発生しました: {e}")
        # 接続できていた場合はロールバックを試みる
        if conn and conn.is_connected():
            conn.rollback()
        raise

    finally:
        if conn and conn.is_connected():
            conn.close()


# --- テスト実行ブロック ---
if __name__ == '__main__':
    try:
        setup_database()
    except Exception:
        print("❌ DBセットアップテスト失敗。Dockerコンテナの起動状況と.envファイルを確認してください。")