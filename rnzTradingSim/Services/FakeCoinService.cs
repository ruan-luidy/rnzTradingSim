using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public class FakeCoinService
  {
    private readonly TradingDbContext _context;
    private readonly Random _random;

    public FakeCoinService(TradingDbContext context)
    {
      _context = context;
      _random = new Random();

      // Garantir que as tabelas existem e popular dados iniciais
      InitializeDatabase();
    }

    private void InitializeDatabase()
    {
      try
      {
        // Força a criação das tabelas
        _context.Database.EnsureCreated();

        // Verifica se já existem moedas no banco
        if (!_context.Coins.Any())
        {
          SeedInitialCoins();
        }

        System.Diagnostics.Debug.WriteLine("Fake coin database initialized successfully");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error initializing fake coin database: {ex.Message}");
      }
    }

    private void SeedInitialCoins()
    {
      var coins = new List<Coin>
      {
        new Coin
        {
          Id = "bitcoin",
          Name = "Bitcoin",
          Symbol = "BTC",
          Image = "https://assets.coingecko.com/coins/images/1/large/bitcoin.png",
          CurrentPrice = 45000.00m,
          MarketCapValue = 850000000000m,
          Volume24h = 25000000000m,
          PriceChange24h = 1200.50m,
          PriceChangePercentage24h = 2.75m,
          MarketCapRank = 1
        },
        new Coin
        {
          Id = "ethereum",
          Name = "Ethereum",
          Symbol = "ETH",
          Image = "https://assets.coingecko.com/coins/images/279/large/ethereum.png",
          CurrentPrice = 2800.00m,
          MarketCapValue = 335000000000m,
          Volume24h = 12000000000m,
          PriceChange24h = -45.20m,
          PriceChangePercentage24h = -1.59m,
          MarketCapRank = 2
        },
        new Coin
        {
          Id = "tether",
          Name = "Tether USDt",
          Symbol = "USDT",
          Image = "https://assets.coingecko.com/coins/images/325/large/Tether.png",
          CurrentPrice = 1.00m,
          MarketCapValue = 95000000000m,
          Volume24h = 45000000000m,
          PriceChange24h = 0.001m,
          PriceChangePercentage24h = 0.01m,
          MarketCapRank = 3
        },
        new Coin
        {
          Id = "binancecoin",
          Name = "BNB",
          Symbol = "BNB",
          Image = "https://assets.coingecko.com/coins/images/825/large/bnb-icon2_2x.png",
          CurrentPrice = 310.50m,
          MarketCapValue = 47000000000m,
          Volume24h = 1200000000m,
          PriceChange24h = 8.75m,
          PriceChangePercentage24h = 2.90m,
          MarketCapRank = 4
        },
        new Coin
        {
          Id = "solana",
          Name = "Solana",
          Symbol = "SOL",
          Image = "https://assets.coingecko.com/coins/images/4128/large/solana.png",
          CurrentPrice = 105.80m,
          MarketCapValue = 45000000000m,
          Volume24h = 2800000000m,
          PriceChange24h = 15.30m,
          PriceChangePercentage24h = 16.90m,
          MarketCapRank = 5
        },
        new Coin
        {
          Id = "ripple",
          Name = "XRP",
          Symbol = "XRP",
          Image = "https://assets.coingecko.com/coins/images/44/large/xrp-symbol-white-128.png",
          CurrentPrice = 0.52m,
          MarketCapValue = 28000000000m,
          Volume24h = 1100000000m,
          PriceChange24h = -0.015m,
          PriceChangePercentage24h = -2.80m,
          MarketCapRank = 6
        },
        new Coin
        {
          Id = "usd-coin",
          Name = "USDC",
          Symbol = "USDC",
          Image = "https://assets.coingecko.com/coins/images/6319/large/USD_Coin_icon.png",
          CurrentPrice = 1.00m,
          MarketCapValue = 26000000000m,
          Volume24h = 3200000000m,
          PriceChange24h = 0.0005m,
          PriceChangePercentage24h = 0.05m,
          MarketCapRank = 7
        },
        new Coin
        {
          Id = "staked-ether",
          Name = "Lido Staked Ether",
          Symbol = "STETH",
          Image = "https://assets.coingecko.com/coins/images/13442/large/steth_logo.png",
          CurrentPrice = 2795.00m,
          MarketCapValue = 25000000000m,
          Volume24h = 85000000m,
          PriceChange24h = -42.10m,
          PriceChangePercentage24h = -1.48m,
          MarketCapRank = 8
        },
        new Coin
        {
          Id = "cardano",
          Name = "Cardano",
          Symbol = "ADA",
          Image = "https://assets.coingecko.com/coins/images/975/large/cardano.png",
          CurrentPrice = 0.395m,
          MarketCapValue = 14000000000m,
          Volume24h = 285000000m,
          PriceChange24h = 0.012m,
          PriceChangePercentage24h = 3.15m,
          MarketCapRank = 9
        },
        new Coin
        {
          Id = "dogecoin",
          Name = "Dogecoin",
          Symbol = "DOGE",
          Image = "https://assets.coingecko.com/coins/images/5/large/dogecoin.png",
          CurrentPrice = 0.085m,
          MarketCapValue = 12500000000m,
          Volume24h = 650000000m,
          PriceChange24h = 0.008m,
          PriceChangePercentage24h = 10.35m,
          MarketCapRank = 10
        },
        new Coin
        {
          Id = "avalanche-2",
          Name = "Avalanche",
          Symbol = "AVAX",
          Image = "https://assets.coingecko.com/coins/images/12559/large/Avalanche_Circle_RedWhite_Trans.png",
          CurrentPrice = 28.50m,
          MarketCapValue = 11000000000m,
          Volume24h = 385000000m,
          PriceChange24h = 1.85m,
          PriceChangePercentage24h = 6.95m,
          MarketCapRank = 11
        },
        new Coin
        {
          Id = "tron",
          Name = "TRON",
          Symbol = "TRX",
          Image = "https://assets.coingecko.com/coins/images/1094/large/tron-logo.png",
          CurrentPrice = 0.165m,
          MarketCapValue = 14500000000m,
          Volume24h = 445000000m,
          PriceChange24h = 0.005m,
          PriceChangePercentage24h = 3.12m,
          MarketCapRank = 12
        }
      };

      _context.Coins.AddRange(coins);
      _context.SaveChanges();

      // Criar histórico inicial para cada moeda
      foreach (var coin in coins)
      {
        CreateInitialHistory(coin);
      }

      System.Diagnostics.Debug.WriteLine($"Seeded {coins.Count} initial coins");
    }

    private void CreateInitialHistory(Coin coin)
    {
      var history = new List<CoinHistory>();
      var currentDate = DateTime.Now.AddDays(-30);

      // Criar 30 dias de histórico
      for (int i = 0; i < 30; i++)
      {
        var variation = (decimal)(_random.NextDouble() * 0.1 - 0.05); // -5% a +5%
        var price = coin.CurrentPrice * (1 + variation);

        history.Add(new CoinHistory
        {
          CoinId = coin.Id,
          Price = Math.Round(price, 6),
          Timestamp = currentDate.AddDays(i)
        });
      }

      _context.CoinHistories.AddRange(history);
      _context.SaveChanges();
    }

    public List<CoinData> GetCoins(int page = 1, int pageSize = 12)
    {
      try
      {
        var skip = (page - 1) * pageSize;

        var coins = _context.Coins
          .OrderBy(c => c.MarketCapRank)
          .Skip(skip)
          .Take(pageSize)
          .ToList();

        return coins.Select(c => new CoinData
        {
          Id = c.Id,
          Name = c.Name,
          Symbol = c.Symbol,
          Image = c.Image,
          CurrentPrice = c.CurrentPrice,
          MarketCapValue = c.MarketCapValue,
          Volume24h = c.Volume24h,
          PriceChange24h = c.PriceChange24h,
          PriceChangePercentage24h = c.PriceChangePercentage24h,
          MarketCapRank = c.MarketCapRank,
          LastUpdated = c.LastUpdated
        }).ToList();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coins: {ex.Message}");
        return new List<CoinData>();
      }
    }

    public CoinData GetCoin(string id)
    {
      try
      {
        var coin = _context.Coins.FirstOrDefault(c => c.Id == id);
        if (coin == null) return null;

        return new CoinData
        {
          Id = coin.Id,
          Name = coin.Name,
          Symbol = coin.Symbol,
          Image = coin.Image,
          CurrentPrice = coin.CurrentPrice,
          MarketCapValue = coin.MarketCapValue,
          Volume24h = coin.Volume24h,
          PriceChange24h = coin.PriceChange24h,
          PriceChangePercentage24h = coin.PriceChangePercentage24h,
          MarketCapRank = coin.MarketCapRank,
          LastUpdated = coin.LastUpdated
        };
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coin {id}: {ex.Message}");
        return null;
      }
    }

    public void UpdateAllCoins()
    {
      try
      {
        var coins = _context.Coins.ToList();

        foreach (var coin in coins)
        {
          // Simular mudanças de preço (-10% a +10%)
          var variation = (decimal)(_random.NextDouble() * 0.2 - 0.1);
          var oldPrice = coin.CurrentPrice;
          var newPrice = Math.Round(oldPrice * (1 + variation), 6);

          // Evitar preços negativos ou muito baixos
          if (newPrice < 0.000001m) newPrice = 0.000001m;

          coin.CurrentPrice = newPrice;
          coin.PriceChange24h = Math.Round(newPrice - oldPrice, 6);
          coin.PriceChangePercentage24h = Math.Round((newPrice - oldPrice) / oldPrice * 100, 2);
          coin.LastUpdated = DateTime.Now;

          // Atualizar market cap baseado no novo preço
          coin.MarketCapValue = Math.Round(newPrice * 1000000000m, 2); // Suposição de supply

          // Atualizar volume (variação de -20% a +20%)
          var volumeVariation = (decimal)(_random.NextDouble() * 0.4 - 0.2);
          coin.Volume24h = Math.Round(coin.Volume24h * (1 + volumeVariation), 2);

          // Adicionar ao histórico
          _context.CoinHistories.Add(new CoinHistory
          {
            CoinId = coin.Id,
            Price = newPrice,
            Timestamp = DateTime.Now
          });
        }

        _context.SaveChanges();

        // Limpar histórico antigo (manter apenas últimos 365 dias)
        var cutoffDate = DateTime.Now.AddDays(-365);
        var oldHistory = _context.CoinHistories.Where(h => h.Timestamp < cutoffDate).ToList();
        if (oldHistory.Any())
        {
          _context.CoinHistories.RemoveRange(oldHistory);
          _context.SaveChanges();
        }

        System.Diagnostics.Debug.WriteLine($"Updated prices for {coins.Count} coins");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error updating coins: {ex.Message}");
      }
    }

    public List<CoinHistory> GetCoinHistory(string coinId, int days = 30)
    {
      try
      {
        var fromDate = DateTime.Now.AddDays(-days);

        return _context.CoinHistories
          .Where(h => h.CoinId == coinId && h.Timestamp >= fromDate)
          .OrderBy(h => h.Timestamp)
          .ToList();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coin history for {coinId}: {ex.Message}");
        return new List<CoinHistory>();
      }
    }

    public void AddCoin(Coin coin)
    {
      try
      {
        _context.Coins.Add(coin);
        _context.SaveChanges();

        CreateInitialHistory(coin);

        System.Diagnostics.Debug.WriteLine($"Added new coin: {coin.Name}");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error adding coin: {ex.Message}");
      }
    }

    public void DeleteCoin(string coinId)
    {
      try
      {
        var coin = _context.Coins.FirstOrDefault(c => c.Id == coinId);
        if (coin != null)
        {
          // Remover histórico primeiro (por causa da foreign key)
          var history = _context.CoinHistories.Where(h => h.CoinId == coinId).ToList();
          _context.CoinHistories.RemoveRange(history);

          // Remover moeda
          _context.Coins.Remove(coin);
          _context.SaveChanges();

          System.Diagnostics.Debug.WriteLine($"Deleted coin: {coin.Name}");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error deleting coin {coinId}: {ex.Message}");
      }
    }

    public int GetTotalCoinsCount()
    {
      try
      {
        return _context.Coins.Count();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting total coins count: {ex.Message}");
        return 0;
      }
    }

    public void Dispose()
    {
      _context?.Dispose();
    }
  }
}