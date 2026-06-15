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

    // ★追加: 画面の余白がクリックされたときに呼ばれる処理
    private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
    {
      if (PlayerListBox != null)
      {
        // リストの選択状態を解除（null）にする
        PlayerListBox.SelectedItem = null;
      }
    }
  }
}
