using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;
using NssOrderTool.Models;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaDataViewModel : ViewModelBase, IRecipient<DataUpdatedMessage>
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
    public record RankingItem(int Rank, string Name, string Rating, string Id);
    public ObservableCollection<RankingItem> TopRanking { get; } = new();

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

      WeakReferenceMessenger.Default.RegisterAll(this);

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

        // 2. 最新レート情報の取得
        if (_playerRepo != null)
        {
          var player = await _playerRepo.GetPlayerAsync(playerId);
          if (player != null)
          {
            DisplayRating = player.RateMean.ToString("F0");
          }
          else
          {
            // 新規プレイヤー（まだDBにない場合）は初期値1500を表示しても良いですが、
            // ここでは "New" または "-" のままでOKです
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
      var topPlayers = await _playerRepo.GetTopRatedPlayersAsync(20);
      TopRanking.Clear();

      int rank = 1;
      foreach (var p in topPlayers)
      {
        string rateText = p.RateMean.ToString("F0");

        // リストに追加
        TopRanking.Add(new RankingItem(rank++, p.Name ?? "Unknown", rateText, p.Id));
      }
    }

    public void Receive(DataUpdatedMessage message)
    {
      // UIスレッドをブロックしないように再読み込みを実行
      _ = ReloadAllAsync();
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
  }
}
