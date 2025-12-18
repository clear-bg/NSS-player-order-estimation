using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NssOrderTool.Models;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class AliasSettingsViewModel : ViewModelBase
  {
    private readonly AliasRepository _aliasRepo;
    private readonly OrderRepository _orderRepo;

    // --- Bindings ---

    [ObservableProperty]
    private string _targetInput = "";

    [ObservableProperty]
    private string _aliasInput = "";

    [ObservableProperty]
    private string _statusText = "";

    // 一覧データ
    public ObservableCollection<AliasGroupItem> AliasList { get; } = new();

    public AliasSettingsViewModel(AliasRepository aliasRepo, OrderRepository orderRepo)
    {
      _aliasRepo = aliasRepo;
      _orderRepo = orderRepo;
      _ = LoadAliases();
    }

    // デザイナー用
    public AliasSettingsViewModel()
    {
      _aliasRepo = null!;
      _orderRepo = null!;
    }

    // --- Commands ---

    [RelayCommand]
    public async Task LoadAliases()
    {
      try
      {
        var dict = await _aliasRepo.GetAliasDictionaryAsync();

        var list = dict.GroupBy(kv => kv.Value)
                       .Select(g => new AliasGroupItem
                       {
                         TargetName = g.Key,
                         Aliases = g.Select(kv => kv.Key).OrderBy(a => a).ToList()
                       })
                       .OrderBy(x => x.TargetName)
                       .ToList();

        AliasList.Clear();
        foreach (var item in list)
        {
          AliasList.Add(item);
        }
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 読み込みエラー: {ex.Message}";
      }
    }

    [RelayCommand]
    private async Task AddAlias()
    {
      if (string.IsNullOrWhiteSpace(AliasInput) || string.IsNullOrEmpty(TargetInput))
      {
        StatusText = "⚠️ 両方の名前を入力してください";
        return;
      }

      var aliasList = AliasInput.Split(',')
                                .Select(a => a.Trim())
                                .Where(a => !string.IsNullOrEmpty(a))
                                .ToList();

      if (aliasList.Count == 0) return;

      try
      {
        int successCount = 0;
        List<string> errors = new List<string>();

        foreach (var alias in aliasList)
        {
          // 自分自身へのエイリアス禁止
          if (alias.Equals(TargetInput, StringComparison.OrdinalIgnoreCase))
          {
            errors.Add($"{alias} (正規名と同じ)");
            continue;
          }

          try
          {
            await _aliasRepo.AddAliasAsync(alias, TargetInput);
            await _orderRepo.MergePlayerIdsAsync(alias, TargetInput);
            successCount++;
          }
          catch
          {
            errors.Add($"{alias} (重複など)");
          }
        }

        if (errors.Count == 0)
        {
          StatusText = $"✅ {successCount} 件追加しました";
          AliasInput = "";
          // TargetInput = ""; // 連続登録しやすくするため残す
        }
        else
        {
          StatusText = $"⚠️ {successCount} 件追加, エラー: {string.Join(", ", errors)}";
        }

        // 一覧更新
        await LoadAliases();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ エラー: {ex.Message}";
      }
    }

    // リスト内の「全削除」ボタンから呼ばれる
    [RelayCommand]
    private async Task DeleteGroup(AliasGroupItem group)
    {
      if (group == null) return;

      try
      {
        foreach (var alias in group.Aliases)
        {
          await _aliasRepo.DeleteAliasAsync(alias);
        }
        StatusText = $"🗑️ '{group.TargetName}' のエイリアスを削除しました";

        await LoadAliases();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 削除エラー: {ex.Message}";
      }
    }
  }
}