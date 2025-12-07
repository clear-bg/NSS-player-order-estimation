using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree; // TopLevel検索用
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

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
                DataContext = App.Services.GetRequiredService<OrderEstimationViewModel>();
            }
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
    }
}