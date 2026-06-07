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
using Avalonia.Media;
using NssOrderTool.Messages;
using NssOrderTool.Models;
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

    // --- 状態管理 ---
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isSuccess;

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

      WeakReferenceMessenger.Default.Register(this);

      LoadSettings();
    }

    private void LoadSettings()
    {
      try
      {
        Environment = _currentConfig.AppSettings?.Environment ?? "TEST";
        _logger.LogInformation("設定を読み込みました。");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "設定の読み込み中にエラーが発生しました。");
        StatusMessage = "❌ 設定の読み込みに失敗しました。";
        IsSuccess = false;
      }
    }

    public async Task LoadDataAsync()
    {
      try
      {
        var players = await _playerRepository.GetAllPlayersAsync();
        PlayerList.Clear();
        foreach (var p in players)
        {
          PlayerList.Add(p);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "データの読み込みに失敗しました。");
      }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
      IsBusy = true;
      StatusMessage = "⏳ 接続テスト中...";
      IsSuccess = false;

      try
      {
        // 画面で選択されている環境(Environment)に応じてテスト先ファイル名を決定
        string dbFileName = (Environment == "PROD" || Environment == "PRODUCTION")
                            ? "local_db_prod.db"
                            : "local_db_test.db";

        // 動的に決定したファイル名で接続テスト
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbFileName}");
        await connection.OpenAsync();

        StatusMessage = $"✅ 接続成功！ (DB: {dbFileName})";
        IsSuccess = true;
      }
      catch (Exception ex)
      {
        StatusMessage = $"❌ 接続失敗: {ex.Message}";
        IsSuccess = false;
        _logger.LogError(ex, "DB接続テストに失敗しました。");
      }
      finally
      {
        IsBusy = false;
        OnPropertyChanged(nameof(StatusBrush));
      }
    }

    [RelayCommand]
    private void SaveSettings()
    {
      try
      {
        _currentConfig.AppSettings ??= new AppSettings();
        _currentConfig.AppSettings.Environment = Environment;

        string json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions { WriteIndented = true });

        // 1. 実行ファイルと同じディレクトリ (再起動時に確実に読み込まれる場所)
        string exeConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        File.WriteAllText(exeConfigPath, json);

        // 2. カレントディレクトリ (開発時のプロジェクトルート) にファイルがあればそちらも更新
        string currentDirConfigPath = Path.Combine(System.Environment.CurrentDirectory, "appsettings.json");
        if (exeConfigPath != currentDirConfigPath && File.Exists(currentDirConfigPath))
        {
          File.WriteAllText(currentDirConfigPath, json);
        }

        StatusMessage = "💾 設定を保存しました。(アプリを再起動して反映してください)";
        IsSuccess = true;
      }
      catch (Exception ex)
      {
        StatusMessage = $"❌ 保存失敗: {ex.Message}";
        IsSuccess = false;
      }
      finally
      {
        OnPropertyChanged(nameof(StatusBrush));
      }
    }

    [RelayCommand]
    private void RestartApplication()
    {
      var processModule = Process.GetCurrentProcess().MainModule;
      if (processModule != null)
      {
        Process.Start(processModule.FileName);
        System.Environment.Exit(0); // System. を追加して明示的に指定します
      }
    }

    public async void Receive(DatabaseUpdatedMessage message)
    {
      await LoadDataAsync();
    }
  }
}
