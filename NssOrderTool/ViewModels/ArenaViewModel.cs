using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Models.Entities;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels.Arena; // 👈 追加

namespace NssOrderTool.ViewModels
{
  public partial class ArenaViewModel : ViewModelBase
  {
    private readonly ArenaRepository _arenaRepo;
    private readonly PlayerRepository _playerRepo;
    private readonly ArenaLogicService _arenaLogic;

    // --- Bindings ---

    public ObservableCollection<ArenaRoundInputItem> RoundInputs { get; } = new();

    // 子ViewModelのコレクション
    public ObservableCollection<ArenaRowViewModel> PlayerRows { get; } = new();

    public ObservableCollection<ArenaSessionEntity> HistoryList { get; } = new();

    public Func<string, Task<bool>>? ShowConfirmDialogAction { get; set; }

    [ObservableProperty]
    private string _statusText = "準備完了";

    public ArenaViewModel(
      ArenaRepository arenaRepo,
      PlayerRepository playerRepo,
      ArenaLogicService arenaLogic)
    {
      _arenaRepo = arenaRepo;
      _playerRepo = playerRepo;
      _arenaLogic = arenaLogic;

      InitializeRounds();
      InitializeMatrix();

      _ = LoadHistoryAsync();
    }

    // デザイナー用
    public ArenaViewModel()
    {
      _arenaRepo = null!;
      _playerRepo = null!;
      _arenaLogic = null!;
      InitializeRounds();
      InitializeMatrix();
    }

    private void InitializeRounds()
    {
      RoundInputs.Clear();
      for (int i = 1; i <= 14; i++)
      {
        var item = new ArenaRoundInputItem { RoundNumber = i };
        // ボタン変更時に再計算をトリガー
        item.PropertyChanged += (s, e) =>
        {
          if (e.PropertyName == nameof(ArenaRoundInputItem.WinningTeam))
          {
            Recalculate();
          }
        };
        RoundInputs.Add(item);
      }
    }

    private void InitializeMatrix()
    {
      PlayerRows.Clear();
      for (int i = 0; i < 8; i++)
      {
        // A, B, C...
        char name = (char)('A' + i);
        PlayerRows.Add(new ArenaRowViewModel(i, name.ToString()));
      }
      Recalculate();
    }

    // 集計処理のメインエントリー
    private void Recalculate()
    {
      if (_arenaLogic == null) return;

      // 1. 各行に更新を依頼 (勝数計算まで)
      foreach (var row in PlayerRows)
      {
        row.UpdateRow(RoundInputs, _arenaLogic);
      }

      // 2. ランク（順位）計算
      // 勝利数が多い順にランク付け (同率は同じランクにする)
      var sortedScores = PlayerRows.Select(p => p.WinCount)
                                   .Distinct()
                                   .OrderByDescending(score => score)
                                   .ToList();

      foreach (var row in PlayerRows)
      {
        row.Rank = PlayerRows.Count(p => p.WinCount > row.WinCount) + 1;
      }
    }

    [RelayCommand]
    private async Task SaveSession()
    {
      if (IsBusy) return;
      IsBusy = true;
      StatusText = "保存中...";

      try
      {
        // 1. プレイヤーID(名前)のリストを抽出
        var playerNames = PlayerRows.Select(p => p.Name).ToList();

        // 2. プレイヤーが存在しないとFKエラーになるため、事前に登録しておく
        await _playerRepo.RegisterPlayersAsync(playerNames);

        // 3. セッション作成
        var session = new ArenaSessionEntity
        {
          CreatedAt = DateTime.Now
        };

        // 参加者情報の作成
        foreach (var row in PlayerRows)
        {
          session.Participants.Add(new ArenaParticipantEntity
          {
            PlayerId = row.Name,       // FK (RegisterPlayersAsyncで登録済み)
            SlotIndex = row.Index,     // 表示順
            WinCount = row.WinCount,   // スナップショット
            Rank = row.Rank            // スナップショット
          });
        }

        // ラウンド情報の作成
        foreach (var input in RoundInputs)
        {
          session.Rounds.Add(new ArenaRoundEntity
          {
            RoundNumber = input.RoundNumber,
            WinningTeam = input.WinningTeam
          });
        }

        await _arenaRepo.AddSessionAsync(session);

        StatusText = "✅ 結果を保存しました";

        await LoadHistoryAsync();

      }
      catch (Exception ex)
      {
        StatusText = $"❌ エラー: {ex.Message}";
      }
      finally
      {
        IsBusy = false;
      }
    }

    public async Task LoadHistoryAsync()
    {
      try
      {
        var sessions = await _arenaRepo.GetAllSessionsAsync();

        HistoryList.Clear();
        foreach (var s in sessions)
        {
          HistoryList.Add(s);
        }
      }
      catch (Exception ex)
      {
        // 読み込み失敗時はログ出力のみにとどめる等
        System.Diagnostics.Debug.WriteLine($"History load failed: {ex.Message}");
      }
    }

    [RelayCommand]
    private async Task DeleteSession(ArenaSessionEntity session)
    {
      if (session == null || IsBusy) return;

      // 確認ダイアログの表示 (Actionが設定されている場合)
      if (ShowConfirmDialogAction != null)
      {
        bool isConfirmed = await ShowConfirmDialogAction("この履歴データを削除しますか？\n(復元できません)");
        if (!isConfirmed) return;
      }

      IsBusy = true;
      StatusText = "削除中...";

      try
      {
        await _arenaRepo.DeleteSessionAsync(session.Id);

        StatusText = "🗑️ 履歴を削除しました";

        // リストから削除 (再読み込みするより高速)
        HistoryList.Remove(session);
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 削除エラー: {ex.Message}";
      }
      finally
      {
        IsBusy = false;
      }
    }
  }
}
