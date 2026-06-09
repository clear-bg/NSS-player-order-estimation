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
using Serilog;
using NssOrderTool.Services.Domain;
using NssOrderTool.Database;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using NssOrderTool.Views;
using NssOrderTool.Models;
using NssOrderTool.Services.Rating;

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

    collection.AddSingleton(appConfig);

    collection.AddLogging(loggingBuilder =>
    {
      loggingBuilder.AddSerilog(dispose: true);
    });

    // DbSchemaService
    collection.AddTransient<DbSchemaService>();

    // ▼ EF Core (AppDbContext) の登録
    collection.AddDbContext<AppDbContext>(options =>
    {
      string envName = appConfig.AppSettings?.Environment?.ToUpper() ?? "TEST";
      string dbFileName = (envName == "PROD" || envName == "PRODUCTION")
                          ? "local_db_prod.db"
                          : "local_db_test.db";

      // SQLiteのローカルファイルを指定
      options.UseSqlite($"Data Source={dbFileName}");
    }, ServiceLifetime.Transient);

    // Domain Services
    collection.AddTransient<RelationshipExtractor>();
    collection.AddTransient<OrderSorter>();
    collection.AddTransient<GraphVizService>();
    collection.AddTransient<ArenaLogicService>();
    collection.AddSingleton<IRatingCalculator, ScoreBasedRatingCalculator>();

    // Repositories
    collection.AddTransient<OrderRepository>();
    collection.AddTransient<PlayerRepository>();
    collection.AddTransient<AliasRepository>();
    collection.AddTransient<ArenaRepository>();
    collection.AddTransient<ArenaViewModel>();

    // ViewModels
    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<UserMasterViewModel>();
    collection.AddTransient<SettingsViewModel>();
    collection.AddTransient<SimulationViewModel>();
    collection.AddTransient<ArenaDataViewModel>();

    // 3. プロバイダのビルド
    Services = collection.BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
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
            Log.Error("❌ DB接続失敗: 接続できませんでした。");
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
