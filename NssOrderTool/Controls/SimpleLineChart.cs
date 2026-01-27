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

    private void DrawText(DrawingContext context, string text, Point origin, bool isRightAlign = false)
    {
      var formattedText = new FormattedText(
          text,
          CultureInfo.CurrentCulture,
          FlowDirection.LeftToRight,
          Typeface.Default,
          10,
          Brushes.LightGray
      );

      var pos = origin;
      if (isRightAlign)
        pos = new Point(origin.X - formattedText.Width, origin.Y - formattedText.Height / 2);
      else
        pos = new Point(origin.X - formattedText.Width / 2, origin.Y);

      context.DrawText(formattedText, pos);
    }

    public override void Render(DrawingContext context)
    {
      base.Render(context);

      var width = Bounds.Width;
      var height = Bounds.Height;
      if (width <= 0 || height <= 0) return;

      // --- 1. レイアウト設定 ---
      double paddingLeft = 40;
      double paddingBottom = 30;
      double paddingRight = 20;
      double paddingTop = 20;

      var graphRect = new Rect(
          paddingLeft,
          paddingTop,
          width - paddingLeft - paddingRight,
          height - paddingTop - paddingBottom
      );

      // 外枠
      var axisPen = new Pen(new SolidColorBrush(Colors.White, 0.5), 1);
      context.DrawRectangle(null, axisPen, graphRect);

      // 軸ラベル
      var rateLabel = new FormattedText("Rate", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(rateLabel, new Point(5, 0));

      var matchLabel = new FormattedText("Matches", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(matchLabel, new Point(width - matchLabel.Width - 5, height - matchLabel.Height));

      var items = ItemsSource;
      int count = (items != null) ? items.Count : 0;

      // データなし
      if (count == 0)
      {
        var noDataText = new FormattedText("NO DATA", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.White);
        context.DrawText(noDataText, new Point((width - noDataText.Width) / 2, (height - noDataText.Height) / 2));
        context.DrawRectangle(null, new Pen(Brushes.White, 1), new Rect(0, 0, width, height));
        return;
      }

      // --- 2. Y軸 (レート) の計算 ---
      double minRate = items.Min(x => x.Rate);
      double maxRate = items.Max(x => x.Rate);
      double range = maxRate - minRate;
      if (range < 10) range = 10;

      // 自動ステップ (10, 20, 50, 100...)
      double roughStep = range / 4.0;
      double yStep = 10.0;
      if (roughStep > 200) yStep = 500.0;
      else if (roughStep > 100) yStep = 200.0;
      else if (roughStep > 50) yStep = 100.0;
      else if (roughStep > 20) yStep = 50.0;
      else if (roughStep > 10) yStep = 20.0;
      else yStep = 10.0;

      double yAxisMin = Math.Floor(minRate / yStep) * yStep;
      double yAxisMax = Math.Ceiling(maxRate / yStep) * yStep;

      if (yAxisMax - yAxisMin < range * 1.1)
      {
        yAxisMax += yStep;
        if (minRate - yAxisMin < yStep * 0.1) yAxisMin -= yStep;
      }
      double yAxisRange = yAxisMax - yAxisMin;


      // --- 3. X軸 (試合数) の計算 ---
      int xMax = Math.Max(count, 5);

      // 座標計算ヘルパー
      double GetXPos(int matchNum)
      {
        if (matchNum == 0) return graphRect.Left; // Y軸線

        double gap0to1 = 15.0;  // 左余白
        double gapRight = 15.0; // 右余白

        double startX = graphRect.Left + gap0to1;
        double effectiveWidth = graphRect.Width - gap0to1 - gapRight;

        if (xMax <= 1) return startX + effectiveWidth / 2;

        // matchNum=1 -> startX
        // matchNum=xMax -> startX + effectiveWidth
        double ratio = (double)(matchNum - 1) / (xMax - 1);
        return startX + (effectiveWidth * ratio);
      }


      // --- 4. 描画開始 ---
      var gridPen = new Pen(new SolidColorBrush(Colors.White, 0.2), 1);

      // Y軸グリッド
      for (double r = yAxisMin; r <= yAxisMax + 0.1; r += yStep)
      {
        double normalized = (r - yAxisMin) / yAxisRange;
        double yPos = graphRect.Bottom - (graphRect.Height * normalized);

        // 外枠部分は描画しない
        if (yPos < graphRect.Top + 1 || yPos > graphRect.Bottom - 1)
        {
          DrawText(context, r.ToString("F0"), new Point(paddingLeft - 5, yPos), isRightAlign: true);
          continue;
        }

        context.DrawLine(gridPen, new Point(graphRect.Left, yPos), new Point(graphRect.Right, yPos));
        DrawText(context, r.ToString("F0"), new Point(paddingLeft - 5, yPos), isRightAlign: true);
      }

      // X軸グリッド (縦線)
      // ★変更: 5や10の倍数を優先するロジック

      int xStep = 1;
      // 20試合までは1刻み
      if (xMax <= 20) xStep = 1;
      // 50試合までは5刻み (5, 10, 15... 40)
      else if (xMax <= 50) xStep = 5;
      // 100試合までは10刻み
      else if (xMax <= 100) xStep = 10;
      // それ以上は20刻み
      else xStep = 20;

      // 描画ヘルパー
      void DrawXGrid(int val)
      {
        double xPos = GetXPos(val);
        context.DrawLine(gridPen, new Point(xPos, graphRect.Top), new Point(xPos, graphRect.Bottom));
        DrawText(context, val.ToString(), new Point(xPos, graphRect.Bottom + 5), isRightAlign: false);
      }

      // まず「1」は必ず描画したい (ただしxStep=1のときはループで描くので重複させない)
      if (xStep > 1)
      {
        DrawXGrid(1);
      }

      // xStep刻みで描画 (5, 10, 15...)
      // xStep=1 のときは 1, 2, 3... となる
      // xStep=5 のときは 5, 10, 15... となる
      for (int x = xStep; x <= xMax; x += xStep)
      {
        DrawXGrid(x);
      }

      // ※ここで「最後の半端な数値 (例:44)」は描画されません。
      // ループ条件が x <= xMax なので、40の次は45になり、45 > 44 で止まるからです。
      // これにより「右端は40、プロットだけ44まである」状態が実現されます。


      // --- 5. プロット描画 ---
      Point ToPoint(int index, double rate)
      {
        double x = GetXPos(index + 1);
        double normalizedRate = (rate - yAxisMin) / yAxisRange;
        double y = graphRect.Bottom - (normalizedRate * graphRect.Height);
        return new Point(x, y);
      }

      var linePen = new Pen(Brushes.CornflowerBlue, 2);
      var dotBrush = Brushes.White;

      // 線
      for (int i = 0; i < count - 1; i++)
      {
        var p1 = ToPoint(i, items[i].Rate);
        var p2 = ToPoint(i + 1, items[i + 1].Rate);

        if (p1.Y >= graphRect.Top && p1.Y <= graphRect.Bottom &&
            p2.Y >= graphRect.Top && p2.Y <= graphRect.Bottom)
        {
          context.DrawLine(linePen, p1, p2);
        }
      }

      // 点
      for (int i = 0; i < count; i++)
      {
        var p = ToPoint(i, items[i].Rate);
        if (p.Y >= graphRect.Top && p.Y <= graphRect.Bottom)
        {
          context.DrawEllipse(dotBrush, null, p, 3, 3);
        }
      }
    }
  }
}
