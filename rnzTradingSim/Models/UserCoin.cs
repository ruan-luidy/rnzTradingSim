using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public class UserCoin
  {
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CreatorId { get; set; } // Player ID que criou
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    // Supply e economia
    public decimal TotalSupply { get; set; }
    public decimal CirculatingSupply { get; set; }
    public decimal InitialPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap => CurrentPrice * CirculatingSupply;
    
    // Liquidity Pool
    public decimal PoolTokenAmount { get; set; } // Quantidade da moeda no pool
    public decimal PoolBaseAmount { get; set; } // Quantidade de USD no pool
    public decimal PoolConstant => PoolTokenAmount * PoolBaseAmount; // K = x * y
    
    // Estatísticas
    public int TotalHolders { get; set; } = 0;
    public int TotalTransactions { get; set; } = 0;
    public decimal Volume24h { get; set; } = 0m;
    public decimal PriceChange24h { get; set; } = 0m;
    public decimal AllTimeHigh { get; set; } = 0m;
    public decimal AllTimeLow { get; set; } = decimal.MaxValue;
    
    // Flags
    public bool IsRugged { get; set; } = false;
    public bool IsLocked { get; set; } = false; // Liquidity locked
    public DateTime? LockEndDate { get; set; }
    
    // Datas
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    
    // Relacionamentos
    public Player? Creator { get; set; }
    
    // Métodos auxiliares
    public decimal CalculatePriceImpact(decimal tradeAmount, bool isBuy)
    {
      if (PoolTokenAmount == 0 || PoolBaseAmount == 0) return 0;
      
      var k = PoolConstant;
      
      if (isBuy)
      {
        // Comprando tokens com USD
        var newBaseAmount = PoolBaseAmount + tradeAmount;
        var newTokenAmount = k / newBaseAmount;
        var tokensOut = PoolTokenAmount - newTokenAmount;
        var pricePerToken = tradeAmount / tokensOut;
        var priceImpact = (pricePerToken - CurrentPrice) / CurrentPrice * 100;
        return Math.Abs(priceImpact);
      }
      else
      {
        // Vendendo tokens por USD
        var newTokenAmount = PoolTokenAmount + tradeAmount;
        var newBaseAmount = k / newTokenAmount;
        var usdOut = PoolBaseAmount - newBaseAmount;
        var pricePerToken = usdOut / tradeAmount;
        var priceImpact = (CurrentPrice - pricePerToken) / CurrentPrice * 100;
        return Math.Abs(priceImpact);
      }
    }
    
    public (decimal tokensOut, decimal newPrice) SimulateBuy(decimal usdAmount)
    {
      if (PoolTokenAmount == 0 || PoolBaseAmount == 0) 
        return (0, 0);
      
      var k = PoolConstant;
      var newBaseAmount = PoolBaseAmount + usdAmount;
      var newTokenAmount = k / newBaseAmount;
      var tokensOut = PoolTokenAmount - newTokenAmount;
      var newPrice = newBaseAmount / newTokenAmount;
      
      return (tokensOut, newPrice);
    }
    
    public (decimal usdOut, decimal newPrice) SimulateSell(decimal tokenAmount)
    {
      if (PoolTokenAmount == 0 || PoolBaseAmount == 0) 
        return (0, 0);
      
      var k = PoolConstant;
      var newTokenAmount = PoolTokenAmount + tokenAmount;
      var newBaseAmount = k / newTokenAmount;
      var usdOut = PoolBaseAmount - newBaseAmount;
      var newPrice = newBaseAmount / newTokenAmount;
      
      return (usdOut, newPrice);
    }
  }
}