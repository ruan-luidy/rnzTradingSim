using System;
using System.Globalization;
using System.Windows.Data;

namespace rnzTradingSim.Converters;

public class StringToDecimalConverter : IValueConverter
{
  private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
      return decimalValue.ToString("F2", UsdCulture);

    return "0.00";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is string stringValue && decimal.TryParse(stringValue, NumberStyles.Any, UsdCulture, out decimal result))
      return result;

    return 0m;
  }
}

// Converter para valores monetários com separador de milhares
public class CurrencyConverter : IValueConverter
{
  private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      return decimalValue.ToString("C2", UsdCulture); // Formato USD com $
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

// Converter para valores abreviados (K, M, B)
public class AbbreviatedCurrencyConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      return FormatAbbreviated(decimalValue);
    }
    return "$0";
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
      return $"${(value / 1_000_000_000):F2}B";
    }
    else if (value >= 1_000_000)
    {
      return $"${(value / 1_000_000):F2}M";
    }
    else if (value >= 1_000)
    {
      return $"${(value / 1_000):F2}K";
    }
    else
    {
      return $"${value:N2}";
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
      return $"{(value / 1_000_000_000):F2}B";
    }
    else if (value >= 1_000_000)
    {
      return $"{(value / 1_000_000):F2}M";
    }
    else if (value >= 1_000)
    {
      return $"{(value / 1_000):F2}K";
    }
    else
    {
      return $"{value:N2}";
    }
  }
}

// Converter específico para percentuais
public class PercentageConverter : IValueConverter
{
  private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is decimal decimalValue)
    {
      var sign = decimalValue >= 0 ? "+" : "";
      return $"{sign}{decimalValue:F2}%";
    }
    return "0.00%";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is string stringValue)
    {
      var cleanValue = stringValue.Replace("%", "").Replace("+", "").Trim();
      if (decimal.TryParse(cleanValue, NumberStyles.Any, UsdCulture, out decimal result))
        return result;
    }
    return 0m;
  }
}