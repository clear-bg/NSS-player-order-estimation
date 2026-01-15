using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class ArenaDataView : UserControl
  {
    public ArenaDataView()
    {
      InitializeComponent();

      if (!Design.IsDesignMode)
      {
        // DIコンテナから ViewModel を取得して設定
        DataContext = App.Services.GetRequiredService<ArenaDataViewModel>();
      }
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
