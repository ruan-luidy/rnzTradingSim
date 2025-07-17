using System.Globalization;

namespace rnzTradingSim.Helpers
{
  public static class CurrencyHelper
  {
    // Culture específica para USD
    private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

    public static string FormatCurrency(this decimal value)
    {
      return value.ToString("C2", UsdCulture);
    }

    public static string FormatAbbreviated(this decimal value)
    {
      if (value >= 1_000_000_000)
        return $"${(value / 1_000_000_000):F2}B";
      else if (value >= 1_000_000)
        return $"${(value / 1_000_000):F2}M";
      else if (value >= 1_000)
        return $"${(value / 1_000):F2}K";
      else
        return $"${value:N2}";
    }

    public static string FormatAbbreviatedClean(this decimal value)
    {
      if (value >= 1_000_000_000)
        return $"{(value / 1_000_000_000):F2}B";
      else if (value >= 1_000_000)
        return $"{(value / 1_000_000):F2}M";
      else if (value >= 1_000)
        return $"{(value / 1_000):F2}K";
      else
        return $"{value:N2}";
    }

    // Formatar valor por extenso para portfolio
    public static string FormatFull(this decimal value)
    {
      return value.ToString("C2", UsdCulture);
    }

    public static decimal ParseCurrency(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return 0m;

      // Remove símbolos de moeda e espaços
      var cleanValue = value.Replace("$", "")
                           .Replace(",", "")
                           .Replace(" ", "")
                           .Trim();

      if (decimal.TryParse(cleanValue, NumberStyles.Any, UsdCulture, out decimal result))
        return result;
      return 0m;
    }

    // Formatar sem símbolo $ para casos específicos
    public static string FormatNumber(this decimal value)
    {
      return value.ToString("N2", UsdCulture);
    }

    // Formatar percentual
    public static string FormatPercent(this decimal value)
    {
      return value.ToString("F2", UsdCulture) + "%";
    }
  }
}