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
                );
                CREATE TABLE IF NOT EXISTS Aliases (
                    alias_name VARCHAR(50) PRIMARY KEY,
                    target_player_id VARCHAR(50)
                );
                ";


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
                new MySqlCommand("TRUNCATE TABLE Aliases;", conn, tx).ExecuteNonQuery();

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

        // 8. エイリアスの追加
        public void AddAlias(string alias, string target)
        {
            // 正規名と別名が同じ場合は登録不要（あるいはエラー）ですが、
            // ここではDB制約に任せてシンプルにINSERTします。
            var sql = "INSERT INTO Aliases (alias_name, target_player_id) VALUES (@alias, @target)";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            cmd.Parameters.AddWithValue("@target", target);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                throw new InvalidOperationException($"エイリアス '{alias}' は既に登録されています。");
            }
        }

        // 9. エイリアスの削除
        public void DeleteAlias(string alias)
        {
            var sql = "DELETE FROM Aliases WHERE alias_name = @alias";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            cmd.ExecuteNonQuery();
        }

        // 10. 全エイリアスの取得 (変換用辞書として返す)
        public Dictionary<string, string> GetAliasDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // 大文字小文字を区別しない
            var sql = "SELECT alias_name, target_player_id FROM Aliases";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var alias = reader.GetString(0);
                var target = reader.GetString(1);
                // 辞書に追加 (重複はDBで弾いているはずだが念のためTryAdd)
                if (!dict.ContainsKey(alias))
                {
                    dict.Add(alias, target);
                }
            }
            return dict;
        }
    }
}