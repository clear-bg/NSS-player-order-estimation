using System.Collections.Generic;
using System.Threading.Tasks; // 追加
using MySqlConnector;
using NssOrderTool.Database;

namespace NssOrderTool.Repositories
{
    public class PlayerRepository
    {
        private readonly DbManager _dbManager;

        public PlayerRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task RegisterPlayersAsync(IEnumerable<string> players)
        {
            using var conn = await _dbManager.GetConnectionAsync();
            foreach (var p in players)
            {
                var sql = "INSERT IGNORE INTO Players (player_id) VALUES (@pid)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pid", p);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<string>> GetAllPlayersAsync()
        {
            var list = new List<string>();
            var sql = "SELECT player_id FROM Players ORDER BY player_id";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        public async Task ClearAllAsync()
        {
            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand("TRUNCATE TABLE Players;", conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}