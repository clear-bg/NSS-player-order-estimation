using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public SeasonManagementViewModel(SeasonRepository seasonRepo)
    {
      _seasonRepo = seasonRepo;
    }

    // 初期読み込みと「Season1」の自動作成ロジック
    public async Task InitializeAsync()
    {
      await LoadSeasonsAsync();

      if (!Seasons.Any())
      {
        var defaultSeason = new SeasonEntity
        {
          Name = "Season1",
          StartDate = DateTime.Now,
          IsActive = true
        };
        await _seasonRepo.AddSeasonAsync(defaultSeason);
        await LoadSeasonsAsync();
      }
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
        StartDate = DateTime.Now,
        IsActive = true
      };

      // ActiveSeason.Entity に対して更新を行う
      if (ActiveSeason != null)
      {
        ActiveSeason.Entity.IsActive = false;
        ActiveSeason.Entity.EndDate = DateTime.Now;
        await _seasonRepo.UpdateSeasonAsync(ActiveSeason.Entity);
      }

      await _seasonRepo.AddSeasonAsync(newSeason);
      await LoadSeasonsAsync();
      StatusMessage = $"{newSeason.Name} を開始しました。";
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
    }
  }
}
