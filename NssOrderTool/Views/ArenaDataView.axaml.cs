using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.ViewModels;

namespace NssOrderTool.Views
{
  public partial class ArenaDataView : UserControl
  {
    public ArenaDataView()
    {
      InitializeComponent();

      if (!Design.IsDesignMode)
      {
        // DIコンテナから ViewModel を取得して設定
        DataContext = App.Services.GetRequiredService<ArenaDataViewModel>();
      }
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }

    private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
      this.Focus();
    }

    private void OnRootKeyDown(object? sender, KeyEventArgs e)
    {
      // Escapeキーが押されたらフォーカスを自分自身(UserControl)に戻す
      if (e.Key == Key.Escape)
      {
        this.Focus();
      }
    }
  }
}
