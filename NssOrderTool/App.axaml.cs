using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection; // 追加

using NssOrderTool.Services.Domain;
using NssOrderTool.Database;
using NssOrderTool.Repositories;
using NssOrderTool.ViewModels;
using NssOrderTool.Views;

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
        // 1. サービスの登録
        var collection = new ServiceCollection();

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

        // 2. プロバイダのビルド
        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            // 起動時のDB接続確認 (DIから取得して実行)
            try
            {
                var db = Services.GetRequiredService<DbManager>();
                using (var conn = db.GetConnection())
                {
                    Debug.WriteLine("✅ DB接続成功！(DI)");
                    Console.WriteLine("✅ DB接続成功！(DI)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DB接続失敗: {ex.Message}");
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}