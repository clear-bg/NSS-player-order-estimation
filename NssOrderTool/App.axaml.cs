using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Serilog;
using NssOrderTool.Services.Domain;
using NssOrderTool.Services.Infrastructure;
using NssOrderTool.Database;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using NssOrderTool.Views;
using NssOrderTool.Models;

namespace NssOrderTool;

public partial class App : Application
{
    // DIコンテナを公開するプロパティ
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. Serilog の設定
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // 2. サービスの登録開始
        var collection = new ServiceCollection();

        // --- 設定ファイルの読み込み ---
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var jsonString = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "{}";
        var appConfig = JsonSerializer.Deserialize<AppConfig>(jsonString) ?? new AppConfig();

        // --- 環境変数(.env)の読み込み ---
        // パスワード等はここから取得します
        Env.Load();

        collection.AddSingleton(appConfig);

        collection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        // SSM Tunnel Service
        collection.AddSingleton<SsmTunnelService>();

        // 旧来の DbManager (リポジトリ移行が終わるまで併用)
        collection.AddSingleton<DbManager>();
        collection.AddTransient<DbSchemaService>();

        // ▼▼▼ 追加: EF Core (AppDbContext) の登録 ▼▼▼
        collection.AddDbContext<AppDbContext>(options =>
        {
            // 接続情報の構築
            var ssm = appConfig.SsmSettings;
            bool useSsm = ssm?.UseSsm == true;

            // 環境(TEST/PROD)によってDB名を切り替え
            string envName = appConfig.AppSettings?.Environment?.ToUpper() ?? "TEST";
            string dbNameKey = (envName == "PROD" || envName == "PRODUCTION") ? "DB_NAME_PROD" : "DB_NAME_TEST";
            string databaseName = Env.GetString(dbNameKey);

            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                // SSMを使うならローカルホスト、使わないなら.envのホスト
                Server = useSsm ? "127.0.0.1" : Env.GetString("DB_HOST"),
                // SSMを使うならローカルポート、使わないなら.envのポート
                Port = useSsm ? (uint)(ssm?.LocalPort ?? 3306) : uint.Parse(Env.GetString("DB_PORT", "3306")),

                UserID = Env.GetString("DB_USER"),
                Password = Env.GetString("DB_PASSWORD"),
                Database = databaseName,

                CharacterSet = "utf8mb4",
                Pooling = true
            };

            // MySQLプロバイダの使用設定
            options.UseMySql(
                builder.ConnectionString,
                ServerVersion.AutoDetect(builder.ConnectionString)
            );
        });
        // ▲▲▲ 追加ここまで ▲▲▲


        // Domain Services
        collection.AddTransient<RelationshipExtractor>();
        collection.AddTransient<OrderSorter>();

        // Repositories
        collection.AddTransient<OrderRepository>();
        collection.AddTransient<PlayerRepository>();
        collection.AddTransient<AliasRepository>();

        // ViewModels
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<OrderEstimationViewModel>();
        collection.AddTransient<AliasSettingsViewModel>();

        // 3. プロバイダのビルド
        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // SSMトンネルの開始
            try
            {
                var ssmService = Services.GetRequiredService<SsmTunnelService>();
                // デッドロック回避のため Task.Run で実行して待機
                Task.Run(() => ssmService.StartAsync()).GetAwaiter().GetResult();

                desktop.Exit += (sender, args) => ssmService.Dispose();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AWS SSM接続の初期化に失敗しました。");
            }

            desktop.MainWindow = new MainWindow();

            // 起動時のDB接続確認 (まだ既存のDbManagerでチェック)
            try
            {
                var db = Services.GetRequiredService<DbManager>();
                using (var conn = Task.Run(() => db.GetConnectionAsync()).GetAwaiter().GetResult())
                {
                    Log.Information("✅ DB接続成功！(起動時チェック - DbManager)");
                }

                // (オプション) AppDbContextでの接続チェックも試したければここで CanConnectAsync を呼べます
                // using (var scope = Services.CreateScope())
                // {
                //     var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                //     if (ctx.Database.CanConnect()) Log.Information("✅ EF Core 接続成功！");
                // }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ DB接続失敗: アプリ起動時の接続チェックでエラーが発生しました。");
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.CloseAndFlush();
    }
}