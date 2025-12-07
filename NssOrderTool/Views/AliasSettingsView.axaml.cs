using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree; // TopLevel取得用
using NssOrderTool.Models;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
    public partial class AliasSettingsView : UserControl
    {
        public AliasSettingsView()
        {
            InitializeComponent();
            DataContext = new AliasSettingsViewModel();
        }

        // 編集ボタンはダイアログを表示するため、ここを経由
        private async void EditGroupButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AliasGroupItem group)
            {
                // 親ウィンドウ取得
                var window = TopLevel.GetTopLevel(this) as Window;
                if (window == null) return;

                // ダイアログ表示
                // ※ダイアログの中身(AliasEditDialog)はまだMVVM化していませんが、
                // リポジトリを直接使って更新まで行う仕様なので、呼び出すだけでOK
                var dialog = new AliasEditDialog(group.TargetName);
                await dialog.ShowDialog(window);

                // 閉じた後に一覧を更新
                if (DataContext is AliasSettingsViewModel vm)
                {
                    vm.LoadAliasesCommand.Execute(null);
                }
            }
        }
    }
}