using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NssOrderTool.Views
{
  public partial class GraphWindow : Window
  {
    public GraphWindow()
    {
      InitializeComponent();
    }

    // ★追加: コピーボタンが押された時の処理
    private async void OnCopyButtonClicked(object? sender, RoutedEventArgs e)
    {
      if (DataContext is ViewModels.GraphWindowViewModel vm)
      {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
          await clipboard.SetTextAsync(vm.GraphText);

          // ボタンのテキストを一時的に変更してフィードバック
          if (sender is Button btn)
          {
            btn.Content = "✅ コピーしました！";
            btn.Background = Avalonia.Media.Brushes.DarkGreen;
          }
        }
      }
    }

    private void OnOpenBrowserClicked(object? sender, RoutedEventArgs e)
    {
      if (DataContext is ViewModels.GraphWindowViewModel vm)
      {
        try
        {
          // 1. Mermaidをブラウザで描画するためのHTMLテキストを組み立てる
          var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>順序グラフ プレビュー</title>
    <script type=""module"">
      import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
      mermaid.initialize({{ startOnLoad: true, theme: 'dark' }});
    </script>
    <style>
      body {{ background-color: #1E1E1E; color: white; display: flex; justify-content: center; padding: 20px; }}
    </style>
</head>
<body>
    <pre class=""mermaid"">
{vm.GraphText}
    </pre>
</body>
</html>";

          // 2. PCの一時フォルダにHTMLファイルとして保存する
          var tempPath = Path.Combine(Path.GetTempPath(), $"mermaid_preview_{Guid.NewGuid()}.html");
          File.WriteAllText(tempPath, html);

          // 3. 標準ブラウザでそのファイルを開く
          Process.Start(new ProcessStartInfo
          {
            FileName = tempPath,
            UseShellExecute = true // これを true にすることで、OSの標準ブラウザが立ち上がります
          });
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($"Browser launch failed: {ex.Message}");
        }
      }
    }
  }
}
