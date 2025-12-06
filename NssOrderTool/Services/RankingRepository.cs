using System;
using System.Collections.Generic;
using MySqlConnector;

namespace NssOrderTool.Services
{
    public class RankingRepository
    {
        private readonly DbManager _dbManager;

        public RankingRepository()
        {
            _dbManager = new DbManager();
        }

        // 1. テーブル作成 (アプリ起動時に呼ぶと安心)
        public void EnsureTablesExist()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Players (
                    player_id VARCHAR(50) PRIMARY KEY,
                    name VARCHAR(100),
                    first_seen DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS Observations (
                    observation_id INT AUTO_INCREMENT PRIMARY KEY,
                    observation_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ordered_list TEXT
                );
                CREATE TABLE IF NOT EXISTS Relationship (
                    superior_player_id VARCHAR(50),
                    inferior_player_id VARCHAR(50),
                    frequency INT DEFAULT 0,
                    PRIMARY KEY (superior_player_id, inferior_player_id)
                );";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        // 2. 観測データの登録 (ログ保存)
        public void AddObservation(string rawInput)
        {
            var sql = "INSERT INTO Observations (ordered_list, observation_time) VALUES (@list, NOW());";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@list", rawInput);
            cmd.ExecuteNonQuery();
        }

        // 3. プレイヤーマスタへの登録 (存在しない場合のみ)
        public void RegisterPlayers(IEnumerable<string> players)
        {
            using var conn = _dbManager.GetConnection();
            foreach (var p in players)
            {
                // IGNOREを使って重複エラーを無視
                var sql = "INSERT IGNORE INTO Players (player_id) VALUES (@pid)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pid", p);
                cmd.ExecuteNonQuery();
            }
        }

        // 4. 順序ペアの保存/更新
        public void UpdatePairs(List<OrderPair> pairs)
        {
            using var conn = _dbManager.GetConnection();
            using var tx = conn.BeginTransaction(); // 整合性のためトランザクション使用

            try
            {
                // 重複していれば frequency (頻度) を +1 する
                var sql = @"
                    INSERT INTO Relationship (superior_player_id, inferior_player_id, frequency)
                    VALUES (@sup, @inf, 1)
                    ON DUPLICATE KEY UPDATE frequency = frequency + 1;";

                foreach (var pair in pairs)
                {
                    using var cmd = new MySqlCommand(sql, conn, tx);
                    cmd.Parameters.AddWithValue("@sup", pair.Predecessor);
                    cmd.Parameters.AddWithValue("@inf", pair.Successor);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw; // エラーを再スロー
            }
        }

        // 5. 全ペアの取得 (ランキング計算用)
        public List<OrderPair> GetAllPairs()
        {
            var list = new List<OrderPair>();
            var sql = "SELECT superior_player_id, inferior_player_id FROM Relationship";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new OrderPair(reader.GetString(0), reader.GetString(1)));
            }
            return list;
        }

        // 6. 全データの削除 (初期化用)
        public void ClearAllData()
        {
            using var conn = _dbManager.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // 外部キー制約がある場合は順序に注意（今回は制約なし）
                new MySqlCommand("TRUNCATE TABLE Relationship;", conn, tx).ExecuteNonQuery();
                new MySqlCommand("TRUNCATE TABLE Observations;", conn, tx).ExecuteNonQuery();
                new MySqlCommand("TRUNCATE TABLE Players;", conn, tx).ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // 7. 現在の環境名を取得する (今回追加)
        public string GetEnvironmentName()
        {
            return _dbManager.CurrentEnvironment;
        }
    }
}