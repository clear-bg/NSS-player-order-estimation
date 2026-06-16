using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;

namespace NssOrderTool.ViewModels
{
  // ★修正: IRecipient<TransferToArenaDataMessage> を追加
  public partial class MainWindowViewModel : ViewModelBase, IRecipient<TransferToArenaMessage>, IRecipient<TransferToArenaDataMessage>
  {
    public string Title => "NSS Order Tool";

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public PlayerHubViewModel PlayerHubViewModel { get; }

    public MainWindowViewModel(PlayerHubViewModel playerHubViewModel)
    {
      PlayerHubViewModel = playerHubViewModel;
      WeakReferenceMessenger.Default.RegisterAll(this); // ★修正: RegisterAll に変更
    }

    public MainWindowViewModel()
    {
      PlayerHubViewModel = null!;
    }

    public void Receive(TransferToArenaMessage message)
    {
      // 既存のアリーナ集計タブへの遷移（そのまま）
      SelectedTabIndex = 2;
    }

    // ★追加: 今回作った新しいメッセージを受け取った時の処理
    public void Receive(TransferToArenaDataMessage message)
    {
      // アリーナデータタブ（インデックス 3）へ切り替え
      SelectedTabIndex = 3;
    }
  }
}
