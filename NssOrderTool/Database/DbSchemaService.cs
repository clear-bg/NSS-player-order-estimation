using MySqlConnector;

namespace NssOrderTool.Database
{
    public class DbSchemaService
    {
        private readonly DbManager _dbManager;

        public DbSchemaService()
        {
            _dbManager = new DbManager();
        }

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
                );";

            using var conn = _dbManager.GetConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}