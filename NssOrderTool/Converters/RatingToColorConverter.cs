using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NssOrderTool.Models.Domain;

namespace NssOrderTool.Converters
{
  public class RatingToColorConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is double rating)
      {
        string hexColor = GetColorHex(rating);
        return Brush.Parse(hexColor);
      }
      return Brush.Parse(RatingColorConstants.Default);
    }

    private string GetColorHex(double rating)
    {
      if (rating < 1200) return RatingColorConstants.Red;
      if (rating < 1450) return RatingColorConstants.Orange;
      if (rating < 1550) return RatingColorConstants.Yellow;
      if (rating < 1700) return RatingColorConstants.Green;
      if (rating < 1900) return RatingColorConstants.Blue;

      return RatingColorConstants.Purple;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
