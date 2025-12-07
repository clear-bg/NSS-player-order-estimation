using Avalonia.Controls;
using Avalonia.Interactivity;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
    public partial class AliasEditDialog : Window
    {
        // コンストラクタでViewModelを受け取る形に変更しても良いが、
        // 今回は呼び出し元でDataContextセット済みであることを想定、
        // またはここでセットする
        public AliasEditDialog(string targetName)
        {
            InitializeComponent();
            // ViewModelを生成してセット
            DataContext = new AliasEditViewModel(targetName);
        }

        // デザイナー用など
        public AliasEditDialog()
        {
            InitializeComponent();
        }

        // 閉じるボタンだけはViewの責務としてここに残す
        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}