using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NssOrderTool.Views
{
  public partial class GraphWindow : Window
  {
    public GraphWindow()
    {
      InitializeComponent();
    }

    // ★追加: コピーボタンが押された時の処理
    private async void OnCopyButtonClicked(object? sender, RoutedEventArgs e)
    {
      if (DataContext is ViewModels.GraphWindowViewModel vm)
      {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
          await clipboard.SetTextAsync(vm.GraphText);

          // ボタンのテキストを一時的に変更してフィードバック
          if (sender is Button btn)
          {
            btn.Content = "✅ コピーしました！";
            btn.Background = Avalonia.Media.Brushes.DarkGreen;
          }
        }
      }
    }
  }
}
