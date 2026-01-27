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

    // テキスト描画用のヘルパーメソッド
    private void DrawText(DrawingContext context, string text, Point origin, bool isRightAlign = false)
    {
      var formattedText = new FormattedText(
          text,
          CultureInfo.CurrentCulture,
          FlowDirection.LeftToRight,
          Typeface.Default,
          10, // フォントサイズ
          Brushes.LightGray // 文字色
      );

      // 右寄せしたい場合（Y軸の数値など）
      var pos = origin;
      if (isRightAlign)
      {
        pos = new Point(origin.X - formattedText.Width, origin.Y - formattedText.Height / 2);
      }
      else
      {
        // 中央揃えっぽい補正（X軸の数値など）
        pos = new Point(origin.X - formattedText.Width / 2, origin.Y);
      }

      context.DrawText(formattedText, pos);
    }

    public override void Render(DrawingContext context)
    {
      base.Render(context);

      var width = Bounds.Width;
      var height = Bounds.Height;
      if (width <= 0 || height <= 0) return;

      // --- 1. レイアウト設定 (余白の確保) ---
      // 左側にレート数値、下側に試合数を入れるための余白
      double paddingLeft = 40;
      double paddingBottom = 30;
      double paddingRight = 20;
      double paddingTop = 20;

      // 実際にグラフを描くエリア
      var graphRect = new Rect(
          paddingLeft,
          paddingTop,
          width - paddingLeft - paddingRight,
          height - paddingTop - paddingBottom
      );

      // 外枠 (デバッグ用に見やすく)
      // context.DrawRectangle(null, new Pen(Brushes.Gray, 1), new Rect(0, 0, width, height));

      // --- 2. 軸の名前 (ラベル) 表示 ---

      // Y軸ラベル "Rate" (左上)
      var rateLabel = new FormattedText("Rate", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(rateLabel, new Point(5, 0));

      // X軸ラベル "Matches" (右下)
      var matchLabel = new FormattedText("Matches", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
      context.DrawText(matchLabel, new Point(width - matchLabel.Width - 5, height - matchLabel.Height));


      // データ取得
      var items = ItemsSource;
      if (items == null || items.Count == 0)
      {
        // NO DATA 表示
        var noDataText = new FormattedText("NO DATA", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.White);
        context.DrawText(noDataText, new Point((width - noDataText.Width) / 2, (height - noDataText.Height) / 2));
        return;
      }

      // --- 3. データの範囲計算 ---
      var minRate = items.Min(x => x.Rate);
      var maxRate = items.Max(x => x.Rate);

      // 少し余裕を持たせる
      var rateRange = maxRate - minRate;
      if (rateRange == 0) rateRange = 100;
      var marginRate = rateRange * 0.1; // 上下10%

      var yAxisMax = maxRate + marginRate;
      var yAxisMin = minRate - marginRate;
      var yAxisRange = yAxisMax - yAxisMin;


      // --- 4. Y軸の目盛りとグリッド線 ---
      // 5分割してグリッドを引く
      int ySteps = 5;
      var gridPen = new Pen(new SolidColorBrush(Colors.White, 0.2), 1); // 薄いグリッド

      for (int i = 0; i <= ySteps; i++)
      {
        // 割合 (0.0 ～ 1.0)
        double ratio = i / (double)ySteps;

        // グラフエリア内のY座標 (上から下へ計算されることに注意)
        double yPos = graphRect.Bottom - (graphRect.Height * ratio);

        // その位置のレート値
        double rateVal = yAxisMin + (yAxisRange * ratio);

        // グリッド線
        context.DrawLine(gridPen, new Point(graphRect.Left, yPos), new Point(graphRect.Right, yPos));

        // 数値描画 (左側の余白部分に)
        DrawText(context, rateVal.ToString("F0"), new Point(paddingLeft - 5, yPos), isRightAlign: true);
      }


      // --- 5. X軸の目盛り ---
      // データ数に応じて間引きながら表示
      int count = items.Count;
      int xSteps = Math.Min(count, 10); // 最大10個くらいまで目盛りを表示
      int stepInterval = (int)Math.Ceiling((double)(count - 1) / (xSteps > 0 ? xSteps : 1));
      if (stepInterval < 1) stepInterval = 1;

      for (int i = 0; i < count; i += stepInterval)
      {
        // グラフエリア内のX座標
        double xPos;
        if (count <= 1) xPos = graphRect.Center.X;
        else xPos = graphRect.Left + (graphRect.Width * (i / (double)(count - 1)));

        // 数値描画 (下側の余白部分に)
        // 試合数は 1 から始めたいので i + 1
        DrawText(context, (i + 1).ToString(), new Point(xPos, graphRect.Bottom + 5), isRightAlign: false);
      }

      // 最後の試合数がループで漏れた場合、必ず描画する
      if ((count - 1) % stepInterval != 0)
      {
        double xPos = graphRect.Right;
        DrawText(context, count.ToString(), new Point(xPos, graphRect.Bottom + 5), isRightAlign: false);
      }


      // --- 6. データ線の描画 ---

      // 座標変換関数 (graphRect の範囲内に収める)
      Point ToPoint(int index, double rate)
      {
        double x;
        if (count <= 1)
          x = graphRect.Center.X;
        else
          x = graphRect.Left + (index / (double)(count - 1)) * graphRect.Width;

        // Y座標は上が0なので反転させる
        // (レート - 最小) / 全体幅 * 高さ
        double normalizedRate = (rate - yAxisMin) / yAxisRange;
        double y = graphRect.Bottom - (normalizedRate * graphRect.Height);

        return new Point(x, y);
      }

      var linePen = new Pen(Brushes.CornflowerBlue, 2); // 青い線
      var dotBrush = Brushes.White; // 白い点

      // 線を引く
      for (int i = 0; i < count - 1; i++)
      {
        var p1 = ToPoint(i, items[i].Rate);
        var p2 = ToPoint(i + 1, items[i + 1].Rate);
        context.DrawLine(linePen, p1, p2);
      }

      // 点を打つ
      for (int i = 0; i < count; i++)
      {
        var p = ToPoint(i, items[i].Rate);
        context.DrawEllipse(dotBrush, null, p, 3, 3);
      }
    }
  }
}
