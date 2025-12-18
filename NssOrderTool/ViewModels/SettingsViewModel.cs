using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using DotNetEnv;
using Avalonia.Media;
using NssOrderTool.Database;
using NssOrderTool.Models;
using NssOrderTool.Services.Infrastructure;

namespace NssOrderTool.ViewModels
{
  public partial class SettingsViewModel : ViewModelBase
  {
    private readonly AppConfig _currentConfig;
    private readonly ILogger<SettingsViewModel> _logger;
    // テスト接続用にLoggerFactoryが必要（SsmTunnelService生成用）
    private readonly ILoggerFactory _loggerFactory;

    // --- 設定プロパティ ---

    [ObservableProperty]
    private string _environment = "TEST";
    public string[] EnvironmentList { get; } = { "TEST", "PROD" };

    [ObservableProperty]
    private bool _useSsm;

    [ObservableProperty]
    private string _instanceId = "";

    [ObservableProperty]
    private string _remoteHost = "";

    [ObservableProperty]
    private int _remotePort = 3306;

    [ObservableProperty]
    private int _localPort = 3306;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    private bool _isSuccess; // メッセージの色分け用
    public IBrush StatusBrush => IsSuccess ? Brushes.Green : Brushes.Red;

    public SettingsViewModel(AppConfig config, ILogger<SettingsViewModel> logger, ILoggerFactory loggerFactory)
    {
      _currentConfig = config;
      _logger = logger;
      _loggerFactory = loggerFactory;

      LoadSettings();
    }

    // デザイナー用
    public SettingsViewModel()
    {
      _currentConfig = new AppConfig();
      _logger = null!;
      _loggerFactory = null!;
    }

    private void LoadSettings()
    {
      // 現在のメモリ上の設定をUIに反映
      var env = _currentConfig.AppSettings?.Environment;

      if (env != "TEST" && env != "PROD")
      {
        env = "TEST";
      }
      Environment = env;

      if (_currentConfig.SsmSettings != null)
      {
        UseSsm = _currentConfig.SsmSettings.UseSsm;
        InstanceId = _currentConfig.SsmSettings.InstanceId;
        RemoteHost = _currentConfig.SsmSettings.RemoteHost;
        RemotePort = _currentConfig.SsmSettings.RemotePort;
        LocalPort = _currentConfig.SsmSettings.LocalPort;
      }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
      if (IsBusy) return;
      IsBusy = true;
      try
      {
        // 1. 新しい設定オブジェクトを作成
        var newConfig = new AppConfig
        {
          AppSettings = new AppSettings { Environment = Environment },
          SsmSettings = new SsmSettings
          {
            UseSsm = UseSsm,
            InstanceId = InstanceId,
            RemoteHost = RemoteHost,
            RemotePort = RemotePort,
            LocalPort = LocalPort
          }
        };

        // 2. JSONにシリアライズして保存
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(newConfig, jsonOptions);

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        await File.WriteAllTextAsync(path, jsonString);

        StatusMessage = "✅ 設定を保存しました。反映にはアプリの再起動が必要です。";
        IsSuccess = true;
      }
      catch (Exception ex)
      {
        StatusMessage = $"❌ 保存エラー: {ex.Message}";
        IsSuccess = false;
        _logger.LogError(ex, "設定の保存に失敗しました");
      }
      finally
      {
        IsBusy = false;
      }
    }

    [RelayCommand]
    private void RestartApplication()
    {
      try
      {
        // 1. 現在の実行ファイルのパスを取得
        var processModule = Process.GetCurrentProcess().MainModule;
        var fileName = processModule?.FileName;

        if (!string.IsNullOrEmpty(fileName))
        {
          // 2. 新しいプロセスとして自分自身を起動
          Process.Start(fileName);

          // 3. 現在のアプリを終了
          System.Environment.Exit(0);
        }
      }
      catch (Exception ex)
      {
        StatusMessage = $"❌ 再起動に失敗しました: {ex.Message}";
        IsSuccess = false;
      }
    }

    [RelayCommand]
    private async Task TestConnection()
    {
      if (IsBusy) return;
      IsBusy = true;
      StatusMessage = "🔄 接続テスト中...";
      IsSuccess = false;

      SsmTunnelService? tempSsm = null;

      try
      {
        // 1. 入力値から一時的な設定を作成
        var tempConfig = new AppConfig
        {
          AppSettings = new AppSettings { Environment = Environment },
          SsmSettings = new SsmSettings
          {
            UseSsm = UseSsm,
            InstanceId = InstanceId,
            RemoteHost = RemoteHost,
            RemotePort = RemotePort,
            LocalPort = LocalPort
          }
        };

        // 2. 必要ならSSMトンネル開始
        if (tempConfig.SsmSettings.UseSsm)
        {
          var ssmLogger = _loggerFactory.CreateLogger<SsmTunnelService>();
          tempSsm = new SsmTunnelService(tempConfig, ssmLogger);
          await tempSsm.StartAsync();
        }

        // 3. 接続文字列の構築 (Envは既存のものを利用)
        string dbNameKey = (Environment == "PROD") ? "DB_NAME_PROD" : "DB_NAME_TEST";
        string databaseName = Env.GetString(dbNameKey);

        var builder = new MySqlConnectionStringBuilder
        {
          Server = tempConfig.SsmSettings.UseSsm ? "127.0.0.1" : Env.GetString("DB_HOST"),
          Port = tempConfig.SsmSettings.UseSsm ? (uint)tempConfig.SsmSettings.LocalPort : uint.Parse(Env.GetString("DB_PORT", "3306")),
          UserID = Env.GetString("DB_USER"),
          Password = Env.GetString("DB_PASSWORD"),
          Database = databaseName,
          ConnectionTimeout = 5 // テストなので短めに
        };

        // 4. 接続試行
        using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        StatusMessage = $"✅ 接続成功！ (DB: {databaseName})";
        IsSuccess = true;
      }
      catch (Exception ex)
      {
        StatusMessage = $"❌ 接続失敗: {ex.Message}";
        IsSuccess = false;
      }
      finally
      {
        // トンネルを閉じる
        tempSsm?.Dispose();
        IsBusy = false;
      }
    }
  }
}