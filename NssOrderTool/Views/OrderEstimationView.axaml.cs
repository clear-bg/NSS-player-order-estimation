using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NssOrderTool.Views
{
  public partial class OrderEstimationView : UserControl
  {
    public OrderEstimationView()
    {
      InitializeComponent();

      // DIコンテナからViewModelを取得 (依存関係も自動解決される)
      if (!Design.IsDesignMode) // デザイナーモード除外
      {
        var vm = App.Services.GetRequiredService<OrderEstimationViewModel>();
        vm.ConfirmCycleCallback = ShowCycleConfirmationAsync;
        DataContext = vm;
      }
    }

    private async Task<bool> ShowCycleConfirmationAsync(string input, List<string> cyclePath)
    {
      // 親ウィンドウを探す
      var window = TopLevel.GetTopLevel(this) as Window;
      if (window == null) return true; // ウィンドウが見つからない場合はスルーして許可(or false)

      // ダイアログを作成して表示
      var dialog = new CycleConfirmationDialog(input, cyclePath);
      var result = await dialog.ShowDialog<bool>(window);

      return result; // true(登録) or false(キャンセル)
    }

    // ダイアログ表示はViewの仕事
    private async void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
      // ViewModelを取得
      if (DataContext is not OrderEstimationViewModel vm) return;

      // 親ウィンドウを探してダイアログを表示
      var window = TopLevel.GetTopLevel(this) as Window;
      if (window == null) return;

      var dialog = new ConfirmationDialog();
      var result = await dialog.ShowDialog<bool>(window);

      // OKならViewModelの削除処理を実行
      if (result)
      {
        await vm.PerformClearAsync();
      }
    }

    private async void CopyGraphButton_Click(object? sender, RoutedEventArgs e)
    {
      if (DataContext is OrderEstimationViewModel vm)
      {
        // ViewModelにテキストを作らせる
        string graphText = await vm.GenerateGraphTextAsync();

        if (!string.IsNullOrEmpty(graphText))
        {
          // View層(TopLevel)の機能を使ってクリップボードにコピー
          var topLevel = TopLevel.GetTopLevel(this);
          if (topLevel?.Clipboard != null)
          {
            await topLevel.Clipboard.SetTextAsync(graphText);
          }
        }
      }
    }
  }
}
