using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
    public partial class SimulationView : UserControl
    {
        public SimulationView()
        {
            InitializeComponent();
            if (!Design.IsDesignMode)
            {
                // DIコンテナからViewModelを取得
                DataContext = App.Services.GetRequiredService<SimulationViewModel>();
            }
        }
    }
}