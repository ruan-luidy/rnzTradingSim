using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public enum TradeType
  {
    Buy,
    Sell,
    AddLiquidity,
    RemoveLiquidity,
    Mint,
    Burn,
    RugPull
  }
  
  public class Trade
  {
    [Key]
    public int Id { get; set; }
    public string CoinId { get; set; } = string.Empty;
    public int PlayerId { get; set; }
    public TradeType Type { get; set; }
    
    // Valores da transação
    public decimal TokenAmount { get; set; }
    public decimal UsdAmount { get; set; }
    public decimal PricePerToken { get; set; }
    public decimal PriceImpact { get; set; } // Porcentagem
    public decimal Slippage { get; set; } // Porcentagem
    
    // Fees
    public decimal TradingFee { get; set; } = 0.003m; // 0.3% padrão
    public decimal GasFee { get; set; } = 0.01m; // $0.01 simulado
    
    // Estado do pool após a transação
    public decimal PoolTokenAfter { get; set; }
    public decimal PoolBaseAfter { get; set; }
    public decimal PriceAfter { get; set; }
    
    // Metadata
    public string TxHash { get; set; } = Guid.NewGuid().ToString("N"); // Simulated
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsSuccessful { get; set; } = true;
    public string? FailureReason { get; set; }
    
    // Relacionamentos
    public UserCoin? Coin { get; set; }
    public Player? Player { get; set; }
    
    // Profit/Loss tracking
    public decimal RealizedPnL { get; set; } = 0m; // Para vendas
    
    public string GetDescription()
    {
      return Type switch
      {
        TradeType.Buy => $"Bought {TokenAmount:N2} {Coin?.Symbol} for ${UsdAmount:N2}",
        TradeType.Sell => $"Sold {TokenAmount:N2} {Coin?.Symbol} for ${UsdAmount:N2}",
        TradeType.AddLiquidity => $"Added ${UsdAmount:N2} liquidity",
        TradeType.RemoveLiquidity => $"Removed ${UsdAmount:N2} liquidity",
        TradeType.Mint => $"Minted {TokenAmount:N0} {Coin?.Symbol}",
        TradeType.RugPull => $"RUG PULLED! Stole ${UsdAmount:N2}",
        _ => "Unknown trade"
      };
    }
  }
}