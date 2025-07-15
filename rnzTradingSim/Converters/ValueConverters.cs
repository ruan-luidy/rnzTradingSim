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

// Converter para valores monetários com separador de milhares
public class CurrencyConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      return decimalValue.ToString("N2", culture); // Formato com separador de milhares
    }
    return "0,00";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is string stringValue && decimal.TryParse(stringValue, NumberStyles.Any, culture, out decimal result))
      return result;

    return 0m;
  }
}

// Converter para valores abreviados (K, M, B)
public class AbbreviatedCurrencyConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      return FormatAbbreviated(decimalValue);
    }
    return "R$ 0";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    // Não implementado para valores abreviados
    return 0m;
  }

  private string FormatAbbreviated(decimal value)
  {
    if (value >= 1_000_000_000)
    {
      return $"R$ {(value / 1_000_000_000):F1}B";
    }
    else if (value >= 1_000_000)
    {
      return $"R$ {(value / 1_000_000):F1}M";
    }
    else if (value >= 1_000)
    {
      return $"R$ {(value / 1_000):F1}K";
    }
    else
    {
      return $"R$ {value:N2}";
    }
  }
}

// Converter para valores sem prefixo monetário abreviados
public class AbbreviatedValueConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      return FormatAbbreviated(decimalValue);
    }
    return "0";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return 0m;
  }

  private string FormatAbbreviated(decimal value)
  {
    if (value >= 1_000_000_000)
    {
      return $"{(value / 1_000_000_000):F1}B";
    }
    else if (value >= 1_000_000)
    {
      return $"{(value / 1_000_000):F1}M";
    }
    else if (value >= 1_000)
    {
      return $"{(value / 1_000):F1}K";
    }
    else
    {
      return $"{value:N2}";
    }
  }
}