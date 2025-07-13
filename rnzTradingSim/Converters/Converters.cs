using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace rnzTradingSim
{
  public static class Converters
  {
    public static readonly CoinflipSideToColorConverter CoinflipSideToColorConverter = new();
    public static readonly FlipButtonTextConverter FlipButtonTextConverter = new();
    public static readonly SpinButtonTextConverter SpinButtonTextConverter = new();
    public static readonly InverseBooleanConverter InverseBooleanConverter = new();
    public static readonly CoinflipResultToColorConverter CoinflipResultToColorConverter = new();
    public static readonly WinLossToColorConverter WinLossToColorConverter = new();
    public static readonly AmountToStringConverter AmountToStringConverter = new();
    public static readonly ProfitLossToColorConverter ProfitLossToColorConverter = new();
    public static readonly Boolean2VisibilityConverter Boolean2VisibilityConverter = new();
  }

  public class CoinflipSideToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var selectedSide = value?.ToString();
      var buttonSide = parameter?.ToString();

      if (selectedSide == buttonSide)
      {
        return new SolidColorBrush(Color.FromRgb(99, 102, 241)); // Primary color
      }

      return new SolidColorBrush(Color.FromRgb(75, 85, 99)); // Default gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class FlipButtonTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isFlipping)
      {
        return isFlipping ? "🪙 Flipping..." : "🪙 FLIP COIN";
      }
      return "🪙 FLIP COIN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class SpinButtonTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isSpinning)
      {
        return isSpinning ? "🎰 Spinning..." : "🎰 SPIN";
      }
      return "🎰 SPIN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class InverseBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue;
      }
      return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue;
      }
      return false;
    }
  }

  public class CoinflipResultToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var result = value?.ToString();

      return result switch
      {
        "HEADS" => new SolidColorBrush(Color.FromRgb(16, 185, 129)), // Green
        "TAILS" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red
        _ => new SolidColorBrush(Color.FromRgb(107, 114, 128)) // Gray
      };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class WinLossToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isWin)
      {
        return isWin
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      if (value is decimal decimalValue)
      {
        return decimalValue >= 0
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      if (value is double doubleValue)
      {
        return doubleValue >= 0
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      // Default to gray if unknown type
      return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class ProfitLossToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is decimal decimalValue)
      {
        return decimalValue >= 0
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      if (value is double doubleValue)
      {
        return doubleValue >= 0
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      if (value is int intValue)
      {
        return intValue >= 0
            ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // Green
            : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
      }

      // Default to gray
      return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class AmountToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is decimal amount)
      {
        var sign = amount >= 0 ? "+" : "";
        return $"{sign}${amount:N0}";
      }

      if (value is double doubleAmount)
      {
        var sign = doubleAmount >= 0 ? "+" : "";
        return $"{sign}${doubleAmount:N0}";
      }

      if (value is int intAmount)
      {
        var sign = intAmount >= 0 ? "+" : "";
        return $"{sign}${intAmount:N0}";
      }

      return "$0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class Boolean2VisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
      }
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visibility)
      {
        return visibility == Visibility.Visible;
      }
      return false;
    }
  }
}