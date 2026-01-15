using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaDataViewModel : ViewModelBase
  {
    private readonly PlayerRepository _playerRepo;

    // 検索フォームの入力値
    [ObservableProperty]
    private string _searchText = string.Empty;

    // 選択されたプレイヤー (ComboBox/ListBoxでの選択状態)
    [ObservableProperty]
    private PlayerEntity? _selectedPlayer;

    // 検索候補となる全プレイヤーリスト
    public ObservableCollection<PlayerEntity> Players { get; } = new();

    // デザインモード用コンストラクタ
    public ArenaDataViewModel()
    {
      _playerRepo = null!;
    }

    // DIコンテナから注入されるコンストラクタ
    public ArenaDataViewModel(PlayerRepository playerRepo)
    {
      _playerRepo = playerRepo;

      // 初期化時にリストを読み込む (Fire-and-forget)
      // ※画面表示時に読み込み直す仕組みを入れることも可能ですが、まずは初期化時のみとします
      _ = LoadPlayersAsync();
    }

    /// <summary>
    /// リポジトリから全プレイヤーを読み込み、リストを更新する
    /// </summary>
    public async Task LoadPlayersAsync()
    {
      if (_playerRepo == null) return;

      try
      {
        var players = await _playerRepo.GetAllPlayersAsync();

        Players.Clear();
        foreach (var p in players)
        {
          Players.Add(p);
        }
      }
      catch (Exception ex)
      {
        // ログ出力など (今回はデバッグ出力のみ)
        System.Diagnostics.Debug.WriteLine($"Error loading players: {ex.Message}");
      }
    }
  }
}
