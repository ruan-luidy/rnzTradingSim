

using System.Windows.Media;

namespace rnzTradingSim.Models
{
  public class CoinData
  {
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
      ? $"{CurrentPrice:F2}"
      : $"{CurrentPrice:F6}";

    public string Change24h => PriceChangePercentage24h >= 0
      ? $"+{PriceChangePercentage24h:F2}"
      : $"{PriceChangePercentage24h:F2}";

    public bool IsPositiveChange => PriceChangePercentage24h >= 0;

    public string MarketCap => FormatLargeNumber(MarketCapValue);

    public string Volume24hFormatted => FormatLargeNumber(Volume24h);

    public string Created => GetTimeAgo();

    public bool HasBadge => IsHot || IsWild;

    public SolidColorBrush BadgeColor => IsHot
      ? new SolidColorBrush(Colors.Orange)
      : IsWild
        ? new SolidColorBrush(Colors.Red)
        : new SolidColorBrush(Colors.Transparent);

    public SolidColorBrush ImageColor => new SolidColorBrush(GetCoinColor());

    // hot/wild logic based on price change
    public bool IsHot => PriceChangePercentage24h >= 10 && PriceChangePercentage24h < 30;
    public bool IsWild => PriceChangePercentage24h >= 30 || PriceChangePercentage24h <= -30;

    private string FormatLargeNumber(decimal value)
    {
      if (value >= 1_000_000_000)
        return $"{value / 1_000_000_000:F1}B";
      else if (value >= 1_000_000)
        return $"{value / 1_000_000:F1}M";
      else if (value >= 1_000)
        return $"{value / 1_000:F1}K";
      else
        return $"{value:F2}";
    }

    private string GetTimeAgo()
    {
      var timeSpan = DateTime.Now - LastUpdated;

      if (timeSpan.TotalDays >= 1)
        return $"{(int)timeSpan.TotalDays}d";
      else if (timeSpan.TotalHours >= 1)
        return $"{(int)timeSpan.TotalHours}h";
      else if (timeSpan.TotalMinutes >= 1)
        return $"{(int)timeSpan.TotalMinutes}m";
      else
        return "1m";
    }

    private Color GetCoinColor()
    {
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
  }
}
