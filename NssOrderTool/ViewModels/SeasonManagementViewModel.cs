using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;
using NssOrderTool.Models.Entities;
using NssOrderTool.Models.UI;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class SeasonManagementViewModel : ViewModelBase
  {
    private readonly SeasonRepository _seasonRepo;

    public ObservableCollection<SeasonUIItem> Seasons { get; } = new();

    [ObservableProperty]
    private SeasonUIItem? _activeSeason;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isEditDialogOpen;

    [ObservableProperty]
    private string _editSeasonName = "";

    [ObservableProperty]
    private DateTime? _editStartDate;

    [ObservableProperty]
    private DateTime? _editEndDate;

    [ObservableProperty]
    private bool _isDeleteDialogOpen;

    [ObservableProperty]
    private string _deleteConfirmMessage = "";

    private SeasonEntity? _editingSeasonEntity;
    private SeasonEntity? _deletingSeasonEntity;

    public SeasonManagementViewModel(SeasonRepository seasonRepo)
    {
      _seasonRepo = seasonRepo;
    }

    // 初期読み込みと「Season1」の自動作成ロジック
    public async Task InitializeAsync()
    {
      await LoadSeasonsAsync();
    }

    [RelayCommand]
    private async Task LoadSeasonsAsync()
    {
      // Repository は開始日の降順（一番新しいものが最初）で返してくる
      var seasons = await _seasonRepo.GetAllSeasonsAsync();
      Seasons.Clear();

      // リストの先頭が「一番新しいシーズン」
      var latestSeason = seasons.FirstOrDefault();

      foreach (var s in seasons)
      {
        var item = new SeasonUIItem(s);

        item.CanDelete = (s == latestSeason && !s.IsActive);

        Seasons.Add(item);
      }
      ActiveSeason = Seasons.FirstOrDefault(s => s.IsActive);
    }

    [RelayCommand]
    private async Task CreateNewSeasonAsync()
    {
      int nextNumber = Seasons.Count + 1;
      var newSeason = new SeasonEntity
      {
        Name = $"Season{nextNumber}",
        IsActive = true
      };

      if (ActiveSeason != null)
      {
        ActiveSeason.Entity.IsActive = false;

        // ★ 修正: 旧シーズンの開始日と「昨日」を比較し、矛盾しない方（遅い方）を基準日とする
        var baseDate = ActiveSeason.Entity.StartDate.Date >= DateTime.Now.Date
                       ? ActiveSeason.Entity.StartDate.Date
                       : DateTime.Now.Date.AddDays(-1);

        // 旧シーズンの終了日をセット
        ActiveSeason.Entity.EndDate = _seasonRepo.NormalizeEndDate(baseDate);
        await _seasonRepo.UpdateSeasonAsync(ActiveSeason.Entity);

        // 新シーズンの開始日は、絶対に「旧シーズンの終了日の翌日」にする
        newSeason.StartDate = _seasonRepo.NormalizeStartDate(baseDate.AddDays(1));
      }
      else
      {
        // 最初のSeason1作成時は「今日」から開始
        newSeason.StartDate = _seasonRepo.NormalizeStartDate(DateTime.Now);
      }

      await _seasonRepo.AddSeasonAsync(newSeason);
      await LoadSeasonsAsync();
      StatusMessage = $"{newSeason.Name} を開始しました。";

      WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());
      WeakReferenceMessenger.Default.Send(new ActiveSeasonChangedMessage());
    }

    [RelayCommand]
    private async Task SetActiveSeasonAsync(SeasonUIItem? targetUIItem)
    {
      if (targetUIItem == null || targetUIItem.IsActive) return;

      var targetSeason = targetUIItem.Entity;

      if (ActiveSeason != null)
      {
        ActiveSeason.Entity.IsActive = false;
        await _seasonRepo.UpdateSeasonAsync(ActiveSeason.Entity);
      }

      targetSeason.IsActive = true;
      await _seasonRepo.UpdateSeasonAsync(targetSeason);

      await LoadSeasonsAsync();
      StatusMessage = $"{targetSeason.Name} をアクティブに変更しました。";

      WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());
      WeakReferenceMessenger.Default.Send(new ActiveSeasonChangedMessage());
    }

    // 1. 編集ボタンを押した時に呼ばれる（ダイアログを開き、データをセットする）
    [RelayCommand]
    private void OpenEditDialog(SeasonUIItem target)
    {
      _editingSeasonEntity = target.Entity;
      EditSeasonName = target.Entity.Name;
      EditStartDate = target.Entity.StartDate;
      EditEndDate = target.Entity.EndDate;
      StatusMessage = "";
      IsEditDialogOpen = true;
    }

    // 2. キャンセルボタンを押した時に呼ばれる
    [RelayCommand]
    private void CloseEditDialog()
    {
      IsEditDialogOpen = false;
      _editingSeasonEntity = null;
    }

    // 3. 保存ボタンを押した時に呼ばれる
    [RelayCommand]
    private async Task SaveEditAsync()
    {
      if (_editingSeasonEntity == null || !EditStartDate.HasValue) return;

      // バリデーション 1: 開始日と終了日の前後関係
      if (EditEndDate.HasValue && EditEndDate.Value < EditStartDate.Value)
      {
        StatusMessage = "⚠️ 終了日は開始日より後の日付を指定してください。";
        return;
      }

      // Step1で作ったロジックを使うための一時エンティティ
      var tempEntity = new SeasonEntity
      {
        Id = _editingSeasonEntity.Id,
        StartDate = _seasonRepo.NormalizeStartDate(EditStartDate.Value),
        EndDate = EditEndDate.HasValue ? _seasonRepo.NormalizeEndDate(EditEndDate.Value) : null
      };

      // バリデーション 2: 重複チェック
      if (await _seasonRepo.IsSeasonOverlappingAsync(tempEntity))
      {
        StatusMessage = "⚠️ 指定された期間は他のシーズンと重複しています。";
        return;
      }

      // チェックを全て通過したら実データに反映してDB保存
      _editingSeasonEntity.Name = EditSeasonName;
      _editingSeasonEntity.StartDate = tempEntity.StartDate;
      _editingSeasonEntity.EndDate = tempEntity.EndDate;

      await _seasonRepo.UpdateSeasonAsync(_editingSeasonEntity);

      // UIの更新と通知
      await LoadSeasonsAsync();
      WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());
      WeakReferenceMessenger.Default.Send(new ActiveSeasonChangedMessage());

      StatusMessage = $"{_editingSeasonEntity.Name} の期間を更新しました。";
      CloseEditDialog(); // 最後にダイアログを閉じる
    }

    // 1. 削除ボタンを押した時に呼ばれる（確認ダイアログを開く）
    [RelayCommand]
    private void ConfirmDelete(SeasonUIItem target)
    {
      _deletingSeasonEntity = target.Entity;
      DeleteConfirmMessage = $"本当に '{target.Name}' を削除しますか？\nこの操作は取り消せません。";
      IsDeleteDialogOpen = true;
    }

    // 2. キャンセルボタンを押した時に呼ばれる
    [RelayCommand]
    private void CancelDelete()
    {
      IsDeleteDialogOpen = false;
      _deletingSeasonEntity = null;
    }

    // 3. 削除実行ボタンを押した時に呼ばれる
    [RelayCommand]
    private async Task ExecuteDeleteAsync()
    {
      if (_deletingSeasonEntity == null) return;

      string deletedName = _deletingSeasonEntity.Name;

      // DBから削除
      await _seasonRepo.DeleteSeasonAsync(_deletingSeasonEntity);

      // UIの更新と通知
      await LoadSeasonsAsync();
      WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());

      StatusMessage = $"{deletedName} を削除しました。";

      // ダイアログを閉じる
      IsDeleteDialogOpen = false;
      _deletingSeasonEntity = null;
    }
  }
}
