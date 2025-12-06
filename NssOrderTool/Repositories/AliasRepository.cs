using System;
using System.Collections.Generic;
using MySqlConnector;
using NssOrderTool.Database;
using NssOrderTool.Models;

namespace NssOrderTool.Repositories
{
    public class AliasRepository
    {
        private readonly DbManager _dbManager;

        public AliasRepository()
        {
            _dbManager = new DbManager();
        }

        // 1. エイリアスの追加
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

        // 2. エイリアスの削除
        public void DeleteAlias(string alias)
        {
            var sql = "DELETE FROM Aliases WHERE alias_name = @alias";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias);
            cmd.ExecuteNonQuery();
        }

        // 3. 全エイリアスの取得
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

        // 4. 指定したターゲットのエイリアス一覧を取得
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