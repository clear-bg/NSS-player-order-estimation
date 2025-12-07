using Avalonia.Controls;
using Avalonia.Interactivity;
using NssOrderTool.ViewModels;
using NssOrderTool.Repositories;

namespace NssOrderTool.Views
{
    public partial class AliasEditDialog : Window
    {
        public AliasEditDialog(string targetName, AliasRepository repo)
        {
            InitializeComponent();
            // ViewModelに渡す
            DataContext = new AliasEditViewModel(targetName, repo);
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