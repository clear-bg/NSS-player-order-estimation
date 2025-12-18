using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.ViewModels
{
  public partial class ViewModelBase : ObservableObject
  {
    [ObservableProperty]
    private bool _isBusy;
  }
}