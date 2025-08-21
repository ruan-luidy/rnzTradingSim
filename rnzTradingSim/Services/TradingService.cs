using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public class TradingService : IDisposable
  {
    private readonly TradingDbContext _context;
    private readonly PlayerService _playerService;
    private readonly Random _random = new();
    private bool _disposed = false;
    
    // Trading parameters
    public const decimal TRADING_FEE = 0.003m; // 0.3%
    public const decimal MIN_TRADE_AMOUNT = 0.01m;
    public const decimal MAX_SLIPPAGE = 0.05m; // 5% max slippage
    
    public TradingService(TradingDbContext context, PlayerService playerService)
    {
      _context = context;
      _playerService = playerService;
    }
    
    public async Task<(bool success, string message, Trade? trade)> BuyTokenAsync(
      string coinId, 
      decimal usdAmount,
      decimal maxSlippage = 0.01m)
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      
      try
      {
        var player = _playerService.GetCurrentPlayer();
        var coin = await _context.UserCoins.FindAsync(coinId);
        
        if (coin == null)
          return (false, "Coin not found", null);
        
        if (coin.IsRugged)
          return (false, "This coin has been rugged!", null);
        
        if (player.Balance < usdAmount)
          return (false, "Insufficient balance", null);
        
        if (usdAmount < MIN_TRADE_AMOUNT)
          return (false, $"Minimum trade amount is ${MIN_TRADE_AMOUNT}", null);
        
        // Calcular swap usando AMM (x * y = k)
        var (tokensOut, newPrice) = coin.SimulateBuy(usdAmount);
        
        if (tokensOut <= 0)
          return (false, "Invalid liquidity pool", null);
        
        // Calcular price impact e verificar slippage
        var priceImpact = coin.CalculatePriceImpact(usdAmount, true);
        
        if (priceImpact > maxSlippage * 100)
          return (false, $"Price impact too high: {priceImpact:N2}%", null);
        
        // Aplicar trading fee
        var fee = usdAmount * TRADING_FEE;
        var actualTokensOut = tokensOut * (1 - TRADING_FEE);
        
        // Atualizar pool
        coin.PoolBaseAmount += usdAmount;
        coin.PoolTokenAmount -= actualTokensOut;
        coin.CurrentPrice = newPrice;
        coin.Volume24h += usdAmount;
        coin.TotalTransactions++;
        
        // Atualizar ATH
        if (newPrice > coin.AllTimeHigh)
          coin.AllTimeHigh = newPrice;
        
        // Obter ou criar portfolio
        var portfolio = await _context.Portfolios
          .FirstOrDefaultAsync(p => p.PlayerId == player.Id && p.CoinId == coinId);
        
        if (portfolio == null)
        {
          portfolio = new Portfolio
          {
            PlayerId = player.Id,
            CoinId = coinId
          };
          _context.Portfolios.Add(portfolio);
          coin.TotalHolders++;
        }
        
        portfolio.UpdateAfterBuy(actualTokensOut, usdAmount, newPrice);
        
        // Criar registro da trade
        var trade = new Trade
        {
          CoinId = coinId,
          PlayerId = player.Id,
          Type = TradeType.Buy,
          TokenAmount = actualTokensOut,
          UsdAmount = usdAmount,
          PricePerToken = usdAmount / actualTokensOut,
          PriceImpact = priceImpact,
          Slippage = maxSlippage * 100,
          TradingFee = fee,
          PoolTokenAfter = coin.PoolTokenAmount,
          PoolBaseAfter = coin.PoolBaseAmount,
          PriceAfter = newPrice
        };
        
        _context.Trades.Add(trade);
        
        // Atualizar balance do player
        player.Balance -= usdAmount;
        _playerService.SavePlayer(player);
        
        // Atualizar coin
        coin.LastUpdated = DateTime.Now;
        _context.UserCoins.Update(coin);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // Detectar trade grande (> $5000)
        if (usdAmount > 5000)
        {
          NotificationService.NotifyBigTrade("BUY", coin.Symbol, actualTokensOut, usdAmount);
        }
        
        LoggingService.Info($"Player {player.Id} bought {actualTokensOut:N2} {coin.Symbol} for ${usdAmount:N2}");
        
        return (true, $"Bought {actualTokensOut:N2} {coin.Symbol}", trade);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        LoggingService.Error("Error executing buy trade", ex);
        return (false, "Trade failed", null);
      }
    }
    
    public async Task<(bool success, string message, Trade? trade)> SellTokenAsync(
      string coinId,
      decimal tokenAmount,
      decimal minSlippage = 0.01m)
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      
      try
      {
        var player = _playerService.GetCurrentPlayer();
        var coin = await _context.UserCoins.FindAsync(coinId);
        
        if (coin == null)
          return (false, "Coin not found", null);
        
        if (coin.IsRugged)
          return (false, "Cannot sell rugged coin", null);
        
        // Verificar portfolio
        var portfolio = await _context.Portfolios
          .FirstOrDefaultAsync(p => p.PlayerId == player.Id && p.CoinId == coinId);
        
        if (portfolio == null || portfolio.TokenBalance < tokenAmount)
          return (false, "Insufficient token balance", null);
        
        // Calcular swap
        var (usdOut, newPrice) = coin.SimulateSell(tokenAmount);
        
        if (usdOut <= 0)
          return (false, "Invalid liquidity pool", null);
        
        // Calcular price impact
        var priceImpact = coin.CalculatePriceImpact(tokenAmount, false);
        
        if (priceImpact > minSlippage * 100)
          return (false, $"Price impact too high: {priceImpact:N2}%", null);
        
        // Aplicar fee
        var fee = usdOut * TRADING_FEE;
        var actualUsdOut = usdOut - fee;
        
        // Detectar rug pull automático (crash > 20% e volume > $1000)
        bool isRugPull = priceImpact > 20 && actualUsdOut > 1000;
        if (isRugPull)
        {
          NotificationService.NotifyRugPull(coin.Symbol, coin.Name, priceImpact, actualUsdOut);
        }
        
        // Atualizar pool
        coin.PoolTokenAmount += tokenAmount;
        coin.PoolBaseAmount -= actualUsdOut;
        coin.CurrentPrice = newPrice;
        coin.Volume24h += actualUsdOut;
        coin.TotalTransactions++;
        
        // Atualizar ATL
        if (newPrice < coin.AllTimeLow)
          coin.AllTimeLow = newPrice;
        
        // Atualizar portfolio
        portfolio.UpdateAfterSell(tokenAmount, actualUsdOut, newPrice);
        
        // Se zerou o balance, diminuir holders
        if (portfolio.TokenBalance == 0)
        {
          coin.TotalHolders = Math.Max(0, coin.TotalHolders - 1);
        }
        
        // Criar trade
        var trade = new Trade
        {
          CoinId = coinId,
          PlayerId = player.Id,
          Type = TradeType.Sell,
          TokenAmount = tokenAmount,
          UsdAmount = actualUsdOut,
          PricePerToken = actualUsdOut / tokenAmount,
          PriceImpact = priceImpact,
          Slippage = minSlippage * 100,
          TradingFee = fee,
          PoolTokenAfter = coin.PoolTokenAmount,
          PoolBaseAfter = coin.PoolBaseAmount,
          PriceAfter = newPrice,
          RealizedPnL = portfolio.RealizedPnL
        };
        
        _context.Trades.Add(trade);
        
        // Atualizar player balance
        player.Balance += actualUsdOut;
        _playerService.SavePlayer(player);
        
        // Atualizar coin
        coin.LastUpdated = DateTime.Now;
        _context.UserCoins.Update(coin);
        _context.Portfolios.Update(portfolio);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // Detectar trade grande (> $5000)
        if (actualUsdOut > 5000)
        {
          NotificationService.NotifyBigTrade("SELL", coin.Symbol, tokenAmount, actualUsdOut);
        }
        
        LoggingService.Info($"Player {player.Id} sold {tokenAmount:N2} {coin.Symbol} for ${actualUsdOut:N2}");
        
        return (true, $"Sold {tokenAmount:N2} {coin.Symbol} for ${actualUsdOut:N2}", trade);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        LoggingService.Error("Error executing sell trade", ex);
        return (false, "Trade failed", null);
      }
    }
    
    public async Task<(bool success, string message, decimal amountStolen)> RugPullAsync(string coinId)
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      
      try
      {
        var player = _playerService.GetCurrentPlayer();
        var coin = await _context.UserCoins.FindAsync(coinId);
        
        if (coin == null)
          return (false, "Coin not found", 0);
        
        if (coin.CreatorId != player.Id.ToString())
          return (false, "You can only rug your own coins", 0);
        
        if (coin.IsRugged)
          return (false, "Already rugged", 0);
        
        if (coin.IsLocked && coin.LockEndDate > DateTime.Now)
          return (false, $"Liquidity locked until {coin.LockEndDate:yyyy-MM-dd}", 0);
        
        // Steal all liquidity
        var stolenAmount = coin.PoolBaseAmount;
        
        // Mark as rugged
        coin.IsRugged = true;
        coin.PoolBaseAmount = 0;
        coin.CurrentPrice = 0;
        coin.LastUpdated = DateTime.Now;
        
        // Create rug pull trade
        var rugTrade = new Trade
        {
          CoinId = coinId,
          PlayerId = player.Id,
          Type = TradeType.RugPull,
          TokenAmount = 0,
          UsdAmount = stolenAmount,
          PricePerToken = 0,
          PriceImpact = 100, // 100% impact
          PoolTokenAfter = coin.PoolTokenAmount,
          PoolBaseAfter = 0,
          PriceAfter = 0
        };
        
        _context.Trades.Add(rugTrade);
        
        // Give money to rugger
        player.Balance += stolenAmount;
        _playerService.SavePlayer(player);
        
        // Update coin
        _context.UserCoins.Update(coin);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        // Notificar rug pull manual
        NotificationService.NotifyRugPull(coin.Symbol, coin.Name, 100, stolenAmount);
        
        LoggingService.Warning($"RUG PULL! Player {player.Id} rugged {coin.Symbol} stealing ${stolenAmount:N2}");
        
        return (true, $"RUG PULLED! Stole ${stolenAmount:N2}", stolenAmount);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        LoggingService.Error("Error executing rug pull", ex);
        return (false, "Rug pull failed", 0);
      }
    }
    
    public async Task<List<Trade>> GetRecentTradesAsync(string? coinId = null, int count = 50)
    {
      var query = _context.Trades.AsQueryable();
      
      if (!string.IsNullOrEmpty(coinId))
        query = query.Where(t => t.CoinId == coinId);
      
      return await query
        .OrderByDescending(t => t.Timestamp)
        .Take(count)
        .Include(t => t.Coin)
        .Include(t => t.Player)
        .AsNoTracking()
        .ToListAsync();
    }
    
    public async Task<Portfolio?> GetPortfolioAsync(int playerId, string coinId)
    {
      return await _context.Portfolios
        .Include(p => p.Coin)
        .FirstOrDefaultAsync(p => p.PlayerId == playerId && p.CoinId == coinId);
    }
    
    public async Task<List<Portfolio>> GetPlayerPortfoliosAsync(int playerId)
    {
      return await _context.Portfolios
        .Where(p => p.PlayerId == playerId && p.TokenBalance > 0)
        .Include(p => p.Coin)
        .OrderByDescending(p => p.TokenBalance * (p.Coin != null ? p.Coin.CurrentPrice : 0))
        .AsNoTracking()
        .ToListAsync();
    }
    
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed && disposing)
      {
        _disposed = true;
      }
    }
  }
}