using System;
using System.Collections.Generic;
using MySqlConnector;
using NssOrderTool.Models;    // 追加
using NssOrderTool.Database;

namespace NssOrderTool.Repositories
{
    public class OrderRepository
    {
        private readonly DbManager _dbManager;

        public OrderRepository()
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

        // 3. 順序ペアの保存/更新
        public void UpdatePairs(List<OrderPair> pairs)
        {
            using var conn = _dbManager.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO SequencePairs (predecessor_id, successor_id, frequency)
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

        // 4. 全ペアの取得
        public List<OrderPair> GetAllPairs()
        {
            var list = new List<OrderPair>();
            var sql = "SELECT predecessor_id, successor_id FROM SequencePairs";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new OrderPair(reader.GetString(0), reader.GetString(1)));
            }
            return list;
        }

        // 5. 全データの削除
        public void ClearAllData()
        {
            using var conn = _dbManager.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // Players と Aliases の TRUNCATE を削除
                new MySqlCommand("TRUNCATE TABLE SequencePairs;", conn, tx).ExecuteNonQuery();
                new MySqlCommand("TRUNCATE TABLE Observations;", conn, tx).ExecuteNonQuery();

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
    }
}