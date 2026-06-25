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

    private SeasonEntity? _editingSeasonEntity;

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
      var seasons = await _seasonRepo.GetAllSeasonsAsync();
      Seasons.Clear();
      foreach (var s in seasons)
      {
        Seasons.Add(new SeasonUIItem(s));
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
        StartDate = _seasonRepo.NormalizeStartDate(DateTime.Now),
        IsActive = true
      };

      // ActiveSeason.Entity に対して更新を行う
      if (ActiveSeason != null)
      {
        ActiveSeason.Entity.IsActive = false;
        ActiveSeason.Entity.EndDate = _seasonRepo.NormalizeEndDate(DateTime.Now);
        await _seasonRepo.UpdateSeasonAsync(ActiveSeason.Entity);
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
  }
}
