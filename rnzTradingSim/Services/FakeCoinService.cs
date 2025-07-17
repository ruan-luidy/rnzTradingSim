using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using System.Globalization;

namespace rnzTradingSim.Services
{
  public class FakeCoinService
  {
    private readonly TradingDbContext _context;
    private readonly Random _random;
    private static readonly List<string> CoinNames = new()
    {
      "Bitcoin", "Ethereum", "Binance Coin", "Cardano", "Solana", "XRP", "Polkadot", "Dogecoin",
      "Avalanche", "Polygon", "Shiba Inu", "Cosmos", "Near Protocol", "Algorand", "VeChain",
      "Internet Computer", "Hedera", "Filecoin", "Sandbox", "Decentraland", "Chainlink", "Uniswap",
      "Aave", "Compound", "Maker", "Sushi", "PancakeSwap", "1inch", "Curve", "Yearn Finance",
      "Synthetix", "Ren", "Loopring", "OMG Network", "Bancor", "Kyber Network", "0x Protocol",
      "Basic Attention Token", "Status", "District0x", "Golem", "Augur", "Gnosis", "Aragon",
      "Civic", "Storj", "Metal", "TenX", "OmiseGO", "Qtum", "Lisk", "Waves", "Stratis", "Ark",
      "Komodo", "Zcash", "Monero", "Dash", "Litecoin", "Bitcoin Cash", "Ethereum Classic",
      "Stellar", "NEO", "IOTA", "Tron", "EOS", "Tezos", "Cosmos", "Chainlink", "Polkadot"
    };

    private static readonly List<string> CoinSymbols = new()
    {
      "BTC", "ETH", "BNB", "ADA", "SOL", "XRP", "DOT", "DOGE", "AVAX", "MATIC", "SHIB", "ATOM",
      "NEAR", "ALGO", "VET", "ICP", "HBAR", "FIL", "SAND", "MANA", "LINK", "UNI", "AAVE", "COMP",
      "MKR", "SUSHI", "CAKE", "1INCH", "CRV", "YFI", "SNX", "REN", "LRC", "OMG", "BNT", "KNC",
      "ZRX", "BAT", "SNT", "DNT", "GNT", "REP", "GNO", "ANT", "CVC", "STORJ", "MTL", "PAY",
      "OMG", "QTUM", "LSK", "WAVES", "STRAT", "ARK", "KMD", "ZEC", "XMR", "DASH", "LTC", "BCH",
      "ETC", "XLM", "NEO", "IOTA", "TRX", "EOS", "XTZ", "ATOM", "LINK", "DOT"
    };

    public FakeCoinService(TradingDbContext context)
    {
      _context = context;
      _random = new Random();
      InitializeCoins();
    }

    private void InitializeCoins()
    {
      try
      {
        if (!_context.Coins.Any())
        {
          System.Diagnostics.Debug.WriteLine("Initializing fake coins...");
          GenerateFakeCoins();
        }
        else
        {
          System.Diagnostics.Debug.WriteLine($"Found {_context.Coins.Count()} existing coins");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error initializing coins: {ex.Message}");
      }
    }

    private void GenerateFakeCoins()
    {
      var coins = new List<Coin>();

      for (int i = 0; i < Math.Min(CoinNames.Count, CoinSymbols.Count); i++)
      {
        // Preços realistas baseados no ranking
        decimal basePrice = i switch
        {
          0 => _random.Next(45000, 65000), // BTC
          1 => _random.Next(2500, 4000),   // ETH
          < 5 => _random.Next(50, 500),    // Top 5
          < 10 => _random.Next(5, 50),     // Top 10
          < 20 => _random.Next(1, 10),     // Top 20
          < 50 => (decimal)_random.NextDouble() * 5, // Top 50
          _ => (decimal)_random.NextDouble() * 1      // Others
        };

        var priceChange = (decimal)(_random.NextDouble() * 20 - 10); // -10% a +10%
        var volume = (decimal)(_random.NextDouble() * 1000000000); // Up to 1B volume

        var coin = new Coin
        {
          Id = CoinSymbols[i].ToLower(),
          Name = CoinNames[i],
          Symbol = CoinSymbols[i],
          CurrentPrice = Math.Round(basePrice, 8),
          MarketCapRank = i + 1,
          PriceChange24h = Math.Round(basePrice * priceChange / 100, 8),
          PriceChangePercentage24h = Math.Round(priceChange, 4),
          Volume24h = Math.Round(volume, 2),
          MarketCapValue = Math.Round(basePrice * _random.Next(1000000, 100000000), 2),
          LastUpdated = DateTime.Now
        };

        coins.Add(coin);
      }

      _context.Coins.AddRange(coins);
      _context.SaveChanges();

      System.Diagnostics.Debug.WriteLine($"Generated {coins.Count} fake coins");
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

        return coins.Select(MapToCoinData).ToList();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coins: {ex.Message}");
        return new List<CoinData>();
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
        System.Diagnostics.Debug.WriteLine($"Error getting coin count: {ex.Message}");
        return 0;
      }
    }

    public void UpdateAllCoins()
    {
      try
      {
        var coins = _context.Coins.ToList();

        foreach (var coin in coins)
        {
          // Simular mudanças de preço pequenas (-5% a +5%)
          var priceChangePercent = (decimal)(_random.NextDouble() * 10 - 5);
          var priceChange = coin.CurrentPrice * priceChangePercent / 100;

          coin.CurrentPrice = Math.Max(0.00000001m, coin.CurrentPrice + priceChange);
          coin.PriceChange24h = priceChange;
          coin.PriceChangePercentage24h = priceChangePercent;
          coin.LastUpdated = DateTime.Now;

          // Atualizar market cap baseado no novo preço
          var marketCapChange = coin.MarketCapValue * priceChangePercent / 100;
          coin.MarketCapValue = Math.Max(1, coin.MarketCapValue + marketCapChange);

          // Simular mudanças no volume
          var volumeChangePercent = (decimal)(_random.NextDouble() * 20 - 10);
          var volumeChange = coin.Volume24h * volumeChangePercent / 100;
          coin.Volume24h = Math.Max(1000, coin.Volume24h + volumeChange);
        }

        _context.SaveChanges();
        System.Diagnostics.Debug.WriteLine($"Updated {coins.Count} coins");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error updating coins: {ex.Message}");
      }
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

    public CoinData GetCoinById(string id)
    {
      try
      {
        var coin = _context.Coins.FirstOrDefault(c => c.Id == id);
        return coin != null ? MapToCoinData(coin) : null;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coin by id: {ex.Message}");
        return null;
      }
    }

    public List<CoinData> SearchCoins(string searchTerm)
    {
      try
      {
        var coins = _context.Coins
            .Where(c => c.Name.Contains(searchTerm) || c.Symbol.Contains(searchTerm))
            .OrderBy(c => c.MarketCapRank)
            .Take(20)
            .ToList();

        return coins.Select(MapToCoinData).ToList();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error searching coins: {ex.Message}");
        return new List<CoinData>();
      }
    }

    public void Dispose()
    {
      _context?.Dispose();
    }
  }
}