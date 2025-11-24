using System;
using System.Data;
using MySqlConnector;
using DotNetEnv;

namespace NssOrderTool.Services
{
    public class DbManager
    {
        // 接続文字列を保持するプライベート変数
        private string? _connectionString;

        public DbManager()
        {
            // .envファイルをロード
            Env.Load();

            // 接続文字列を構築
            _connectionString = BuildConnectionString();
        }

        private string BuildConnectionString()
        {
            string host = Env.GetString("DB_HOST");
            string port = Env.GetString("DB_PORT", "3306");
            string database = Env.GetString("DB_NAME", "order_ranking_db");
            string user = Env.GetString("DB_USER");
            string password = Env.GetString("DB_PASSWORD");

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("DB接続情報が .env に正しく設定されていません。");
            }

            // ConnectionStringの構築
            var builder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = uint.Parse(port),
                Database = database,
                UserID = user,
                Password = password,
                // 【重要】文字化け防止
                CharacterSet = "utf8mb4",
                // 【重要】接続プーリング有効化（都度Open/Closeしても効率的になります）
                Pooling = true,
                // 【重要】サーバーとの通信が切れた場合の自動再接続はオフにし、アプリ側で制御する
                // (Pooling=trueなら、Open時に壊れた接続は自動で破棄され新しいのが作られます)
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// 新しいデータベース接続を開いて返します。
        /// 使用後は必ず Dispose (using文など) してください。
        /// </summary>
        public MySqlConnection GetConnection()
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("接続文字列が初期化されていません。");
            }

            var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // 【重要】セッションのタイムゾーンを日本時間 (JST) に設定
            // これにより、NOW() などで保存される時間が日本時間になります
            using (var cmd = new MySqlCommand("SET time_zone = '+09:00';", connection))
            {
                cmd.ExecuteNonQuery();
            }

            return connection;
        }
    }
}