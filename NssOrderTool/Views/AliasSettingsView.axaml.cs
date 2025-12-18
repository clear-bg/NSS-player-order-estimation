using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree; // TopLevel取得用
using Microsoft.Extensions.DependencyInjection;
using NssOrderTool.Models;
using NssOrderTool.ViewModels;
using NssOrderTool.Repositories;

namespace NssOrderTool.Views
{
  public partial class AliasSettingsView : UserControl
  {
    public AliasSettingsView()
    {
      InitializeComponent();
      if (!Design.IsDesignMode)
      {
        // ここが重要！ new AliasSettingsViewModel() ではなく DIから取得します
        DataContext = App.Services.GetRequiredService<AliasSettingsViewModel>();
      }
    }

    private async void EditGroupButton_Click(object? sender, RoutedEventArgs e)
    {
      if (sender is Button btn && btn.DataContext is AliasGroupItem group)
      {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        // ダイアログ表示用のリポジトリも DI から取得
        var repo = App.Services.GetRequiredService<AliasRepository>();

        // ダイアログにリポジトリを渡す
        var dialog = new AliasEditDialog(group.TargetName, repo);

        await dialog.ShowDialog(window);

        if (DataContext is AliasSettingsViewModel vm)
        {
          await vm.LoadAliasesCommand.ExecuteAsync(null);
        }
      }
    }
  }
}