using System;
using System.Collections.ObjectModel;
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
    }

    // --- 以下、コマンド用のメソッド（中身の実装はステップ4で行います） ---

    public virtual async Task SaveAsync()
    {
      // TODO: 入力チェック、PlayerとAliasのUpsert処理
    }

    public virtual async Task DeleteUserAsync(string targetPlayerId)
    {
      // TODO: ユーザーと紐づくエイリアスの論理削除処理
    }

    public virtual async Task DeleteAliasAsync(string aliasName)
    {
      // TODO: エイリアスの論理削除処理
    }

    public virtual async Task LoadDataAsync()
    {
      // TODO: DBから登録済みユーザーとエイリアスを読み込み、UserListに格納する処理
    }
  }
}
