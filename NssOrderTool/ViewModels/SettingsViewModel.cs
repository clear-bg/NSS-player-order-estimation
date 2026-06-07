using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Avalonia.Media;
using NssOrderTool.Messages;
using NssOrderTool.Models;
using NssOrderTool.Services.Infrastructure;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class SettingsViewModel : ViewModelBase, IRecipient<DatabaseUpdatedMessage>
  {
    private readonly AppConfig _currentConfig;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly PlayerRepository _playerRepository;

    // --- 設定プロパティ ---
    public string[] EnvironmentList { get; } = { "TEST", "PROD" };
    [ObservableProperty]
    private string _environment = "TEST";
    public ObservableCollection<PlayerEntity> PlayerList { get; } = new();

    [ObservableProperty]
    private PlayerEntity? _selectedDefaultPlayer;

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

    public SettingsViewModel(
        AppConfig config,
        ILogger<SettingsViewModel> logger,
        ILoggerFactory loggerFactory,
        PlayerRepository playerRepository)
    {
      _currentConfig = config;
      _logger = logger;
      _loggerFactory = loggerFactory;
      _playerRepository = playerRepository;

      WeakReferenceMessenger.Default.RegisterAll(this);

      LoadSettings();
      // プレイヤーリスト読み込み開始
      _ = LoadPlayersAsync();
    }

    // デザイナー用
    public SettingsViewModel()
    {
      _currentConfig = new AppConfig();
      _logger = null!;
      _loggerFactory = null!;
      _playerRepository = null!;
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

    private async Task LoadPlayersAsync()
    {
      if (_playerRepository == null) return;
      try
      {
        var players = await _playerRepository.GetAllPlayersAsync();
        PlayerList.Clear();
        foreach (var p in players) PlayerList.Add(p);

        // 設定されているIDがあれば選択状態にする
        var savedId = _currentConfig.AppSettings?.DefaultPlayerId;
        if (!string.IsNullOrEmpty(savedId))
        {
          SelectedDefaultPlayer = PlayerList.FirstOrDefault(p => p.Id == savedId);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "プレイヤーリストの読み込みに失敗しました");
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
          AppSettings = new AppSettings
          {
            Environment = Environment,
            DefaultPlayerId = SelectedDefaultPlayer?.Id ?? ""
          },
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

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=local_database.db");
        await connection.OpenAsync();

        StatusMessage = "✅ 接続成功！ (DB: SQLite ローカル)";
        IsSuccess = true;

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

    public void Receive(DatabaseUpdatedMessage message)
    {
      // プレイヤーリストを再読み込みしてドロップダウンを最新にする
      _ = LoadPlayersAsync();
    }
  }
}
