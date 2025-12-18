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
    Env.Load();

    collection.AddSingleton(appConfig);

    collection.AddLogging(loggingBuilder =>
    {
      loggingBuilder.AddSerilog(dispose: true);
    });

    // SSM Tunnel Service
    collection.AddSingleton<SsmTunnelService>();

    // DbSchemaService
    collection.AddTransient<DbSchemaService>();

    // ▼ EF Core (AppDbContext) の登録
    collection.AddDbContext<AppDbContext>(options =>
    {
      // 接続情報の構築
      var ssm = appConfig.SsmSettings;
      bool useSsm = ssm?.UseSsm == true;

      string envName = appConfig.AppSettings?.Environment?.ToUpper() ?? "TEST";
      string dbNameKey = (envName == "PROD" || envName == "PRODUCTION") ? "DB_NAME_PROD" : "DB_NAME_TEST";
      string databaseName = Env.GetString(dbNameKey);

      var builder = new MySqlConnector.MySqlConnectionStringBuilder
      {
        Server = useSsm ? "127.0.0.1" : Env.GetString("DB_HOST"),
        Port = useSsm ? (uint)(ssm?.LocalPort ?? 3306) : uint.Parse(Env.GetString("DB_PORT", "3306")),
        UserID = Env.GetString("DB_USER"),
        Password = Env.GetString("DB_PASSWORD"),
        Database = databaseName,
        CharacterSet = "utf8mb4",
        Pooling = true
      };

      options.UseMySql(
              builder.ConnectionString,
              ServerVersion.AutoDetect(builder.ConnectionString)
          );
    }, ServiceLifetime.Transient);

    // Domain Services
    collection.AddTransient<RelationshipExtractor>();
    collection.AddTransient<OrderSorter>();
    collection.AddTransient<GraphVizService>();

    // Repositories
    collection.AddTransient<OrderRepository>();
    collection.AddTransient<PlayerRepository>();
    collection.AddTransient<AliasRepository>();
    collection.AddTransient<ArenaRepository>();
    collection.AddTransient<ArenaViewModel>();

    // ViewModels
    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<OrderEstimationViewModel>();
    collection.AddTransient<AliasSettingsViewModel>();
    collection.AddTransient<SettingsViewModel>();
    collection.AddTransient<SimulationViewModel>();

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

      // 起動時のDB接続確認 (EF Core版)
      try
      {
        // AppDbContext は Scoped なので Scope を作って取得する
        using (var scope = Services.CreateScope())
        {
          var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

          // CanConnectAsync で接続確認
          bool canConnect = Task.Run(() => db.Database.CanConnectAsync()).GetAwaiter().GetResult();

          if (canConnect)
          {
            Log.Information("✅ DB接続成功！(起動時チェック - EF Core)");
          }
          else
          {
            Log.Error("❌ DB接続失敗: 接続できませんでした。接続設定やVPN/SSMを確認してください。");
          }
        }
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "❌ DB接続チェック中に例外が発生しました。");
      }
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
  {
    Log.CloseAndFlush();
  }
}
