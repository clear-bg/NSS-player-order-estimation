using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;
using NssOrderTool.Models.UI;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public partial class PlayerHubViewModel : ViewModelBase
  {
    private readonly PlayerRepository _playerRepo;
    private readonly AliasRepository _aliasRepo;
    private readonly OrderRepository _orderRepo;

    [ObservableProperty]
    private string _targetInput = "";

    [ObservableProperty]
    private string _aliasInput = "";

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private bool _isEditing;
    [ObservableProperty]
    private bool _isShowDeleteConfirm;
    [ObservableProperty]
    private PlayerHubItem? _playerToDelete;

    // ユーザー一覧データ
    public ObservableCollection<PlayerHubItem> PlayerList { get; } = new();

    public PlayerHubViewModel(PlayerRepository playerRepo, AliasRepository aliasRepo, OrderRepository orderRepo)
    {
      _playerRepo = playerRepo;
      _aliasRepo = aliasRepo;
      _orderRepo = orderRepo;

      _ = LoadPlayersAsync();
    }

    // デザイナー用
    public PlayerHubViewModel()
    {
      _playerRepo = null!;
      _aliasRepo = null!;
      _orderRepo = null!;
    }

    [RelayCommand]
    public async Task LoadPlayersAsync()
    {
      try
      {
        var players = await _playerRepo.GetAllPlayersAsync();
        var aliasDict = await _aliasRepo.GetAliasDictionaryAsync(); // Dictionary<Alias, TargetName>

        // TargetName をキーにして、エイリアスのリストをまとめる
        var targetToAliases = aliasDict
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        PlayerList.Clear();
        foreach (var p in players.OrderBy(p => p.Name))
        {
          var aliases = targetToAliases.ContainsKey(p.Name!) ? targetToAliases[p.Name!] : new List<string>();
          var item = new PlayerHubItem
          {
            PlayerId = p.Id,
            Name = p.Name!
          };
          foreach (var a in aliases)
          {
            item.Aliases.Add(a);
          }

          PlayerList.Add(item);
        }
      }
      catch (Exception ex)
      {
        StatusText = $"❌ 読み込みエラー: {ex.Message}";
      }
    }

    [RelayCommand]
    private async Task RegisterPlayerAsync()
    {
      if (string.IsNullOrWhiteSpace(TargetInput))
      {
        StatusText = "⚠️ 正規名（プレイヤー名）を入力してください";
        return;
      }

      IsBusy = true;
      StatusText = "登録処理中...";

      try
      {
        var targetName = TargetInput.Trim();

        // 1. ユーザーの新規登録（すでに存在する場合はそのまま既存のUUIDが返る）
        await _playerRepo.GetOrCreatePlayersAsync(new[] { targetName }, allowCreate: true);

        // 2. エイリアスの処理（入力がある場合のみ）
        if (!string.IsNullOrWhiteSpace(AliasInput))
        {
          var aliasList = AliasInput.Split(',')
                                    .Select(a => a.Trim())
                                    .Where(a => !string.IsNullOrEmpty(a) && !a.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                                    .ToList();

          foreach (var alias in aliasList)
          {
            try
            {
              await _aliasRepo.AddAliasAsync(alias, targetName);
              await _orderRepo.MergePlayerIdsAsync(alias, targetName);
            }
            catch
            {
              // 重複エラー等はスキップ
            }
          }
        }

        StatusText = $"✅ {targetName} の登録・更新が完了しました";
        TargetInput = "";
        AliasInput = "";

        WeakReferenceMessenger.Default.Send(new DatabaseUpdatedMessage());
        await LoadPlayersAsync();
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

    [RelayCommand]
    private void ToggleEditMode()
    {
      // 編集ボタンが押されたら True / False を反転させる
      IsEditing = !IsEditing;
    }

    [RelayCommand]
    private async Task ShowGraphAsync()
    {
      // TODO: 順序グラフを別ウィンドウで表示する処理
      await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GoToArenaDataAsync(PlayerHubItem player)
    {
      if (player == null) return;
      // TODO: アリーナデータの戦績画面へ遷移する処理
      await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeletePlayerAsync(PlayerHubItem player)
    {
      if (player == null) return;

      // 削除対象をセットして、確認ダイアログを表示する
      PlayerToDelete = player;
      IsShowDeleteConfirm = true;

      await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExecuteDeletePlayerAsync()
    {
      if (PlayerToDelete == null) return;

      try
      {
        // ★修正: リポジトリの論理削除メソッドを呼び出す
        await _playerRepo.DeletePlayerAsync(PlayerToDelete.PlayerId);

        // UI（リスト）からも消す
        PlayerList.Remove(PlayerToDelete);
        StatusText = $"🗑️ プレイヤー '{PlayerToDelete.Name}' を削除しました";
      }
      catch (Exception ex)
      {
        StatusText = $"❌ プレイヤー削除エラー: {ex.Message}";
      }
      finally
      {
        // 処理が終わったらダイアログを閉じる
        IsShowDeleteConfirm = false;
        PlayerToDelete = null;
      }
    }

    [RelayCommand]
    private void CancelDeletePlayer()
    {
      // キャンセル時はただダイアログを閉じるだけ
      IsShowDeleteConfirm = false;
      PlayerToDelete = null;
    }

    [RelayCommand]
    private async Task DeleteAliasAsync(string alias)
    {
      if (string.IsNullOrEmpty(alias)) return;

      try
      {
        // エイリアスは「即消し」方針なので、ここでリポジトリを呼んで即削除します
        await _aliasRepo.DeleteAliasAsync(alias);
        StatusText = $"🗑️ エイリアス '{alias}' を削除しました";

        // 削除後、リストを再読み込みして画面に反映
        await LoadPlayersAsync();
      }
      catch (Exception ex)
      {
        StatusText = $"❌ エイリアス削除エラー: {ex.Message}";
      }
    }
  }
}
