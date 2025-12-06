using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using NssOrderTool.Services;
using System;
using System.Diagnostics;

using NssOrderTool.Database;

namespace NssOrderTool;

public partial class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindow();

      try
      {
        var db = new DbManager();
        using (var conn = db.GetConnection())
        {
          Debug.WriteLine("✅ DB接続成功！");
          Console.WriteLine("✅ DB接続成功！"); // ターミナル用
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"❌ DB接続失敗: {ex.Message}");
        Console.WriteLine($"❌ DB接続失敗: {ex.Message}");
      }
    }

    base.OnFrameworkInitializationCompleted();
  }
}
