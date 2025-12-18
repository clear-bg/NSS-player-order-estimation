using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NssOrderTool.Views
{
  public partial class ConfirmationDialog : Window
  {
    public ConfirmationDialog()
    {
      InitializeComponent();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
      Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
      Close(false);
    }
  }
}
