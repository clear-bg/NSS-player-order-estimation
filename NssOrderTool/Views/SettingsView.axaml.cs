using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                DataContext = App.Services.GetRequiredService<SettingsViewModel>();
            }
        }
    }
}