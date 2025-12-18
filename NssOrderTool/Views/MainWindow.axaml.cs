using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      if (!Design.IsDesignMode)
      {
        // DIからViewModelを取得
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
      }
    }
  }
}