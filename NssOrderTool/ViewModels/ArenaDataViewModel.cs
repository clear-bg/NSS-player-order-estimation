using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;
using NssOrderTool.Models.Configuration;
using NssOrderTool.Models.DTOs;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Models.UI;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaDataViewModel : ViewModelBase, IRecipient<DatabaseUpdatedMessage>, IRecipient<TransferToArenaDataMessage>
  {
    private readonly PlayerRepository _playerRepo;
    private readonly ArenaRepository _arenaRepository;
    private readonly SeasonRepository _seasonRepo;
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

    // グラフ用プロパティ
    [ObservableProperty]
    private List<RateHistoryEntity> _rateHistory = new();
    [ObservableProperty]
    private SeasonUIItem? _selectedSeason;

    public ObservableCollection<PlayerEntity> Players { get; } = new();
    public record RankingItem(int Rank, string Name, string RatingText, double RatingValue, string Id);
    public ObservableCollection<RankingItem> TopRanking { get; } = new();
    public ObservableCollection<SeasonUIItem> Seasons { get; } = new();

    // デザイン用
    public ArenaDataViewModel()
    {
      _playerRepo = null!;
      _arenaRepository = null!;
      _seasonRepo = null!;
      _appConfig = null!;
    }

    // 本番用 (DI)
    public ArenaDataViewModel(PlayerRepository playerRepo, ArenaRepository arenaRepository, SeasonRepository seasonRepo, AppConfig appConfig)
    {
      _playerRepo = playerRepo;
      _arenaRepository = arenaRepository;
      _seasonRepo = seasonRepo;
      _appConfig = appConfig;

      WeakReferenceMessenger.Default.RegisterAll(this);

      _ = InitializeSeasonsAsync();
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
      if (_arenaRepository == null || SelectedSeason == null) return;

      IsLoadingDetails = true;
      DisplayRating = "Loading...";

      RateHistory = new List<RateHistoryEntity>();

      try
      {
        // 1. 詳細データ(スタッツ)の取得
        var data = await Task.Run(() => _arenaRepository.GetPlayerDetailsAsync(playerId, SelectedSeason.Entity.Id));
        Details = data;

        // 2. 最新レート情報の取得
        if (_playerRepo != null)
        {
          var seasonRating = await _playerRepo.GetPlayerSeasonRatingAsync(playerId, SelectedSeason.Entity.Id);
          if (seasonRating != null)
          {
            DisplayRating = seasonRating.RateMean.ToString("F0");
          }
          else
          {
            DisplayRating = "New";
          }
        }

        var history = await _arenaRepository.GetRateHistoryAsync(playerId);
        RateHistory = history.OrderBy(h => h.RecordedAt).ToList();

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
        await LoadRankingAsync();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading players: {ex.Message}");
      }
    }

    private async Task LoadRankingAsync()
    {
      if (_playerRepo == null) return;

      // Repository側で RateMean順 になっているので、そのまま表示するだけでOK
      var topPlayers = await _playerRepo.GetTopRatedPlayersBySeasonAsync(20, SelectedSeason!.Entity.Id);
      TopRanking.Clear();

      int rank = 1;
      foreach (var p in topPlayers)
      {
        string rateText = p.RateMean.ToString("F0");

        TopRanking.Add(new RankingItem(rank++, p.Player?.Name ?? "Unknown", rateText, p.RateMean, p.PlayerId));
      }
    }

    public void Receive(DatabaseUpdatedMessage message)
    {
      // UIスレッドをブロックしないように再読み込みを実行
      _ = ReloadAllAsync();
    }

    public void Receive(TransferToArenaDataMessage message)
    {
      var targetPlayerId = message.Value;
      var target = Players.FirstOrDefault(p => p.Id == targetPlayerId);

      if (target != null)
      {
        SelectedPlayer = target; // これにより自動的に詳細データのロード(LoadDetailsAsync)が走ります
      }
    }

    // 全データを最新の状態にリフレッシュする
    private async Task ReloadAllAsync()
    {
      // 1. プレイヤーリストとランキングの更新
      await LoadPlayersAsync();

      // 2. もし誰かの詳細を開いているなら、その詳細情報も更新する
      if (SelectedPlayer != null)
      {
        // LoadDetailsAsync は async void なので、メソッド内で直接呼び出し
        // (本来は Task を返す形にリファクタリング推奨ですが、現状はこれで動きます)
        LoadDetailsAsync(SelectedPlayer.Id);
      }
    }

    private async Task InitializeSeasonsAsync()
    {
      var seasons = await _seasonRepo.GetAllSeasonsAsync();

      Seasons.Clear();
      foreach (var s in seasons)
      {
        Seasons.Add(new SeasonUIItem(s));
      }

      var active = await _seasonRepo.GetActiveSeasonAsync();

      if (Seasons.Any())
      {
        SelectedSeason = active != null
            ? Seasons.FirstOrDefault(s => s.Entity.Id == active.Id)
            : Seasons.LastOrDefault();
      }

      await LoadPlayersAsync();
    }

    partial void OnSelectedSeasonChanged(SeasonUIItem? value)
    {
      if (value != null)
      {
        _ = LoadRankingAsync(); // ランキングを再読み込み
        if (SelectedPlayer != null)
        {
          LoadDetailsAsync(SelectedPlayer.Id); // 選択中プレイヤーがいれば詳細も再計算
        }
      }
    }
  }
}
