using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using System.Collections.Concurrent;

namespace rnzTradingSim.Services
{
  public class MarketSimulationService : IDisposable
  {
    private readonly TradingDbContext _context;
    private readonly Random _random = new();
    private readonly System.Timers.Timer _marketTimer;
    private readonly System.Timers.Timer _eventTimer;
    private readonly ConcurrentDictionary<string, MarketTrend> _coinTrends = new();
    private bool _disposed = false;

    // Market parameters
    private const int MARKET_UPDATE_INTERVAL_MS = 30000; // 30 seconds
    private const int EVENT_CHECK_INTERVAL_MS = 120000; // 2 minutes
    private const decimal MAX_PRICE_CHANGE = 0.05m; // 5% max per update
    private const decimal WHALE_TRADE_THRESHOLD = 25000m; // $25k+ = whale

    public MarketSimulationService(TradingDbContext context)
    {
      _context = context;
      
      InitializeMarketTrends();
      
      // Timer for price updates
      _marketTimer = new System.Timers.Timer(MARKET_UPDATE_INTERVAL_MS);
      _marketTimer.Elapsed += OnMarketUpdate;
      _marketTimer.AutoReset = true;
      _marketTimer.Start();

      // Timer for special events
      _eventTimer = new System.Timers.Timer(EVENT_CHECK_INTERVAL_MS);
      _eventTimer.Elapsed += OnEventCheck;
      _eventTimer.AutoReset = true;
      _eventTimer.Start();

      LoggingService.Info("Market simulation started");
    }

    private enum MarketTrend
    {
      Bullish,    // Tends to go up
      Bearish,    // Tends to go down
      Sideways,   // Consolidation
      Volatile,   // Random big moves
      Pumping,    // Artificial pump
      Dumping     // Getting rugged
    }

    private void InitializeMarketTrends()
    {
      Task.Run(async () =>
      {
        try
        {
          var coins = await _context.UserCoins.Where(c => !c.IsRugged).ToListAsync();
          
          foreach (var coin in coins)
          {
            var trend = (MarketTrend)_random.Next(0, 6);
            _coinTrends.TryAdd(coin.Id, trend);
          }
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error initializing market trends", ex);
        }
      });
    }

    private async void OnMarketUpdate(object? sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        await UpdateCoinPrices();
        await SimulateRandomTrades();
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error in market update", ex);
      }
    }

