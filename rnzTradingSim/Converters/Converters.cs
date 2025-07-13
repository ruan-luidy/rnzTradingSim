using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace rnzTradingSim
{
  public static class Converters
  {
    public static readonly InverseBooleanConverter InverseBooleanConverter = new();
    public static readonly FlipButtonTextConverter FlipButtonTextConverter = new();
    public static readonly SpinButtonTextConverter SpinButtonTextConverter = new();
    public static readonly CoinflipSideToColorConverter CoinflipSideToColorConverter = new();
    public static readonly CoinflipResultToColorConverter CoinflipResultToColorConverter = new();
    public static readonly WinLossToColorConverter WinLossToColorConverter = new();
    public static readonly AmountToStringConverter AmountToStringConverter = new();
  }

  public class InverseBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
        return !boolValue;
      return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
        return !boolValue;
      return false;
    }
  }

  public class FlipButtonTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isFlipping)
        return isFlipping ? "🎯 FLIPPING..." : "🎯 FLIP COIN";
      return "🎯 FLIP COIN";
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
        return isSpinning ? "🎰 SPINNING..." : "🎰 SPIN";
      return "🎰 SPIN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class CoinflipSideToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string selectedSide && parameter is string buttonSide)
      {
        if (selectedSide == buttonSide)
        {
          return buttonSide == "HEADS" ?
              Application.Current.Resources["SuccessBrush"] :
              Application.Current.Resources["DangerBrush"];
        }
        else
        {
          return Application.Current.Resources["SecondaryRegionBrush"];
        }
      }
      return Application.Current.Resources["SecondaryRegionBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class CoinflipResultToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string result)
      {
        return result == "H" ?
            Application.Current.Resources["SuccessBrush"] :
            Application.Current.Resources["DangerBrush"];
      }
      return Application.Current.Resources["SecondaryRegionBrush"];
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
        return isWin ?
            Application.Current.Resources["SuccessBrush"] :
            Application.Current.Resources["DangerBrush"];
      }
      return Application.Current.Resources["SecondaryTextBrush"];
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
        string sign = amount >= 0 ? "+" : "";
        return $"{sign}${amount:N0}";
      }
      return "$0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}