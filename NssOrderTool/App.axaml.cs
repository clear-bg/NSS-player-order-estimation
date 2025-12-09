using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection; // 追加
using Serilog;
using Microsoft.Extensions.Logging;
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

    public override async void OnFrameworkInitializationCompleted()
    {
        // 1.Serilog の設定(ログの出力先などを定義)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // デバッグレベル以上を出力
            .WriteTo.Console()    // コンソールにも出す
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // 日付ごとにファイル分割
            .CreateLogger();

        // 2. サービスの登録
        var collection = new ServiceCollection();

        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var jsonString = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "{}";
        var appConfig = JsonSerializer.Deserialize<AppConfig>(jsonString) ?? new AppConfig();

        collection.AddSingleton(appConfig);

        collection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true); // Serilogを使うように指示
        });

        // SSM Tunnel Service
        collection.AddSingleton<SsmTunnelService>();

        // Database & Schema
        collection.AddSingleton<DbManager>();      // 設定を持つだけなのでSingletonでOK
        collection.AddTransient<DbSchemaService>();

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
        // AliasEditViewModel はパラメータ(targetName)が必要なのでここには登録せず、
        // 必要な場所で Factory 的に生成するか、ActivatorUtilitiesを使います。

        // 3. プロバイダのビルド
        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            // SSMトンネルの開始
            try
            {
                var ssmService = Services.GetRequiredService<SsmTunnelService>();
                await ssmService.StartAsync(); // トンネル掘削

                // アプリ終了時にSSMを切断するように登録
                desktop.Exit += (sender, args) => ssmService.Dispose();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AWS SSM接続の初期化に失敗しました。");
                // 必要ならここでreturnして起動を中断
            }

            // 起動時のDB接続確認 (DIから取得して実行)
      try
      {
        var db = Services.GetRequiredService<DbManager>();

        using (var conn = await db.GetConnectionAsync())
        {
          Log.Information("✅ DB接続成功！(起動時チェック)");
        }
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "❌ DB接続失敗: アプリ起動時の接続チェックでエラーが発生しました。");
      }
        }

        base.OnFrameworkInitializationCompleted();
    }

    // アプリ終了時にログをフラッシュ（書き込み漏れ防止）
    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.CloseAndFlush();
    }
}