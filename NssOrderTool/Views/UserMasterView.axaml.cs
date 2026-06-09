using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace NssOrderTool.Views
{
  public partial class UserMasterView : UserControl
  {
    public UserMasterView()
    {
      InitializeComponent();

      if (!Design.IsDesignMode)
      {
        DataContext = App.Services.GetRequiredService<ViewModels.UserMasterViewModel>();
      }
    }
  }
}
