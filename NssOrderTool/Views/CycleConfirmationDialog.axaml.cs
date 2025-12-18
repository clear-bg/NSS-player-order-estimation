using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NssOrderTool.Views
{
  public partial class CycleConfirmationDialog : Window
  {
    // コンストラクタ：外部からデータを受け取る
    public CycleConfirmationDialog(string input, List<string> cyclePath)
    {
      InitializeComponent();

      // 画面のコントロールに値をセット
      if (this.FindControl<TextBlock>("InputTextBlock") is TextBlock inputBlock)
      {
        inputBlock.Text = input;
      }

      if (this.FindControl<TextBlock>("CycleTextBlock") is TextBlock cycleBlock)
      {
        cycleBlock.Text = string.Join(" → ", cyclePath);
      }
    }

    // デザイナー用（引数なしコンストラクタ）
    public CycleConfirmationDialog()
    {
      InitializeComponent();
    }

    private void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
      // true = 登録続行
      Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
      // false = キャンセル
      Close(false);
    }
  }
}
