using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection; // 追加
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