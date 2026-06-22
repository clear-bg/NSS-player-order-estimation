using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;
using NssOrderTool.Models.Entities;
using NssOrderTool.Models.Domain;
using NssOrderTool.Models.UI;
using NssOrderTool.Repositories;
using NssOrderTool.Services.Domain;
using NssOrderTool.ViewModels.Arena;

namespace NssOrderTool.ViewModels
{
  public partial class ArenaViewModel : ViewModelBase,
   IRecipient<TransferToArenaMessage>,
   IRecipient<DatabaseUpdatedMessage>
  {
    private readonly ArenaRepository _arenaRepo;
    private readonly PlayerRepository _playerRepo;
    private readonly ArenaLogicService _arenaLogic;
    private readonly OrderRepository _orderRepo;
    private readonly SeasonRepository _seasonRepo;

    // --- Bindings ---

    public ObservableCollection<ArenaRoundInputItem> RoundInputs { get; } = new();

    // 子ViewModelのコレクション
    public ObservableCollection<ArenaRowViewModel> PlayerRows { get; } = new();

    public ObservableCollection<ArenaSessionDisplayModel> HistoryList { get; } = new();

    public Func<string, Task<bool>>? ShowConfirmDialogAction { get; set; }

    public Action<ArenaSessionEntity>? ShowDetailDialogAction { get; set; }

    [ObservableProperty]
    private string _statusText = "準備完了";

    [ObservableProperty]
    private string _inputDate = DateTime.Now.ToString("yyyyMMdd");

    [ObservableProperty]
    private string _inputTime = DateTime.Now.ToString("HHmm");

    [ObservableProperty]
    private string _newSessionMemo = string.Empty; // 新規保存用のメモ一時退避

    [ObservableProperty]
    private bool _isShowMemoModal;

    [ObservableProperty]
    private string _editingMemoText = string.Empty;

    [ObservableProperty]
    private SeasonEntity? _currentActiveSeason;

    public ObservableCollection<SeasonUIItem> Seasons { get; } = new();

    [ObservableProperty]
    private SeasonUIItem? _selectedHistorySeason;

    private ArenaSessionDisplayModel? _editingDisplayModel;

    public ArenaViewModel(
      ArenaRepository arenaRepo,
      PlayerRepository playerRepo,
      ArenaLogicService arenaLogic,
      OrderRepository orderRepo,
      SeasonRepository seasonRepo)
    {
      _arenaRepo = arenaRepo;
      _playerRepo = playerRepo;
      _arenaLogic = arenaLogic;
      _orderRepo = orderRepo;
      _seasonRepo = seasonRepo;

      InitializeRounds();
      InitializeMatrix();

      WeakReferenceMessenger.Default.RegisterAll(this);

      _ = InitializeSeasonsAsync();
    }

    // デザイナー用
    public ArenaViewModel()
    {
      _arenaRepo = null!;
      _playerRepo = null!;
      _arenaLogic = null!;
      _orderRepo = null!;
      _seasonRepo = null!;
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

      SaveSessionCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveSession()
    {
      if (IsBusy) return;
      IsBusy = true;
      StatusText = "保存中...";

      Console.WriteLine("=== [DEBUG] SaveSession スタート ===");
      Console.WriteLine($"[DEBUG] CurrentActiveSeason は NULLですか？ : {CurrentActiveSeason == null}");
      Console.WriteLine($"[DEBUG] 取得できている SeasonId : {CurrentActiveSeason?.Id ?? 0}");

      try
      {
        if (!DateTime.TryParseExact($"{InputDate}{InputTime}", "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out var parsedSessionDate))
        {
          StatusText = "❌ 保存失敗: 開催日時の形式が正しくありません (日付8桁、時刻4桁で入力してください)";
          IsBusy = false;
          return;
        }

        // 1. プレイヤーID(名前)のリストを抽出
        var playerNames = PlayerRows.Select(p => p.Name).ToList();

        // 2. 新規登録は許可せず、DBに存在するかチェックして UUID の対応表をもらう
        var nameToIdMap = await _playerRepo.GetOrCreatePlayersAsync(playerNames, allowCreate: false);

        // 未登録のプレイヤーを探す
        var missingPlayers = playerNames.Where(n => !string.IsNullOrWhiteSpace(n) && !nameToIdMap.ContainsKey(n)).ToList();
        if (missingPlayers.Any())
        {
          StatusText = $"❌ 保存失敗: 未登録のプレイヤーが含まれています ({string.Join(", ", missingPlayers)})";
          IsBusy = false;
          return; // 保存をストップ
        }

        // 3. セッション作成 (DB保存用データ)
        var session = new ArenaSessionEntity
        {
          CreatedAt = DateTime.Now,
          SessionDate = parsedSessionDate,
          Memo = NewSessionMemo,
          SeasonId = CurrentActiveSeason?.Id ?? 0
        };

        // 参加者情報の作成
        foreach (var row in PlayerRows)
        {
          if (string.IsNullOrWhiteSpace(row.Name)) continue;

          session.Participants.Add(new ArenaParticipantEntity
          {
            PlayerId = nameToIdMap[row.Name], // ★修正: 名前ではなく UUID を保存する
            SlotIndex = row.Index,
            WinCount = row.WinCount,
            Rank = row.Rank
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

        // DBにセッション保存
        await _arenaRepo.AddSessionAsync(session);

        var orderedPlayerIds = PlayerRows
            .Where(r => !string.IsNullOrWhiteSpace(r.Name) && nameToIdMap.ContainsKey(r.Name))
            .OrderBy(r => r.Index)
            .Select(r => nameToIdMap[r.Name])
            .ToList();

        if (orderedPlayerIds.Count >= 2)
        {
          // 1. Observation(履歴ログ)として配列をそのまま保存
          await _orderRepo.AddObservationAsync(orderedPlayerIds);

          // 2. ペア(Predecessor -> Successor)を生成（隣接ペアのみ抽出）
          var pairsToUpdate = new List<OrderPair>();
          for (int i = 1; i < orderedPlayerIds.Count - 1; i++)
          {
            // i番目のスロットの人は、すぐ下の(i+1)番目のスロットの人よりも優先度（ハッシュ等）が高い
            pairsToUpdate.Add(new OrderPair(orderedPlayerIds[i], orderedPlayerIds[i + 1]));
          }

          // 3. ペアの集計(Frequency)を更新
          await _orderRepo.UpdatePairsAsync(pairsToUpdate);
        }

        // 4. 勝利数を集計してレート更新を実行
        StatusText = "レーティング更新中...";

        // IDごとの勝利数カウンターを用意
        var winCounts = new Dictionary<string, int>();
        foreach (var name in playerNames)
        {
          if (!string.IsNullOrWhiteSpace(name))
          {
            winCounts[nameToIdMap[name]] = 0;
          }
        }

        // 全14ラウンドの結果から、実際の勝利数をカウントアップ
        foreach (var round in RoundInputs)
        {
          if (round.WinningTeam == 0) continue; // 勝敗なしはスキップ

          for (int i = 0; i < 8; i++)
          {
            string pid = playerNames[i];
            if (string.IsNullOrWhiteSpace(pid)) continue;

            // そのラウンドで勝ったチームに所属していたら +1
            if (_arenaLogic.IsWinner(round.RoundNumber, i, round.WinningTeam))
            {
              winCounts[nameToIdMap[pid]]++;
            }
          }
        }

        // まとめて計算・更新を実行 (LogicServiceへ)
        await _arenaLogic.UpdateRatingsAsync(winCounts);
        WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());

        StatusText = "✅ 結果を保存し、レートを更新しました";

        NewSessionMemo = string.Empty;

        await LoadHistoryAsync();

      }
      catch (Exception ex)
      {
        StatusText = $"❌ エラー: {ex.Message}";
        System.Diagnostics.Debug.WriteLine($"Save Error: {ex}");
      }
      finally
      {
        IsBusy = false;
      }
    }

    private bool CanSave()
    {
      // RoundInputsが存在し、要素数が14で、かつ全てのWinningTeamが0(未選択)以外であること
      // (WinningTeam: 0=None, 1=Blue, 2=Orange と想定)
      if (RoundInputs == null || RoundInputs.Count < 14) return false;

      return RoundInputs.All(r => r.WinningTeam != 0);
    }

    public async Task LoadHistoryAsync()
    {
      try
      {
        if (SelectedHistorySeason == null) return;

        var sessions = await _arenaRepo.GetAllSessionsAsync(SelectedHistorySeason.Entity.Id);

        HistoryList.Clear();
        foreach (var s in sessions)
        {
          HistoryList.Add(new ArenaSessionDisplayModel(s));
        }
      }
      catch (Exception ex)
      {
        // 読み込み失敗時はログ出力のみにとどめる等
        System.Diagnostics.Debug.WriteLine($"History load failed: {ex.Message}");
      }
    }

    [RelayCommand]
    private async Task DeleteSession(ArenaSessionDisplayModel session)
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

        WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());

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

    [RelayCommand]
    private async Task ShowSessionDetail(ArenaSessionDisplayModel displayModel)
    {
      if (displayModel == null) return;

      // IDを使って、RepositoryからParticipants等を含む完全なEntityを取得する
      var fullEntity = await _arenaRepo.GetSessionDetailAsync(displayModel.Id);

      if (fullEntity != null)
      {
        ShowDetailDialogAction?.Invoke(fullEntity);
      }
    }

    public void Receive(TransferToArenaMessage message)
    {
      var names = message.Value; // List<string>

      // PlayerRows (入力欄) に名前を上書きする
      // ※PlayerRowsの数が8個ある前提で、先頭から順に埋めます
      for (int i = 0; i < PlayerRows.Count; i++)
      {
        if (i < names.Count)
        {
          PlayerRows[i].Name = names[i];
        }
        else
        {
          PlayerRows[i].Name = string.Empty; // 余った欄はクリア
        }
      }

      ResetRounds();
    }

    public void Receive(DatabaseUpdatedMessage message)
    {
      // データ更新通知が来たら、履歴リストをリロードする
      _ = LoadHistoryAsync();
    }

    [RelayCommand]
    private void ResetRounds()
    {
      foreach (var input in RoundInputs)
      {
        input.WinningTeam = 0; // 0 = 未選択
      }
      Recalculate(); // リセット後に計算を再実行して画面に反映
    }

    [RelayCommand]
    private void OpenNewMemo()
    {
      _editingDisplayModel = null; // 新規セッションであることを明示
      EditingMemoText = NewSessionMemo;
      IsShowMemoModal = true;
    }

    [RelayCommand]
    private void OpenEditMemo(ArenaSessionDisplayModel session)
    {
      if (session == null) return;
      _editingDisplayModel = session; // どの履歴を編集しているか保持
      EditingMemoText = session.Memo;
      IsShowMemoModal = true;
    }

    [RelayCommand]
    private async Task SaveMemoAsync()
    {
      if (_editingDisplayModel == null)
      {
        // 新規セッション作成前のメモ一時保存
        NewSessionMemo = EditingMemoText;
        StatusText = "📝 新規セッション用のメモを一時保存しました (※まだ結果は保存されていません)";
      }
      else
      {
        // 既存の履歴のメモ更新
        try
        {
          await _arenaRepo.UpdateSessionMemoAsync(_editingDisplayModel.Id, EditingMemoText);
          _editingDisplayModel.Memo = EditingMemoText; // UI用モデルにも反映
          StatusText = $"📝 履歴のメモを更新しました";
        }
        catch (Exception ex)
        {
          StatusText = $"❌ メモ保存エラー: {ex.Message}";
        }
      }

      IsShowMemoModal = false;
      _editingDisplayModel = null;
    }

    [RelayCommand]
    private void CancelMemo()
    {
      IsShowMemoModal = false;
      _editingDisplayModel = null;
    }

    private async Task InitializeSeasonsAsync()
    {
      var seasons = await _seasonRepo.GetAllSeasonsAsync();
      Seasons.Clear();
      foreach (var s in seasons)
      {
        Seasons.Add(new SeasonUIItem(s));
      }

      // アクティブなシーズンをプロパティに保持
      CurrentActiveSeason = await _seasonRepo.GetActiveSeasonAsync();

      // ドロップダウンの初期選択をアクティブシーズンにする
      if (CurrentActiveSeason != null)
      {
        SelectedHistorySeason = Seasons.FirstOrDefault(s => s.Entity.Id == CurrentActiveSeason.Id);
      }
    }

    partial void OnSelectedHistorySeasonChanged(SeasonUIItem? value)
    {
      if (value != null)
      {
        _ = LoadHistoryAsync();
      }
    }
  }
}
