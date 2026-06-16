using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.ViewModels
{
  public partial class GraphWindowViewModel : ViewModelBase
  {
    [ObservableProperty]
    private string _graphText = "";

    public GraphWindowViewModel()
    {
    }

    public GraphWindowViewModel(string graphText)
    {
      GraphText = graphText;
    }
  }
}
