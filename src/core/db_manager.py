# order_tool/core/db_manager.py

import mysql.connector
from mysql.connector import Error
import os
# 環境変数 (.env) をロードするためのライブラリ
from dotenv import load_dotenv
from typing import List, Tuple

# プロジェクトルートにある.envファイルをロード
load_dotenv()

# 接続情報の設定: 環境変数から取得。取得できなければデフォルト値（Docker Composeの設定）を使用
DB_CONFIG = {
    'host': os.getenv('DB_HOST'),
    'port': os.getenv('DB_PORT', 3306),
    'database': os.getenv('DB_NAME', 'player_order_data_test'),
    'user': os.getenv('DB_USER', 'admin'),
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
    if not DB_CONFIG['password'] or not DB_CONFIG['host']:
        raise ValueError("DB_HOSTまたはDB_PASSWORDが設定されていません。")

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

def insert_observation_log(conn, ordered_list_str: str) -> int:
    """
    Observations テーブルに観測ログを挿入し、新しく生成された ID を返す。
    """
    sql = "INSERT INTO Observations (ordered_list, observation_time) VALUES (%s, NOW())"
    cursor = conn.cursor()
    cursor.execute(sql, (ordered_list_str,))

    # MySQLでの最終挿入IDの取得
    new_id = cursor.lastrowid
    conn.commit()
    return new_id

def update_relationships(conn, relationship_data: List[Tuple[str, str, int, int]]):
    """
    Relationship テーブルに優劣関係を挿入/更新する。
    ON DUPLICATE KEY UPDATE 構文で頻度をインクリメントする。
    """
    sql_insert_relationship = """
    INSERT INTO Relationship (superior_player_id, inferior_player_id, frequency)
    VALUES (%s, %s, %s)
    ON DUPLICATE KEY UPDATE
        frequency = frequency + VALUES(frequency);
    """
    # 観測IDは今回は無視して、frequencyの更新のみに集中
    data_for_update = [(r[0], r[1], r[3]) for r in relationship_data]

    cursor = conn.cursor()
    cursor.executemany(sql_insert_relationship, data_for_update)
    conn.commit()
    print(f"データベースに {cursor.rowcount} 件の優劣関係を挿入/更新しました。")

def fetch_all_relationships(conn) -> List[Tuple[str, str, int]]:
    """
    Relationship テーブルの全レコード (優位ID, 劣位ID, 頻度) を取得する。
    """
    sql = "SELECT superior_player_id, inferior_player_id, frequency FROM Relationship"
    cursor = conn.cursor()
    cursor.execute(sql)
    return cursor.fetchall()

def fetch_all_player_ids(conn) -> List[str]:
    """
    Players テーブルに登録されている全プレイヤーIDを取得する。
    """
    sql = "SELECT player_id FROM Players"
    cursor = conn.cursor()
    cursor.execute(sql)
    # 結果がタプルのリストで返されるため、IDのリストに変換
    return [row[0] for row in cursor.fetchall()]

def setup_test_environment(conn):
    """
    DBをクリアし、テーブルをセットアップする機能。
    ⚠️ 本番設定では、テストプレイヤーの静的登録は行いません。
    """
    cursor = conn.cursor()
    print("--- 環境セットアップ: DBクリア ---")
    try:
        setup_database()

        # MySQLではTRUNCATE TABLEでデータをクリア
        cursor.execute("TRUNCATE TABLE Observations")
        cursor.execute("TRUNCATE TABLE Relationship")
        cursor.execute("TRUNCATE TABLE Players") # プレイヤーマスターテーブルをクリア

        # ⚠️ ここにあったテストプレイヤー（A, B, C...）の静的登録コードを削除します。

        conn.commit()
        print("✅ DBクリアとテーブル初期化完了。")
    except Exception as e:
        print(f"⚠️ 環境セットアップ中にエラー: {e}")
        conn.rollback()

# --- テスト実行ブロック ---
if __name__ == '__main__':
    try:
        setup_database()
    except Exception:
        print("❌ DBセットアップテスト失敗。Dockerコンテナの起動状況と.envファイルを確認してください。")