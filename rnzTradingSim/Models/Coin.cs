using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public class Coin
  {
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
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
  }
}