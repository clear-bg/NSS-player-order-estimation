using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
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
    collection.AddTransient<OrderEstimationViewModel>();
    collection.AddTransient<AliasSettingsViewModel>();
    collection.AddTransient<SettingsViewModel>();
    collection.AddTransient<SimulationViewModel>();
    collection.AddTransient<ArenaDataViewModel>();

    // 3. プロバイダのビルド
    Services = collection.BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      var args = desktop.Args ?? Array.Empty<string>();

      // 2. 引数に "reset-db" が含まれているかチェック
      if (args.Contains("reset-db"))
      {
        // 初期化対象を取得 (例: "test", "prod", "all")
        string target = args.Length > 1 ? args[1].ToLower() : "";
        RunDbResetMode(target);

        // 処理が終わったら、メイン画面を立ち上げずにアプリを即終了させる
        desktop.Shutdown();
        return;
      }

      // --- 以下は通常の起動処理 (合言葉がない場合) ---
      desktop.MainWindow = new MainWindow();

      try
      {
        using (var scope = Services.CreateScope())
        {
          var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

          // DBが存在しなければ作成し、未適用のマイグレーションがあれば適用する
          db.Database.Migrate();

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

  /// <summary>
  /// コマンドラインから呼び出されるDB初期化（物理削除）モード
  /// </summary>
  private void RunDbResetMode(string target)
  {
    Console.WriteLine($"\n=== データベース初期化モード ({target}) ===");

    string prodDb = "local_db_prod.db";
    string testDb = "local_db_test.db";

    try
    {
      if (target == "prod" || target == "all")
      {
        if (File.Exists(prodDb))
        {
          File.Delete(prodDb);
          Console.WriteLine("✅ 本番用DB (local_db_prod.db) を削除しました。");
        }
        else
        {
          Console.WriteLine("ℹ️ 本番用DBは見つかりませんでした。");
        }
      }

      if (target == "test" || target == "all")
      {
        if (File.Exists(testDb))
        {
          File.Delete(testDb);
          Console.WriteLine("✅ テスト用DB (local_db_test.db) を削除しました。");
        }
        else
        {
          Console.WriteLine("ℹ️ テスト用DBは見つかりませんでした。");
        }
      }

      Console.WriteLine("※次回のアプリ起動時に、最新の設計図で自動的に再構築(Migrate)されます。");
      Console.WriteLine("=======================================\n");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ 削除中にエラーが発生しました。ファイルが開かれていないか確認してください。\nエラー詳細: {ex.Message}");
    }
  }
}
