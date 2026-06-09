using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NssOrderTool.Models;
using NssOrderTool.Repositories;

namespace NssOrderTool.ViewModels
{
  public class UserMasterViewModel : ViewModelBase
  {
    private readonly PlayerRepository _playerRepo;
    private readonly AliasRepository _aliasRepo;

    private string _inputUserName = "";
    public string InputUserName
    {
      get => _inputUserName;
      set => SetProperty(ref _inputUserName, value);
    }

    private string _inputAliasName = "";
    public string InputAliasName
    {
      get => _inputAliasName;
      set => SetProperty(ref _inputAliasName, value);
    }

    private bool _isEditMode = false;
    public bool IsEditMode
    {
      get => _isEditMode;
      set => SetProperty(ref _isEditMode, value);
    }

    public ObservableCollection<AliasGroupItem> UserList { get; } = new();

    // DI用のコンストラクタ
    public UserMasterViewModel(PlayerRepository playerRepo, AliasRepository aliasRepo)
    {
      _playerRepo = playerRepo;
      _aliasRepo = aliasRepo;

      _ = LoadDataAsync();
    }

    public virtual async Task SaveAsync()
    {
      var playerName = InputUserName?.Trim();
      var aliasName = InputAliasName?.Trim();

      // 正規名が空の場合は処理しない
      if (string.IsNullOrEmpty(playerName)) return;

      // 1. プレイヤーの登録または更新 (UUID付きのエンティティが返ってくる)
      var player = await _playerRepo.UpsertPlayerAsync(playerName);

      // 2. エイリアスの入力があれば、そのプレイヤーのUUIDに紐付けて登録
      if (!string.IsNullOrEmpty(aliasName))
      {
        await _aliasRepo.UpsertAliasAsync(aliasName, player.Id);
      }

      // 3. 入力欄をクリアして、一覧を再読み込み
      InputUserName = "";
      InputAliasName = "";
      await LoadDataAsync();
    }

    public virtual async Task DeleteUserAsync(string targetPlayerId)
    {
      await _playerRepo.SoftDeletePlayerAsync(targetPlayerId);
      await _aliasRepo.SoftDeleteAliasesByPlayerIdAsync(targetPlayerId);
      await LoadDataAsync();
    }

    public virtual async Task DeleteAliasAsync(string aliasName)
    {
      await _aliasRepo.DeleteAliasAsync(aliasName);
      await LoadDataAsync();
    }

    public virtual async Task LoadDataAsync()
    {
      UserList.Clear();

      // 削除されていないプレイヤーとエイリアスをすべて取得
      var allPlayers = await _playerRepo.GetAllPlayersAsync();
      var activePlayers = allPlayers.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
      var activeAliases = await _aliasRepo.GetAliasDictionaryAsync(); // 内部でIsDeleted対応が必要な場合は後で調整

      foreach (var p in activePlayers)
      {
        // このプレイヤーUUIDに紐づくエイリアス名を抽出
        var pAliases = activeAliases
            .Where(kvp => kvp.Value == p.Id)
            .Select(kvp => kvp.Key)
            .ToList();

        UserList.Add(new AliasGroupItem
        {
          TargetPlayerId = p.Id,
          TargetName = p.Name, // 画面表示用
          Aliases = pAliases
        });
      }
    }
  }
}
