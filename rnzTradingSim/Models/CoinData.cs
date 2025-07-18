using System.Windows.Media;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace rnzTradingSim.Models
{
  public class CoinData
  {
    private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal MarketCapValue { get; set; }
    public decimal Volume24h { get; set; }
    public decimal PriceChange24h { get; set; }
    public decimal PriceChangePercentage24h { get; set; }
    public int MarketCapRank { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // Display properties
    public string Rank => $"#{MarketCapRank}";

    public string Price => CurrentPrice >= 1
          ? CurrentPrice.ToString("C2", UsdCulture)
          : CurrentPrice.ToString("C6", UsdCulture);

    public string Change24h => PriceChangePercentage24h >= 0
              ? $"+{PriceChangePercentage24h:F2}%"
              : $"{PriceChangePercentage24h:F2}%";

    public bool IsPositiveChange => PriceChangePercentage24h >= 0;

    public string MarketCap => FormatLargeNumber(MarketCapValue);

    public string Volume24hFormatted => FormatLargeNumber(Volume24h);

    public bool HasBadge => IsHot || IsWild;

    public string BadgeText => IsHot ? "HOT" : IsWild ? "WILD" : "";

    public SolidColorBrush ImageColor => new SolidColorBrush(GetCoinColor());

    // Gravatar ID baseado no símbolo da moeda
    public string GravatarId => GenerateGravatarId(Symbol);

    // Hot/Wild logic based on price change
    public bool IsHot => PriceChangePercentage24h >= 10 && PriceChangePercentage24h < 30;
    public bool IsWild => PriceChangePercentage24h >= 30 || PriceChangePercentage24h <= -30;

    private string FormatLargeNumber(decimal value)
    {
      if (value >= 1_000_000_000)
        return $"${value / 1_000_000_000:F2}B";
      else if (value >= 1_000_000)
        return $"${value / 1_000_000:F2}M";
      else if (value >= 1_000)
        return $"${value / 1_000:F2}K";
      else
        return value.ToString("C2", UsdCulture);
    }

    private Color GetCoinColor()
    {
      // Generate a color based on the coin symbol
      var hash = Symbol.GetHashCode();
      var colors = new[]
      {
                                          Colors.Orange,
                                          Colors.Blue,
                                          Colors.Green,
                                          Colors.Purple,
                                          Colors.Red,
                                          Colors.Teal,
                                          Colors.Pink,
                                          Colors.Brown,
                                          Colors.Cyan,
                                          Colors.Lime
                                          };

      return colors[Math.Abs(hash) % colors.Length];
    }

    private string GenerateGravatarId(string input)
    {
      // Usar o símbolo da moeda + um salt para gerar um hash MD5 único
      var saltedInput = $"{input.ToLowerInvariant()}@crypto.coin";

      using (var md5 = MD5.Create())
      {
        var inputBytes = Encoding.UTF8.GetBytes(saltedInput);
        var hashBytes = md5.ComputeHash(inputBytes);

        // Converter para string hexadecimal
        var sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
          sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
      }
    }

    // Propriedades formatadas para diferentes contextos
    public string PriceFormatted => Price;
    public string PriceChangeFormatted => Change24h;
    public string MarketCapFormatted => MarketCap;
    public string VolumeFormatted => Volume24hFormatted;

    // Método para obter cor baseada na mudança de preço
    public SolidColorBrush GetPriceChangeColor()
    {
      return IsPositiveChange
      ? new SolidColorBrush(Color.FromRgb(34, 197, 94))  // Green-500
      : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red-500
    }

    // Método para obter o texto formatado da mudança de preço com símbolo
    public string GetFormattedPriceChange()
    {
      var changeAmount = Math.Abs(PriceChange24h);
      var sign = PriceChangePercentage24h >= 0 ? "+" : "-";
      return $"{sign}{changeAmount.ToString("C6", UsdCulture)} ({Change24h})";
    }
  }
}