using rnzTradingSim.Models;
using rnzTradingSim.Data;
using Microsoft.EntityFrameworkCore;

namespace rnzTradingSim.Services
{
  public class FakeCoinService
  {
    private readonly TradingDbContext _db;
    private readonly List<Coin> _coins;
    private readonly Random _random = new();

    public FakeCoinService(TradingDbContext db)
    {
      _db = db;
      _coins = _db.Coins.ToList();
      if (_coins.Count == 0)
      {
        SeedInitialCoins();
      }
    }

    public List<CoinData> GetCoins(int page, int perPage)
    {
      var paged = _coins
        .OrderBy(c => c.MarketCapRank)
        .Skip((page - 1) * perPage)
        .Take(perPage)
        .ToList();

      return paged.Select(MapToCoinData).ToList();
    }

    public void UpdateAllCoins()
    {
      foreach (var coin in _coins)
      {
        UpdateCoinPrice(coin);
      }
      _db.SaveChanges();
    }

    public void UpdateCoinPrice(Coin coin)
    {
      // Simula variação de preço
      var pctChange = (decimal)(_random.NextDouble() * 8 - 4); // -4% a +4%
      var oldPrice = coin.CurrentPrice;
      var newPrice = Math.Max(0.0001m, oldPrice * (1 + pctChange / 100));
      var priceChange = newPrice - oldPrice;

      coin.PriceChange24h = priceChange;
      coin.PriceChangePercentage24h = pctChange;
      coin.CurrentPrice = newPrice;
      coin.LastUpdated = DateTime.Now;

      // Salva histórico
      var history = new CoinHistory
      {
        CoinId = coin.Id,
        Price = newPrice,
        Timestamp = DateTime.Now
      };
      _db.CoinHistories.Add(history);
    }

    public void SeedInitialCoins()
    {
      // Adicione suas moedas fictícias aqui
      var initialCoins = new List<Coin>
      {
        new Coin { Id = Guid.NewGuid().ToString(), Name = "RafaelCoin", Symbol = "RAF", CurrentPrice = 10, MarketCapValue = 1000000, Volume24h = 50000, MarketCapRank = 1, Image = "", LastUpdated = DateTime.Now },
        new Coin { Id = Guid.NewGuid().ToString(), Name = "NandoToken", Symbol = "NAN", CurrentPrice = 2, MarketCapValue = 500000, Volume24h = 20000, MarketCapRank = 2, Image = "", LastUpdated = DateTime.Now },
        new Coin { Id = Guid.NewGuid().ToString(), Name = "ZéBucks", Symbol = "ZEB", CurrentPrice = 0.5m, MarketCapValue = 200000, Volume24h = 10000, MarketCapRank = 3, Image = "", LastUpdated = DateTime.Now },
        // Adicione mais moedas conforme desejar
      };

      _db.Coins.AddRange(initialCoins);
      _db.SaveChanges();
      _coins.Clear();
      _coins.AddRange(_db.Coins.ToList());
    }

    private CoinData MapToCoinData(Coin coin)
    {
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
  }
}