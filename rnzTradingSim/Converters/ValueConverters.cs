using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace rnzTradingSim.Converters;

public class StringToDecimalConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
      return decimalValue.ToString("F2", culture);

    return "0.00";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is string stringValue && decimal.TryParse(stringValue, NumberStyles.Any, culture, out decimal result))
      return result;

    return 0m;
  }
}