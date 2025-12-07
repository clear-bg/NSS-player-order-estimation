using System.Collections.Generic;
using MySqlConnector;
using NssOrderTool.Database;

namespace NssOrderTool.Repositories
{
    public class PlayerRepository
    {
        private readonly DbManager _dbManager;

        public PlayerRepository()
        {
            _dbManager = new DbManager();
        }

        // プレイヤーの登録 (重複無視)
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

        // 全プレイヤーIDの取得 (将来の機能用)
        public List<string> GetAllPlayers()
        {
            var list = new List<string>();
            var sql = "SELECT player_id FROM Players ORDER BY player_id";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        // プレイヤーデータの全削除
        public void ClearAll()
        {
            using var conn = _dbManager.GetConnection();
            new MySqlCommand("TRUNCATE TABLE Players;", conn).ExecuteNonQuery();
        }
    }
}