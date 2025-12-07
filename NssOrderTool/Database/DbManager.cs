using DotNetEnv;
using MySqlConnector;
using NssOrderTool.Models;
using System;
using System.Data;
using System.IO;          // 追加: ファイル読み込み用
using System.Text.Json;   // 追加: JSONパース用
using System.Threading.Tasks;

namespace NssOrderTool.Database
{
    public class DbManager
    {
        private string? _connectionString;

        // 外部（GUI）から「今どっちの環境？」を知るためのプロパティ
        public string CurrentEnvironment { get; private set; } = "UNKNOWN";

        public DbManager()
        {
            // .envファイルをロード
            Env.Load();

            // 接続文字列を構築
            _connectionString = BuildConnectionString();
        }

        private string BuildConnectionString()
        {
            // 1. .env から機密情報を取得
            string host = Env.GetString("DB_HOST");
            string port = Env.GetString("DB_PORT", "3306");
            string user = Env.GetString("DB_USER");
            string password = Env.GetString("DB_PASSWORD");

            // --- ▼ 変更: 環境設定を appsettings.json から読み込む ▼ ---
            string env = "TEST"; // 読み込めなかった場合のフォールバック（デフォルト）

            try
            {
                // 実行フォルダにある appsettings.json を読み込む
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    var jsonString = File.ReadAllText(jsonPath);
                    // AppConfigクラスは Services/AppSettings.cs で定義されている前提です
                    var config = JsonSerializer.Deserialize<AppConfig>(jsonString);

                    if (config?.AppSettings?.Environment != null)
                    {
                        env = config.AppSettings.Environment.ToUpper();
                    }
                }
            }
            catch (Exception ex)
            {
                // エラー時はログに出すなどしても良いが、とりあえずデフォルト(TEST)で進める
                System.Diagnostics.Debug.WriteLine($"設定ファイル読み込みエラー: {ex.Message}");
            }
            // --- ▲ 変更ここまで ▲ ---

            string database;
            if (env == "PROD" || env == "PRODUCTION")
            {
                CurrentEnvironment = "PROD";
                database = Env.GetString("DB_NAME_PROD");
            }
            else
            {
                CurrentEnvironment = "TEST";
                database = Env.GetString("DB_NAME_TEST");
            }

            // DB名が設定されていなかった場合のガード
            if (string.IsNullOrEmpty(database))
            {
                throw new InvalidOperationException($"環境 '{env}' 用のデータベース名が .env に設定されていません。");
            }

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
                // 【重要】接続プーリング有効化
                Pooling = true,
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// 同期でデータベース接続を開いて返します。
        /// </summary>

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("接続文字列が初期化されていません。");
            }

            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(); // 非同期でオープン

            // タイムゾーン設定も非同期で
            using (var cmd = new MySqlCommand("SET time_zone = '+09:00';", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            return connection;
        }

        public MySqlConnection GetConnection()
        {
            if (_connectionString == null) throw new InvalidOperationException("接続文字列エラー");

            var connection = new MySqlConnection(_connectionString);
            connection.Open(); // 同期オープン

            using (var cmd = new MySqlCommand("SET time_zone = '+09:00';", connection))
            {
                cmd.ExecuteNonQuery(); // 同期実行
            }
            return connection;
        }
    }
}