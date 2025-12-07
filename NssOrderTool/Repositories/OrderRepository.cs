using System.Collections.Generic;
using System.Threading.Tasks; // 追加
using MySqlConnector;
using NssOrderTool.Database;
using NssOrderTool.Models;

namespace NssOrderTool.Repositories
{
    public class OrderRepository
    {
        private readonly DbManager _dbManager;

        public OrderRepository()
        {
            _dbManager = new DbManager();
        }

        public async Task AddObservationAsync(string rawInput)
        {
            var sql = "INSERT INTO Observations (ordered_list, observation_time) VALUES (@list, NOW());";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@list", rawInput);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePairsAsync(List<OrderPair> pairs)
        {
            using var conn = await _dbManager.GetConnectionAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var sql = @"
                    INSERT INTO SequencePairs (predecessor_id, successor_id, frequency)
                    VALUES (@pred, @succ, 1)
                    ON DUPLICATE KEY UPDATE frequency = frequency + 1;";

                foreach (var pair in pairs)
                {
                    using var cmd = new MySqlCommand(sql, conn, tx);
                    cmd.Parameters.AddWithValue("@pred", pair.Predecessor);
                    cmd.Parameters.AddWithValue("@succ", pair.Successor);
                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderPair>> GetAllPairsAsync()
        {
            var list = new List<OrderPair>();
            var sql = "SELECT predecessor_id, successor_id FROM SequencePairs";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new OrderPair(reader.GetString(0), reader.GetString(1)));
            }
            return list;
        }

        public async Task ClearAllDataAsync()
        {
            using var conn = await _dbManager.GetConnectionAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                using (var cmd = new MySqlCommand("TRUNCATE TABLE SequencePairs;", conn, tx)) await cmd.ExecuteNonQueryAsync();
                using (var cmd = new MySqlCommand("TRUNCATE TABLE Observations;", conn, tx)) await cmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public string GetEnvironmentName()
        {
            return _dbManager.CurrentEnvironment;
        }
    }
}