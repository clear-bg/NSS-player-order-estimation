using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class ArenaView : UserControl
  {
    public ArenaView()
    {
      InitializeComponent();

      // デザインモードでなければ、DIコンテナからViewModelを取得してセットする
      if (!Design.IsDesignMode)
      {
        DataContext = App.Services.GetRequiredService<ArenaViewModel>();
      }
    }
  }
}
