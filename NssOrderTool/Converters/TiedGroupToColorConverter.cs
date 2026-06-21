using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NssOrderTool.Converters
{
  public class TiedGroupToColorConverter : IValueConverter
  {
    // グループごとに割り当てる色を定義 (ダークテーマで映える色)
    private static readonly IBrush[] GroupColors = new IBrush[]
    {
        SolidColorBrush.Parse("#88FF88"),
        SolidColorBrush.Parse("#FFAA44"),
        SolidColorBrush.Parse("#FF88FF"),
        SolidColorBrush.Parse("#4488FF"),
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is int groupIndex && groupIndex >= 0)
      {
        // インデックスが配列の範囲を超える場合はループさせる
        return GroupColors[groupIndex % GroupColors.Length];
      }

      // タイ（同配置順）でない場合は白を返す
      return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
