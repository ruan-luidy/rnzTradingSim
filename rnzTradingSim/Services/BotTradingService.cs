using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using System.Timers;

namespace rnzTradingSim.Services
{
  public enum BotPersonality
  {
    Whale,        // Faz trades grandes, move mercado
    Scalper,      // Muitos trades pequenos
    HODLer,       // Compra e segura
    PanicSeller,  // Vende em quedas
    FOMOBuyer,    // Compra em altas
    Sniper,       // Compra coins novas
    Dumper        // Vende progressivamente
  }
  
  public class TradingBot
  {
    public string Name { get; set; }
    public BotPersonality Personality { get; set; }
    public decimal Balance { get; set; }
    public decimal RiskTolerance { get; set; } // 0-1
    public decimal TradeFrequency { get; set; } // Trades por hora
    public Dictionary<string, decimal> Holdings { get; set; } = new();
  }
  
  public class BotTradingService : IDisposable
  {
    private readonly TradingDbContext _context;
    private readonly Random _random = new();
    private readonly List<TradingBot> _bots = new();
    private System.Timers.Timer? _botTimer;
    private bool _disposed = false;
    
    // Bot names para simular traders reais
    private readonly string[] _botNames = {
      "CryptoWhale", "DiamondHands", "PaperHands", "MoonBoy", "BearKiller",
      "SatoshiJr", "VitalikFan", "DogeLord", "APEstrong", "ChadTrader",
      "WenLambo", "NGMI", "WAGMI", "Degen420", "ShillMaster",
      "RugDoctor", "FudBuster", "HodlGang", "PumpChaser", "DumpDetective"
    };
    
    public BotTradingService(TradingDbContext context)
    {
      _context = context;
      InitializeBots();
    }
    
    private void InitializeBots()
    {
      // Criar diferentes tipos de bots
      for (int i = 0; i < 20; i++)
      {
        var personality = (BotPersonality)_random.Next(0, 7);
        var bot = new TradingBot
        {
          Name = _botNames[i % _botNames.Length] + _random.Next(1, 999),
          Personality = personality,
          Balance = personality switch
          {
            BotPersonality.Whale => _random.Next(10000, 100000),
            BotPersonality.Sniper => _random.Next(5000, 20000),
            _ => _random.Next(100, 5000)
          },
          RiskTolerance = personality switch
          {
            BotPersonality.PanicSeller => 0.1m,
            BotPersonality.FOMOBuyer => 0.9m,
            BotPersonality.Whale => 0.7m,
            _ => (decimal)_random.NextDouble()
          },
          TradeFrequency = personality switch
          {
            BotPersonality.Scalper => _random.Next(10, 30),
            BotPersonality.HODLer => _random.Next(1, 3),
            _ => _random.Next(3, 10)
          }
        };
        
        _bots.Add(bot);
      }
      
      LoggingService.Info($"Initialized {_bots.Count} trading bots");
    }
    
    public void StartBotTrading()
    {
      if (_botTimer != null) return;
      
      _botTimer = new System.Timers.Timer(5000); // Executar a cada 5 segundos
      _botTimer.Elapsed += async (s, e) => await ExecuteBotTrades();
      _botTimer.Start();
      
      LoggingService.Info("Bot trading started");
    }
    
    public void StopBotTrading()
    {
      _botTimer?.Stop();
      _botTimer?.Dispose();
      _botTimer = null;
      
      LoggingService.Info("Bot trading stopped");
    }
    
    private async Task ExecuteBotTrades()
    {
      if (_disposed) return;
      
      try
      {
        // Selecionar alguns bots aleatórios para fazer trade
        var activeBots = _bots
          .Where(b => _random.NextDouble() < (double)(b.TradeFrequency / 720m)) // 720 = 5sec intervals per hour
          .Take(5)
          .ToList();
        
        foreach (var bot in activeBots)
        {
          await ExecuteBotAction(bot);
        }
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error in bot trading", ex);
      }
    }
    
    private async Task ExecuteBotAction(TradingBot bot)
    {
      try
      {
        // Pegar moedas disponíveis
        var coins = await _context.UserCoins
          .Where(c => !c.IsRugged && c.PoolBaseAmount > 10)
          .OrderByDescending(c => c.Volume24h)
          .Take(20)
          .ToListAsync();
        
        if (!coins.Any()) return;
        
        var targetCoin = SelectCoinForBot(bot, coins);
        if (targetCoin == null) return;
        
        // Decidir ação baseada na personalidade
        var action = DecideBotAction(bot, targetCoin);
        
        await ExecuteTrade(bot, targetCoin, action);
      }
      catch (Exception ex)
      {
        LoggingService.Debug($"Bot {bot.Name} trade failed: {ex.Message}");
      }
    }
    
