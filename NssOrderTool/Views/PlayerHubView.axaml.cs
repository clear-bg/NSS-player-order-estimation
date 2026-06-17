using Avalonia.Controls;
using Avalonia.Input; // ★追加

namespace NssOrderTool.Views
{
  public partial class PlayerHubView : UserControl
  {
    public PlayerHubView()
    {
      InitializeComponent();
    }

    private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
    {
      // DataGridの外側をクリックしたら選択を解除する
      if (PlayerGrid != null)
      {
        PlayerGrid.SelectedItem = null;
      }
    }
  }
}
