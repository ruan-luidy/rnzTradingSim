using System.Globalization;

namespace rnzTradingSim.Helpers
{
  public static class CurrencyHelper
  {
    public static string FormatCurrency(this decimal value)
    {
      return value.ToString("C2", CultureInfo.CurrentCulture);
    }

    public static string FormatAbbreviated(this decimal value)
    {
      if (value >= 1_000_000_000)
        return $"R$ {(value / 1_000_000_000):F1}B";
      else if (value >= 1_000_000)
        return $"R$ {(value / 1_000_000):F1}M";
      else if (value >= 1_000)
        return $"R$ {(value / 1_000):F1}K";
      else
        return $"R$ {value:N2}";
    }

    public static decimal ParseCurrency(string value)
    {
      if (decimal.TryParse(value.Replace("R$", "").Replace(".", "").Replace(",", "."),
          NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        return result;
      return 0m;
    }
  }
}
