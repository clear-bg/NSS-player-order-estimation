using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class SeasonManagementView : UserControl
  {
    public SeasonManagementView()
    {
      InitializeComponent();
      if (!Design.IsDesignMode)
      {
        var vm = App.Services.GetRequiredService<SeasonManagementViewModel>();
        DataContext = vm;

        // 画面読み込み時に初期化処理を実行
        Loaded += async (s, e) => await vm.InitializeAsync();
      }
    }
  }
}
