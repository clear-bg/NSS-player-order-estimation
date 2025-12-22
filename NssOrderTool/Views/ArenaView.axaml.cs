using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
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

    private async void OnScreenshotClick(object? sender, RoutedEventArgs e)
    {
      // 1. 撮影対象(表エリア)を取得
      var target = this.FindControl<Control>("CaptureTarget");
      if (target == null) return;

      try
      {
        // 2. 保存先ダイアログ
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
          Title = "集計表を保存",
          SuggestedFileName = $"ArenaTable_{DateTime.Now:yyyyMMdd_HHmm}.png",
          DefaultExtension = ".png",
          FileTypeChoices = new[] { FilePickerFileTypes.ImagePng }
        });

        if (file == null) return;

        // 3. レンダリング
        // 画面に表示されているサイズで画像化します
        var pixelSize = new PixelSize((int)target.Bounds.Width, (int)target.Bounds.Height);
        var dpiVector = new Vector(96, 96);

        using var bitmap = new RenderTargetBitmap(pixelSize, dpiVector);
        bitmap.Render(target);

        // 4. 書き出し
        using var stream = await file.OpenWriteAsync();
        bitmap.Save(stream);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Screenshot Error: {ex.Message}");
      }
    }

    private async void OnCopyToClipboardClick(object? sender, RoutedEventArgs e)
    {
      // 1. 撮影対象(表エリア)を取得
      var target = this.FindControl<Control>("CaptureTarget");
      if (target == null) return;

      try
      {
        // 2. クリップボード機能へのアクセス権を取得
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        // 3. レンダリング
        var pixelSize = new PixelSize((int)target.Bounds.Width, (int)target.Bounds.Height);
        var dpiVector = new Vector(96, 96);

        using var bitmap = new RenderTargetBitmap(pixelSize, dpiVector);
        bitmap.Render(target);

        // 4. クリップボードへ転送 (ここを修正)
        // DataObjectを作成せず、直接 SetBitmapAsync を使用します
        // これにより DataFormats.Bitmap がないエラーや Obsolete 警告も解消されます
        await topLevel.Clipboard.SetBitmapAsync(bitmap);

        if (DataContext is ArenaViewModel vm)
        {
          vm.StatusText = "📋 クリップボードにコピーしました";
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Clipboard Error: {ex.Message}");
      }
    }
  }
}
