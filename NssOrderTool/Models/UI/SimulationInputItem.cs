using CommunityToolkit.Mvvm.ComponentModel;

namespace NssOrderTool.Models.UI
{
  public partial class SimulationInputItem : ObservableObject
  {
    public int Index { get; set; }
    public string Placeholder { get; set; } = "";

    [ObservableProperty]
    private string _name = "";
  }
}
