using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NssOrderTool.Models.Entities;

namespace NssOrderTool.Controls
{
  public class SimpleLineChart : Control
  {
    public static readonly StyledProperty<List<RateHistoryEntity>> ItemsSourceProperty =
        AvaloniaProperty.Register<SimpleLineChart, List<RateHistoryEntity>>(nameof(ItemsSource));

    public List<RateHistoryEntity> ItemsSource
    {
      get => GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }

    static SimpleLineChart()
    {
      AffectsRender<SimpleLineChart>(ItemsSourceProperty);
    }

    public override void Render(DrawingContext context)
    {
      base.Render(context);

      var width = Bounds.Width;
      var height = Bounds.Height;

      // サイズが確保できていない場合は描画しようがない
      if (width <= 0 || height <= 0) return;

      // --- 1. 外枠とグリッド (データ有無に関わらず表示) ---

      // 外枠を白で描画 (これで表示領域が確実にわかります)
      context.DrawRectangle(null, new Pen(Brushes.White, 1), new Rect(0, 0, width, height));

      // グリッド線 (薄い白)
      var gridBrush = new SolidColorBrush(Colors.White, 0.3); // 透明度30%の白
      var gridPen = new Pen(gridBrush, 1);
      context.DrawLine(gridPen, new Point(0, height * 0.25), new Point(width, height * 0.25));
      context.DrawLine(gridPen, new Point(0, height * 0.50), new Point(width, height * 0.50));
      context.DrawLine(gridPen, new Point(0, height * 0.75), new Point(width, height * 0.75));

      // --- データの取得 ---
      var items = ItemsSource;

      // データがない場合の表示
      if (items == null || items.Count == 0)
      {
        // 中央に「NO DATA」と白文字で表示
        var formattedText = new FormattedText(
            "NO DATA",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            24,
            Brushes.White
        );

        context.DrawText(formattedText, new Point((width - formattedText.Width) / 2, (height - formattedText.Height) / 2));
        return;
      }

      // --- 2. Y軸 (レート) の計算 ---
      var minRate = items.Min(x => x.Rate);
      var maxRate = items.Max(x => x.Rate);

      var rateRange = maxRate - minRate;
      if (rateRange == 0) rateRange = 100;
      var margin = rateRange * 0.2;

      var yMin = minRate - margin;
      var yRange = (maxRate + margin) - yMin;

      // --- 3. 座標変換 (横軸＝試合数等間隔) ---
      Point GetPoint(int index, double rate)
      {
        double x;
        if (items.Count <= 1)
          x = width / 2;
        else
          x = (index / (double)(items.Count - 1)) * width;

        var y = height - ((rate - yMin) / yRange * height);
        return new Point(x, y);
      }

      // --- 4. 線の描画 (白) ---
      // ★ここをご希望通り白に変更しました
      var linePen = new Pen(Brushes.White, 2);
      var dotBrush = Brushes.White;

      // 線を引く
      for (int i = 0; i < items.Count - 1; i++)
      {
        var p1 = GetPoint(i, items[i].Rate);
        var p2 = GetPoint(i + 1, items[i + 1].Rate);
        context.DrawLine(linePen, p1, p2);
      }

      // 点を打つ
      for (int i = 0; i < items.Count; i++)
      {
        var p = GetPoint(i, items[i].Rate);
        context.DrawEllipse(dotBrush, null, p, 4, 4);
      }
    }
  }
}
