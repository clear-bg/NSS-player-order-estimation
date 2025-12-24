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

        // 3. レンダリング (作業用キャンバスを作成)
        // 画面のピクセルサイズとDPIに合わせてビットマップを生成
        var pixelSize = new PixelSize((int)target.Bounds.Width, (int)target.Bounds.Height);
        var dpiVector = new Vector(192, 192);

        using var renderBitmap = new RenderTargetBitmap(pixelSize, dpiVector);
        renderBitmap.Render(target);

        // =========================================================
        // 【重要な修正点】
        // RenderTargetBitmapを直接渡さず、一度 MemoryStream を経由して
        // 「純粋なBitmapオブジェクト」に変換してから渡します。
        // これにより、Windows/Mac問わず空データになるのを防げます。
        // =========================================================

        // 4. メモリ上で一度PNG形式として保存する
        // (RenderTargetBitmap -> Stream)
        using var stream = new MemoryStream();
        renderBitmap.Save(stream);

        // ストリームの位置を先頭に戻す (必須)
        stream.Position = 0;

        // 5. ストリームから新しいBitmapを作成する
        // (Stream -> Bitmap)
        // これでGPU描画リソースから切り離された、ただの画像データになります
        var clipboardBitmap = new Bitmap(stream);

        // 6. クリップボードへ転送
        await topLevel.Clipboard.SetBitmapAsync(clipboardBitmap);

        // 完了通知
        if (DataContext is ArenaViewModel vm)
        {
          vm.StatusText = "📋 クリップボードにコピーしました";
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Clipboard Error: {ex.Message}");
        if (DataContext is ArenaViewModel vm)
        {
          vm.StatusText = "⚠️ コピーに失敗しました";
        }
      }
    }
  }
}
