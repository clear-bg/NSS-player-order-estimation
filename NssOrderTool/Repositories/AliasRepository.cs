using System;
using System.Collections.Generic;
using System.Threading.Tasks; // 追加
using MySqlConnector;
using NssOrderTool.Database;

namespace NssOrderTool.Repositories
{
    public class AliasRepository
    {
        private readonly DbManager _dbManager;

        public AliasRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task AddAliasAsync(string alias, string target)
        {
            var sql = "INSERT INTO Aliases (alias_name, target_player_id) VALUES (@alias, @target)";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            cmd.Parameters.AddWithValue("@target", target);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new InvalidOperationException($"エイリアス '{alias}' は既に登録されています。");
            }
        }

        public virtual async Task DeleteAliasAsync(string alias)
        {
            var sql = "DELETE FROM Aliases WHERE alias_name = @alias";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<Dictionary<string, string>> GetAliasDictionaryAsync()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sql = "SELECT alias_name, target_player_id FROM Aliases";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
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

        public virtual async Task<List<string>> GetAliasesByTargetAsync(string targetName)
        {
            var list = new List<string>();
            var sql = "SELECT alias_name FROM Aliases WHERE target_player_id = @target ORDER BY alias_name";

            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@target", targetName);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        public virtual async Task ClearAllAsync()
        {
            using var conn = await _dbManager.GetConnectionAsync();
            using var cmd = new MySqlCommand("TRUNCATE TABLE Aliases;", conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}