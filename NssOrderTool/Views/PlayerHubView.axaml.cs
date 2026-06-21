using Avalonia.Controls;
using Avalonia.Input;

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

      TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
      }
    }
  }
}
