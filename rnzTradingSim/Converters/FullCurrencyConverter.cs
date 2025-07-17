using System;
using System.Globalization;
using System.Windows.Data;

namespace rnzTradingSim.Converters
{
  // Converter para valores monetários por extenso (sem abreviação)
  public class FullCurrencyConverter : IValueConverter
  {
    private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is decimal decimalValue)
      {
        return decimalValue.ToString("C2", UsdCulture);
      }
      return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string stringValue)
      {
        // Remove símbolos de moeda e parse
        var cleanValue = stringValue.Replace("$", "").Replace(",", "").Trim();
        if (decimal.TryParse(cleanValue, NumberStyles.Any, UsdCulture, out decimal result))
          return result;
      }

      return 0m;
    }
  }
}