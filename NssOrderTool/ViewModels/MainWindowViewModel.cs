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

    public MainWindowViewModel()
    {
      WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(TransferToArenaMessage message)
    {
      SelectedTabIndex = 2;
    }
  }
}
