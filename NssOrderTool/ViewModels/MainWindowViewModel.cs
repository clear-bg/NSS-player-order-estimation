using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NssOrderTool.Messages;

namespace NssOrderTool.ViewModels
{
  public partial class MainWindowViewModel : ViewModelBase, IRecipient<TransferToArenaMessage>
  {
    public string Title => "NSS Order Tool";

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // ... (コンストラクタ等)

    public MainWindowViewModel()
    {
      // ★追加: メッセージ受信登録
      WeakReferenceMessenger.Default.Register(this);
    }

    // ★追加: メッセージ受信時の処理
    public void Receive(TransferToArenaMessage message)
    {
      // アリーナ集計タブ（インデックス1と仮定）へ切り替え
      // ※ご自身の環境のタブ順序に合わせて数字を調整してください
      SelectedTabIndex = 1;
    }
  }
}
