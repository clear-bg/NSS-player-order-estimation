using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NssOrderTool.Models; // DTO利用
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaDataViewModel : ViewModelBase
  {
    private readonly PlayerRepository _playerRepo;
    private readonly ArenaRepository _arenaRepository;
    private readonly AppConfig _appConfig;

    // 検索フォーム
    [ObservableProperty]
    private string _searchText = string.Empty;

    // 選択されたプレイヤー
    [ObservableProperty]
    private PlayerEntity? _selectedPlayer;

    // 詳細データ
    [ObservableProperty]
    private PlayerDetailsDto? _details;

    // 読み込み中フラグ
    [ObservableProperty]
    private bool _isLoadingDetails;

    // 画面表示用のレート文字列
    [ObservableProperty]
    private string _displayRating = "-";

    public ObservableCollection<PlayerEntity> Players { get; } = new();

    // デザイン用
    public ArenaDataViewModel()
    {
      _playerRepo = null!;
      _arenaRepository = null!;
      _appConfig = null!;
    }

    // 本番用 (DI)
    public ArenaDataViewModel(PlayerRepository playerRepo, ArenaRepository arenaRepository, AppConfig appConfig)
    {
      _playerRepo = playerRepo;
      _arenaRepository = arenaRepository;
      _appConfig = appConfig;

      _ = LoadPlayersAsync();
    }

    // プレイヤー選択時に呼ばれる
    partial void OnSelectedPlayerChanged(PlayerEntity? value)
    {
      if (value != null)
      {
        // string ID を渡す
        LoadDetailsAsync(value.Id);
      }
      else
      {
        Details = null;
        DisplayRating = "-";
      }
    }

    private async void LoadDetailsAsync(string playerId)
    {
      if (_arenaRepository == null) return;

      IsLoadingDetails = true;
      DisplayRating = "Loading...";

      try
      {
        // 1. 詳細データ(スタッツ)の取得
        var data = await Task.Run(() => _arenaRepository.GetPlayerDetailsAsync(playerId));
        Details = data;

        // 2. 最新レート情報の取得と計算
        if (_playerRepo != null)
        {
          var player = await _playerRepo.GetPlayerAsync(playerId);
          if (player != null)
          {
            // 表示用レート(Conservative Rating) = Mean - 3 * Sigma
            double ordinal = player.RateMean - (3.0 * player.RateSigma);

            // 整数で表示
            DisplayRating = ordinal.ToString("F0");
          }
          else
          {
            DisplayRating = "New";
          }
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading details: {ex.Message}");
        DisplayRating = "Error";
      }
      finally
      {
        IsLoadingDetails = false;
      }
    }

    public async Task LoadPlayersAsync()
    {
      if (_playerRepo == null) return;

      try
      {
        var players = await _playerRepo.GetAllPlayersAsync();
        Players.Clear();
        foreach (var p in players) Players.Add(p);

        var defaultId = _appConfig.AppSettings?.DefaultPlayerId;
        if (!string.IsNullOrEmpty(defaultId))
        {
          var target = Players.FirstOrDefault(p => p.Id == defaultId);
          if (target != null)
          {
            SelectedPlayer = target;
          }
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading players: {ex.Message}");
      }
    }
  }
}
