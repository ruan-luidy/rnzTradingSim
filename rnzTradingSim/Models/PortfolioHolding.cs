using System.Globalization;

namespace rnzTradingSim.Models
{
  public class PortfolioHolding
  {
    private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");

    public string CoinId { get; set; } = string.Empty;
    public string CoinName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal AverageBuyPrice { get; set; }
    public decimal Value { get; set; }
    public decimal PnLAmount { get; set; }
    public decimal PnLPercentage { get; set; }
    public decimal Change24h { get; set; }
    public decimal PortfolioPercentage { get; set; }

    // Computed properties
    public string QuantityFormatted => Quantity.ToString("N8");
    public string CurrentPriceFormatted => CurrentPrice.ToString("C8", UsdCulture);
    public string ValueFormatted => Value.ToString("C2", UsdCulture);
    
    public string PnLPercentageFormatted => PnLPercentage >= 0 
      ? $"+{PnLPercentage:F2}%" 
      : $"{PnLPercentage:F2}%";
    
    public string Change24hFormatted => Change24h >= 0 
      ? $"+{Change24h:F2}%" 
      : $"{Change24h:F2}%";
    
    public string PortfolioPercentageFormatted => $"{PortfolioPercentage:F1}%";

    public bool IsPositivePnL => PnLPercentage >= 0;
    public bool IsPositive24hChange => Change24h >= 0;
  }
}