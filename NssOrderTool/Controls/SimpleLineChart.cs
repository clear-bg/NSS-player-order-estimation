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

      // 軸ラベル
      var rateLabel = new FormattedText("Rate", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(rateLabel, new Point(5, 0));

      var matchLabel = new FormattedText("Matches", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(matchLabel, new Point(width - matchLabel.Width - 5, height - matchLabel.Height));

      var items = ItemsSource;
      int count = (items != null) ? items.Count : 0;

      // データなし表示
      if (count == 0)
      {
        var noDataText = new FormattedText("NO DATA", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.White);
        context.DrawText(noDataText, new Point((width - noDataText.Width) / 2, (height - noDataText.Height) / 2));
        context.DrawRectangle(null, new Pen(Brushes.White, 1), new Rect(0, 0, width, height));
        return;
      }

      // --- 2. Y軸 (レート) の計算: 自動ステップ調整 ---
      double minRate = items.Min(x => x.Rate);
      double maxRate = items.Max(x => x.Rate);

      // データの振れ幅
      double range = maxRate - minRate;
      if (range < 10) range = 10; // 最低でも10幅は確保

      // 目標: 画面内に線を4～5本くらい引きたい
      double roughStep = range / 4.0;

      // roughStep に近い「切りの良い数字」を選ぶ
      // (10, 20, 50, 100, 200, 500...)
      double yStep = 10.0;
      if (roughStep > 200) yStep = 500.0;
      else if (roughStep > 100) yStep = 200.0;
      else if (roughStep > 50) yStep = 100.0;
      else if (roughStep > 20) yStep = 50.0;
      else if (roughStep > 10) yStep = 20.0;
      else yStep = 10.0;

      // yStepの倍数になるように最小・最大を丸める
      double yAxisMin = Math.Floor(minRate / yStep) * yStep;
      double yAxisMax = Math.Ceiling(maxRate / yStep) * yStep;

      // データが境界線ギリギリすぎると見にくいので、窮屈なら1段広げる
      if (yAxisMax - yAxisMin < range * 1.1)
      {
        yAxisMax += yStep;
        // それでもまだ狭ければ下も広げる
        if (minRate - yAxisMin < yStep * 0.1) yAxisMin -= yStep;
      }

      double yAxisRange = yAxisMax - yAxisMin;


      // --- 3. X軸 (試合数) の計算 ---
      int xMax = Math.Max(count, 5);

      // X座標計算ヘルパー (前回と同じ: 0-1間を狭くするロジック)
      double GetXPos(int matchNum)
      {
        if (matchNum == 0) return graphRect.Left;

        double gap0to1 = 15.0; // 0と1の間隔
        double startX = graphRect.Left + gap0to1;
        double remainingWidth = graphRect.Width - gap0to1;

        if (xMax <= 1) return startX + remainingWidth / 2;

        double ratio = (double)(matchNum - 1) / (xMax - 1);
        return startX + (remainingWidth * ratio);
      }


      // --- 4. 描画開始 ---
      var gridPen = new Pen(new SolidColorBrush(Colors.White, 0.2), 1);
      var axisPen = new Pen(new SolidColorBrush(Colors.White, 0.5), 1);

      // Y軸グリッド (自動計算した yStep 刻み)
      // 数値誤差対策で少しマージンを持たせてループ
      for (double r = yAxisMin; r <= yAxisMax + 0.1; r += yStep)
      {
        double normalized = (r - yAxisMin) / yAxisRange;
        double yPos = graphRect.Bottom - (graphRect.Height * normalized);

        // 範囲外チェック
        if (yPos < graphRect.Top - 1 || yPos > graphRect.Bottom + 1) continue;

        context.DrawLine(gridPen, new Point(graphRect.Left, yPos), new Point(graphRect.Right, yPos));
        DrawText(context, r.ToString("F0"), new Point(paddingLeft - 5, yPos), isRightAlign: true);
      }

      // X軸グリッド (縦線)
      // Y軸線
      context.DrawLine(axisPen, new Point(graphRect.Left, graphRect.Top), new Point(graphRect.Left, graphRect.Bottom));

      // 目盛りの間引き設定
      int xStep = 1;
      if (xMax > 100) xStep = 10;
      else if (xMax > 50) xStep = 5;
      else if (xMax > 20) xStep = 2;

      // 目盛り (1 ～ xMax)
      for (int x = 1; x <= xMax; x += xStep)
      {
        double xPos = GetXPos(x);
        context.DrawLine(gridPen, new Point(xPos, graphRect.Top), new Point(xPos, graphRect.Bottom));
        DrawText(context, x.ToString(), new Point(xPos, graphRect.Bottom + 5), isRightAlign: false);
      }


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

        // クリッピング処理 (簡易)
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
