using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class ArenaView : UserControl
  {
    private TextBox? _lastFocusedInput;
    public ArenaView()
    {
      InitializeComponent();

      // デザインモードでなければ、DIコンテナからViewModelを取得してセットする
      if (!Design.IsDesignMode)
      {
        DataContext = App.Services.GetRequiredService<ArenaViewModel>();
      }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      if (e.Key == Key.F2)
      {
        // 1. 直前に触っていた欄があればそこにフォーカス
        if (_lastFocusedInput != null)
        {
          _lastFocusedInput.Focus();
          // カーソルを末尾に移動させる（お好みでSelectAllでも可）
          _lastFocusedInput.CaretIndex = _lastFocusedInput.Text?.Length ?? 0;
          e.Handled = true;
          return;
        }

        // 2. まだ一度も触っていない場合は、画面内の最初の入力欄を探してフォーカス
        var firstTextBox = this.GetVisualDescendants()
                               .OfType<TextBox>()
                               .FirstOrDefault(t => t.Classes.Contains("PlayerNameBox"));

        if (firstTextBox != null)
        {
          firstTextBox.Focus();
          e.Handled = true;
        }
      }
    }

    private void OnNameInputKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        this.Focus();
      }
    }

    private void OnNameInputGotFocus(object sender, GotFocusEventArgs e)
    {
      if (sender is TextBox textBox)
      {
        _lastFocusedInput = textBox;
      }
    }
  }
}
