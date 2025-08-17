using System.ComponentModel.DataAnnotations;

namespace rnzTradingSim.Models
{
  public class Portfolio
  {
    [Key]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string CoinId { get; set; } = string.Empty;
    
    // Holdings
    public decimal TokenBalance { get; set; } = 0m;
    public decimal AverageBuyPrice { get; set; } = 0m;
    public decimal TotalInvested { get; set; } = 0m;
    
    // Estatísticas
    public decimal RealizedPnL { get; set; } = 0m;
    public decimal UnrealizedPnL => CalculateUnrealizedPnL();
    public int TotalBuys { get; set; } = 0;
    public int TotalSells { get; set; } = 0;
    
    // Liquidity provision
    public decimal LiquidityTokens { get; set; } = 0m; // LP tokens
    public decimal PoolShare => CalculatePoolShare(); // Porcentagem do pool
    
    // Dates
    public DateTime FirstPurchase { get; set; }
    public DateTime LastTransaction { get; set; }
    
    // Relacionamentos
    public Player? Player { get; set; }
    public UserCoin? Coin { get; set; }
    
    private decimal CalculateUnrealizedPnL()
    {
      if (Coin == null || TokenBalance == 0) return 0;
      
      var currentValue = TokenBalance * Coin.CurrentPrice;
      return currentValue - TotalInvested;
    }
    
    private decimal CalculatePoolShare()
    {
      if (Coin == null || LiquidityTokens == 0) return 0;
      
      // Implementar cálculo baseado no total de LP tokens
      return 0; // TODO: Implementar quando tivermos o sistema de LP tokens
    }
    
    public void UpdateAfterBuy(decimal tokens, decimal usdSpent, decimal price)
    {
      var newTotal = TokenBalance + tokens;
      var newInvested = TotalInvested + usdSpent;
      
      AverageBuyPrice = newInvested / newTotal;
      TokenBalance = newTotal;
      TotalInvested = newInvested;
      TotalBuys++;
      LastTransaction = DateTime.Now;
      
      if (FirstPurchase == default)
        FirstPurchase = DateTime.Now;
    }
    
    public void UpdateAfterSell(decimal tokens, decimal usdReceived, decimal price)
    {
      if (tokens > TokenBalance)
        throw new InvalidOperationException("Insufficient balance");
      
      // Calcular PnL realizado
      var costBasis = tokens * AverageBuyPrice;
      var profit = usdReceived - costBasis;
      RealizedPnL += profit;
      
      // Atualizar saldos
      TokenBalance -= tokens;
      TotalInvested = Math.Max(0, TotalInvested - costBasis);
      TotalSells++;
      LastTransaction = DateTime.Now;
      
      // Se vendeu tudo, resetar average price
      if (TokenBalance == 0)
      {
        AverageBuyPrice = 0;
        TotalInvested = 0;
      }
    }
  }
}