using System;
using System.Collections.Generic;
using MySqlConnector;
using NssOrderTool.Models;    // 追加
using NssOrderTool.Database;  // 追加 (DbManager用)

namespace NssOrderTool.Services
{
    public class RankingRepository
    {
        private readonly DbManager _dbManager;

        public RankingRepository()
        {
            _dbManager = new DbManager();
        }

        // 削除: EnsureTablesExist は DbSchemaService に移動しました

        // 2. 観測データの登録
        public void AddObservation(string rawInput)
        {
            var sql = "INSERT INTO Observations (ordered_list, observation_time) VALUES (@list, NOW());";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@list", rawInput);
            cmd.ExecuteNonQuery();
        }

        // 3. プレイヤーマスタへの登録
        public void RegisterPlayers(IEnumerable<string> players)
        {
            using var conn = _dbManager.GetConnection();
            foreach (var p in players)
            {
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
            using var tx = conn.BeginTransaction();

            try
            {
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
                throw;
            }
        }

        // 5. 全ペアの取得
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

        // 6. 全データの削除
        public void ClearAllData()
        {
            using var conn = _dbManager.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
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

        // 7. 現在の環境名を取得する
        public string GetEnvironmentName()
        {
            return _dbManager.CurrentEnvironment;
        }

        // 8. エイリアスの追加
        public void AddAlias(string alias, string target)
        {
            var sql = "INSERT INTO Aliases (alias_name, target_player_id) VALUES (@alias, @target)";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            cmd.Parameters.AddWithValue("@target", target);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
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

        // 10. 全エイリアスの取得
        public Dictionary<string, string> GetAliasDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sql = "SELECT alias_name, target_player_id FROM Aliases";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var alias = reader.GetString(0);
                var target = reader.GetString(1);
                if (!dict.ContainsKey(alias))
                {
                    dict.Add(alias, target);
                }
            }
            return dict;
        }

        // 11. 指定したターゲットのエイリアス一覧を取得
        public List<string> GetAliasesByTarget(string targetName)
        {
            var list = new List<string>();
            var sql = "SELECT alias_name FROM Aliases WHERE target_player_id = @target ORDER BY alias_name";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@target", targetName);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }
    }
}