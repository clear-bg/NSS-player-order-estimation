using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NssOrderTool.Views
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            // ダイアログを閉じて、結果として true (削除する) を返す
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            // ダイアログを閉じて、結果として false (キャンセル) を返す
            Close(false);
        }
    }
}