    private UserCoin? SelectCoinForBot(TradingBot bot, List<UserCoin> coins)
    {
      return bot.Personality switch
      {
        BotPersonality.Whale => coins.OrderByDescending(c => c.MarketCap).FirstOrDefault(),
        BotPersonality.Sniper => coins.OrderBy(c => c.CreatedAt).FirstOrDefault(),
        BotPersonality.FOMOBuyer => coins.OrderByDescending(c => c.PriceChange24h).FirstOrDefault(),
        BotPersonality.PanicSeller => coins.OrderBy(c => c.PriceChange24h).FirstOrDefault(),
        _ => coins[_random.Next(coins.Count)]
      };
    }
    
    private (bool isBuy, decimal amount) DecideBotAction(TradingBot bot, UserCoin coin)
    {
      var priceChange = coin.PriceChange24h;
      var hasHoldings = bot.Holdings.ContainsKey(coin.Id) && bot.Holdings[coin.Id] > 0;
      
      var decision = bot.Personality switch
      {
        BotPersonality.FOMOBuyer => (priceChange > 0, true), // Compra em alta
        BotPersonality.PanicSeller => (priceChange < -5 && hasHoldings, false), // Vende em queda
        BotPersonality.HODLer => (!hasHoldings, true), // Só compra, nunca vende
        BotPersonality.Scalper => (_random.NextDouble() > 0.5, _random.NextDouble() > 0.5),
        BotPersonality.Dumper => (hasHoldings && _random.NextDouble() > 0.7, false),
        _ => (_random.NextDouble() > 0.5, _random.NextDouble() > 0.5)
      };
      
      // Calcular quantidade baseada na personalidade e risco
      decimal amount;
      if (decision.Item1) // Buy
      {
        var maxSpend = bot.Balance * bot.RiskTolerance * 0.1m; // Max 10% por trade
        amount = bot.Personality switch
        {
          BotPersonality.Whale => _random.Next(1000, 5000),
          BotPersonality.Scalper => _random.Next(10, 100),
          _ => _random.Next(50, 500)
        };
        amount = Math.Min(amount, Math.Min(maxSpend, bot.Balance * 0.5m));
      }
      else // Sell
      {
        if (!hasHoldings) return (false, 0);
        
        var holdings = bot.Holdings[coin.Id];
        amount = bot.Personality switch
        {
          BotPersonality.PanicSeller => holdings, // Vende tudo
          BotPersonality.Dumper => holdings * 0.2m, // Vende 20%
          _ => holdings * (decimal)_random.NextDouble()
        };
      }
      
      return (decision.Item1, amount);
    }
    
    private async Task ExecuteTrade(TradingBot bot, UserCoin coin, (bool isBuy, decimal amount) action)
    {
      if (action.amount <= 0) return;
      
      using var transaction = await _context.Database.BeginTransactionAsync();
      
      try
      {
        if (action.isBuy)
        {
          // Simular compra
          var (tokensOut, newPrice) = coin.SimulateBuy(action.amount);
          
          if (tokensOut <= 0) return;
          
          // Atualizar pool
          coin.PoolBaseAmount += action.amount;
          coin.PoolTokenAmount -= tokensOut;
          coin.CurrentPrice = newPrice;
          coin.Volume24h += action.amount;
          coin.TotalTransactions++;
          
          // Atualizar holdings do bot
          if (!bot.Holdings.ContainsKey(coin.Id))
            bot.Holdings[coin.Id] = 0;
          
          bot.Holdings[coin.Id] += tokensOut;
          bot.Balance -= action.amount;
          
          // Atualizar ATH
          if (newPrice > coin.AllTimeHigh)
            coin.AllTimeHigh = newPrice;
          
          LoggingService.Debug($"Bot {bot.Name} bought {tokensOut:N2} {coin.Symbol} for ${action.amount:N2}");
        }
        else
        {
          // Simular venda
          if (!bot.Holdings.ContainsKey(coin.Id) || bot.Holdings[coin.Id] < action.amount)
            return;
          
          var (usdOut, newPrice) = coin.SimulateSell(action.amount);
          
          if (usdOut <= 0) return;
          
          // Atualizar pool
          coin.PoolTokenAmount += action.amount;
          coin.PoolBaseAmount -= usdOut;
          coin.CurrentPrice = newPrice;
          coin.Volume24h += usdOut;
          coin.TotalTransactions++;
          
          // Atualizar holdings
          bot.Holdings[coin.Id] -= action.amount;
          bot.Balance += usdOut;
          
          // Atualizar ATL
          if (newPrice < coin.AllTimeLow)
            coin.AllTimeLow = newPrice;
          
          LoggingService.Debug($"Bot {bot.Name} sold {action.amount:N2} {coin.Symbol} for ${usdOut:N2}");
        }
        
        // Atualizar coin
        coin.LastUpdated = DateTime.Now;
        _context.UserCoins.Update(coin);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        LoggingService.Debug($"Bot trade failed: {ex.Message}");
      }
    }
    
    public List<string> GetActiveBots()
    {
      return _bots.Select(b => $"{b.Name} ({b.Personality})").ToList();
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
        StopBotTrading();
      }
    }
    
    ~BotTradingService()
    {
      Dispose(false);
    }
  }
}