    private async void OnEventCheck(object? sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        await CheckForSpecialEvents();
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error in event check", ex);
      }
    }

    private async Task UpdateCoinPrices()
    {
      var coins = await _context.UserCoins
        .Where(c => !c.IsRugged && c.CurrentPrice > 0)
        .ToListAsync();

      foreach (var coin in coins.Take(20)) // Update top 20 coins to avoid performance issues
      {
        var trend = _coinTrends.GetValueOrDefault(coin.Id, MarketTrend.Sideways);
        var priceChange = CalculatePriceChange(trend);
        var oldPrice = coin.CurrentPrice;
        
        // Apply price change
        coin.CurrentPrice = Math.Max(0.00000001m, coin.CurrentPrice * (1 + priceChange));
        
        // Update 24h change
        var changePercent = ((coin.CurrentPrice - oldPrice) / oldPrice) * 100;
        coin.PriceChange24h += changePercent;
        
        // Update ATH/ATL
        if (coin.CurrentPrice > coin.AllTimeHigh)
        {
          coin.AllTimeHigh = coin.CurrentPrice;
          
          // Notify if big pump (>50% from previous ATH)
          if (changePercent > 50)
          {
            NotificationService.NotifyPriceAlert(coin.Symbol, coin.CurrentPrice, changePercent);
          }
        }
        
        if (coin.CurrentPrice < coin.AllTimeLow)
        {
          coin.AllTimeLow = coin.CurrentPrice;
        }

        // Simulate volume
        var volumeChange = _random.NextDouble() * 0.2 - 0.1; // ±10%
        coin.Volume24h = Math.Max(0, coin.Volume24h * (1 + (decimal)volumeChange));

        coin.LastUpdated = DateTime.Now;
      }

      await _context.SaveChangesAsync();
    }

    private decimal CalculatePriceChange(MarketTrend trend)
    {
      var baseChange = (_random.NextDouble() - 0.5) * 0.02; // ±1% base

      return trend switch
      {
        MarketTrend.Bullish => (decimal)(baseChange + _random.NextDouble() * 0.03), // +0-3% bias
        MarketTrend.Bearish => (decimal)(baseChange - _random.NextDouble() * 0.03), // -0-3% bias
        MarketTrend.Sideways => (decimal)(baseChange * 0.5), // Reduced volatility
        MarketTrend.Volatile => (decimal)((_random.NextDouble() - 0.5) * 0.1), // ±5% swings
        MarketTrend.Pumping => (decimal)(_random.NextDouble() * 0.08), // 0-8% up
        MarketTrend.Dumping => (decimal)(-_random.NextDouble() * 0.12), // 0-12% down
        _ => (decimal)baseChange
      };
    }

    private async Task SimulateRandomTrades()
    {
      var activeCoins = await _context.UserCoins
        .Where(c => !c.IsRugged && c.CurrentPrice > 0)
        .Take(10)
        .ToListAsync();

      foreach (var coin in activeCoins)
      {
        // 15% chance of simulated trade per coin per update
        if (_random.NextDouble() < 0.15)
        {
          var tradeValue = GenerateRandomTradeValue();
          var isWhale = tradeValue > WHALE_TRADE_THRESHOLD;
          
          if (isWhale)
          {
            var tradeType = _random.NextDouble() > 0.5 ? "BUY" : "SELL";
            var tokenAmount = tradeValue / coin.CurrentPrice;
            
            NotificationService.NotifyBigTrade(tradeType, coin.Symbol, tokenAmount, tradeValue);
          }

          // Update coin volume
          coin.Volume24h += tradeValue;
          coin.TotalTransactions++;
        }
      }

      await _context.SaveChangesAsync();
    }

    private decimal GenerateRandomTradeValue()
    {
      var roll = _random.NextDouble();
      
      return roll switch
      {
        < 0.7 => (decimal)(_random.NextDouble() * 500 + 10), // $10-510 (70% - small trades)
        < 0.9 => (decimal)(_random.NextDouble() * 2000 + 500), // $500-2500 (20% - medium)
        < 0.98 => (decimal)(_random.NextDouble() * 10000 + 2500), // $2.5k-12.5k (8% - large)
        _ => (decimal)(_random.NextDouble() * 50000 + 10000) // $10k-60k (2% - whale)
      };
    }

    private async Task CheckForSpecialEvents()
    {
      // 5% chance of special event per check
      if (_random.NextDouble() < 0.05)
      {
        await TriggerSpecialEvent();
      }

      // Randomly change market trends
      await UpdateMarketTrends();
    }

    private async Task TriggerSpecialEvent()
    {
      var eventType = _random.Next(0, 4);
      var coins = await _context.UserCoins.Where(c => !c.IsRugged).Take(10).ToListAsync();
      
      if (!coins.Any()) return;

      var targetCoin = coins[_random.Next(coins.Count)];

      switch (eventType)
      {
        case 0: // Pump event
          _coinTrends.TryUpdate(targetCoin.Id, MarketTrend.Pumping, _coinTrends.GetValueOrDefault(targetCoin.Id));
          NotificationService.ShowNotification($"🚀 {targetCoin.Symbol} is pumping! Volume spiking!", NotificationService.NotificationType.Info);
          break;

        case 1: // Dump event
          _coinTrends.TryUpdate(targetCoin.Id, MarketTrend.Dumping, _coinTrends.GetValueOrDefault(targetCoin.Id));
          NotificationService.ShowNotification($"📉 {targetCoin.Symbol} is dumping hard!", NotificationService.NotificationType.Warning);
          break;

        case 2: // Whale alert
          var whaleAmount = (decimal)(_random.NextDouble() * 100000 + 50000); // $50k-150k
          NotificationService.ShowNotification($"🐋 WHALE ALERT! Someone just moved ${whaleAmount:N0} in {targetCoin.Symbol}!", NotificationService.NotificationType.BigTrade);
          break;

        case 3: // Random rug pull (very rare)
          if (_random.NextDouble() < 0.1) // Only 10% chance when this event is selected
          {
            targetCoin.IsRugged = true;
            targetCoin.CurrentPrice = 0;
            targetCoin.PoolBaseAmount = 0;
            await _context.SaveChangesAsync();
            
            NotificationService.NotifyRugPull(targetCoin.Symbol, targetCoin.Name, 100m, targetCoin.PoolBaseAmount);
          }
          break;
      }
    }

    private async Task UpdateMarketTrends()
    {
      // 20% chance to change trend for each coin
      var coinIds = _coinTrends.Keys.ToList();
      
      foreach (var coinId in coinIds.Take(10)) // Limit to avoid performance issues
      {
        if (_random.NextDouble() < 0.2)
        {
          var newTrend = (MarketTrend)_random.Next(0, 6);
          _coinTrends.TryUpdate(coinId, newTrend, _coinTrends[coinId]);
        }
      }
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
        _marketTimer?.Stop();
        _marketTimer?.Dispose();
        
        _eventTimer?.Stop();
        _eventTimer?.Dispose();

        _disposed = true;
        LoggingService.Info("Market simulation stopped");
      }
    }
  }
}