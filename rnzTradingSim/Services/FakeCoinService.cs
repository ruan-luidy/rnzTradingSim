using Microsoft.EntityFrameworkCore;
using rnzTradingSim.Data;
using rnzTradingSim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace rnzTradingSim.Services
{
  public class FakeCoinService
  {
    private readonly TradingDbContext _context;
    private readonly Random _random;
    private readonly List<string> _coinNames;
    private readonly List<string> _coinSymbols;

    public FakeCoinService(TradingDbContext context)
    {
      _context = context;
      _random = new Random();

      // Lista de nomes e símbolos de moedas fake
      _coinNames = new List<string>
      {
        "Bitcoin", "Ethereum", "Binance Coin", "Cardano", "Solana",
        "XRP", "Polkadot", "Dogecoin", "Avalanche", "Chainlink",
        "Polygon", "Uniswap", "Litecoin", "Internet Computer", "Bitcoin Cash",
        "Stellar", "VeChain", "Filecoin", "TRON", "Ethereum Classic",
        "Monero", "Hedera", "Cosmos", "Cronos", "Near Protocol",
        "Flow", "Elrond", "Tezos", "Mina", "Zcash",
        "Decentraland", "The Sandbox", "Axie Infinity", "Enjin Coin", "Gala",
        "ApeCoin", "Shiba Inu", "Floki Inu", "SafeMoon", "Baby Doge",
        "PancakeSwap", "SushiSwap", "Compound", "Aave", "MakerDAO",
        "Yearn Finance", "Curve DAO", "1inch", "0x Protocol", "Balancer",
        "Synthetix", "REN", "Band Protocol", "Kyber Network", "Loopring",
        "Zilliqa", "Ontology", "Qtum", "Waves", "NEM",
        "Verge", "DigiByte", "Siacoin", "Nano", "IOTA",
        "Theta Network", "Helium", "Arweave", "The Graph", "Livepeer",
        "Basic Attention Token", "Brave", "Golem", "Status", "OmiseGO",
        "Augur", "Gnosis", "iExec RLC", "Storj", "Civic",
        "PowerLedger", "WazirX", "Holo", "Fetch.ai", "Ocean Protocol",
        "SingularityNET", "Numeraire", "Cortex", "DeepBrain Chain", "Matrix AI",
        "ChainGuardian", "Moonbeam", "Kusama", "Acala", "Karura",
        "Parallel Finance", "Bifrost", "Centrifuge", "HydraDX", "Basilisk",
        "Kintsugi", "Shiden", "Astar", "Phala Network", "Khala",
        "Clover Finance", "Sakura", "Calamari", "Robonomics", "KILT Protocol",
        "SubDAO", "Darwinia", "Crab Network", "Moonriver", "SolarBeam",
        "RenzoMoon", "GoldenCrypto", "DiamondHands", "SatoshiVision", "CryptoKing"
      };

      _coinSymbols = new List<string>
      {
        "BTC", "ETH", "BNB", "ADA", "SOL",
        "XRP", "DOT", "DOGE", "AVAX", "LINK",
        "MATIC", "UNI", "LTC", "ICP", "BCH",
        "XLM", "VET", "FIL", "TRX", "ETC",
        "XMR", "HBAR", "ATOM", "CRO", "NEAR",
        "FLOW", "EGLD", "XTZ", "MINA", "ZEC",
        "MANA", "SAND", "AXS", "ENJ", "GALA",
        "APE", "SHIB", "FLOKI", "SFM", "BABYDOGE",
        "CAKE", "SUSHI", "COMP", "AAVE", "MKR",
        "YFI", "CRV", "1INCH", "ZRX", "BAL",
        "SNX", "REN", "BAND", "KNC", "LRC",
        "ZIL", "ONT", "QTUM", "WAVES", "XEM",
        "XVG", "DGB", "SC", "NANO", "MIOTA",
        "THETA", "HNT", "AR", "GRT", "LPT",
        "BAT", "BRAVE", "GNT", "SNT", "OMG",
        "REP", "GNO", "RLC", "STORJ", "CVC",
        "POWR", "WRX", "HOT", "FET", "OCEAN",
        "AGIX", "NMR", "CTXC", "DBC", "MAN",
        "CGG", "GLMR", "KSM", "ACA", "KAR",
        "PARA", "BNC", "CFG", "HDX", "BSX",
        "KINT", "SDN", "ASTR", "PHA", "KPHA",
        "CLV", "SKU", "KMA", "XRT", "KILT",
        "GOV", "RING", "CRAB", "MOVR", "SOLAR",
        "RENZO", "GOLD", "DIAMOND", "SATOSHI", "KING"
      };

      // Inicializar moedas se o banco estiver vazio
      InitializeCoinsIfEmpty();
    }

    private void InitializeCoinsIfEmpty()
    {
      try
      {
        if (!_context.Coins.Any())
        {
          System.Diagnostics.Debug.WriteLine("Initializing fake coins...");
          CreateFakeCoins();
        }
        else
        {
          System.Diagnostics.Debug.WriteLine($"Coins already exist: {_context.Coins.Count()} coins found");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error checking/initializing coins: {ex.Message}");
      }
    }

    private void CreateFakeCoins()
    {
      try
      {
        var coins = new List<Coin>();

        // Criar moedas principais primeiro (Bitcoin, Ethereum, etc.)
        for (int i = 0; i < Math.Min(_coinNames.Count, _coinSymbols.Count); i++)
        {
          var coin = new Coin
          {
            Id = Guid.NewGuid().ToString(),
            Name = _coinNames[i],
            Symbol = _coinSymbols[i],
            Image = $"https://via.placeholder.com/32x32?text={_coinSymbols[i]}",
            CurrentPrice = GeneratePrice(i + 1),
            MarketCapRank = i + 1,
            LastUpdated = DateTime.Now
          };

          // Calcular market cap baseado no rank
          coin.MarketCapValue = coin.CurrentPrice * GenerateSupply(coin.MarketCapRank);
          coin.Volume24h = coin.MarketCapValue * ((decimal)_random.NextSingle() * 0.3m + 0.05m); // 5-35% do market cap

          // Gerar mudanças de preço realistas
          var changePercent = (_random.NextSingle() - 0.5f) * 40; // -20% a +20%
          coin.PriceChangePercentage24h = (decimal)changePercent;
          coin.PriceChange24h = coin.CurrentPrice * (coin.PriceChangePercentage24h / 100);

          coins.Add(coin);
        }

        // Adicionar ao banco
        _context.Coins.AddRange(coins);
        _context.SaveChanges();

        System.Diagnostics.Debug.WriteLine($"Created {coins.Count} fake coins successfully!");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error creating fake coins: {ex.Message}");
        throw;
      }
    }

    private decimal GeneratePrice(int rank)
    {
      // Preços mais realistas baseados no rank
      return rank switch
      {
        1 => 45000m + (_random.Next(-5000, 5000)), // Bitcoin ~$45k
        2 => 2800m + (_random.Next(-300, 300)),    // Ethereum ~$2.8k
        3 => 320m + (_random.Next(-30, 30)),       // BNB ~$320
        4 => 0.45m + (_random.Next(-5, 5) / 100m), // ADA ~$0.45
        5 => 55m + (_random.Next(-10, 10)),        // SOL ~$55
        _ when rank <= 10 => (decimal)(_random.NextDouble() * 50 + 5), // Top 10: $5-55
        _ when rank <= 50 => (decimal)(_random.NextDouble() * 10 + 0.5), // Top 50: $0.5-10.5
        _ when rank <= 100 => (decimal)(_random.NextDouble() * 2 + 0.1), // Top 100: $0.1-2.1
        _ => (decimal)(_random.NextDouble() * 0.5 + 0.001) // Outros: $0.001-0.501
      };
    }

    private decimal GenerateSupply(int rank)
    {
      // Supply baseado no rank (maior rank = menor supply geralmente)
      return rank switch
      {
        1 => 21_000_000m, // Bitcoin
        2 => 120_000_000m, // Ethereum
        _ when rank <= 10 => (decimal)(_random.NextDouble() * 500_000_000 + 100_000_000),
        _ when rank <= 50 => (decimal)(_random.NextDouble() * 5_000_000_000 + 500_000_000),
        _ => (decimal)(_random.NextDouble() * 100_000_000_000 + 1_000_000_000)
      };
    }

    public List<CoinData> GetCoins(int page = 1, int pageSize = 12)
    {
      try
      {
        var coins = _context.Coins
          .OrderBy(c => c.MarketCapRank)
          .Skip((page - 1) * pageSize)
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

    public int GetTotalCoinsCount()
    {
      try
      {
        return _context.Coins.Count();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error getting coins count: {ex.Message}");
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
          var priceChangePercent = (_random.NextSingle() - 0.5f) * 10; // -5% a +5%
          var newPrice = coin.CurrentPrice * (1 + (decimal)priceChangePercent / 100);

          // Garantir que o preço não seja negativo
          coin.CurrentPrice = Math.Max(newPrice, 0.0001m);

          // Atualizar mudança de 24h (acumular)
          coin.PriceChangePercentage24h += (decimal)priceChangePercent * 0.1m; // Pequena acumulação
          coin.PriceChange24h = coin.CurrentPrice * (coin.PriceChangePercentage24h / 100);

          // Atualizar market cap
          coin.MarketCapValue = coin.CurrentPrice * GenerateSupply(coin.MarketCapRank);
          coin.Volume24h = coin.MarketCapValue * ((decimal)_random.NextSingle() * 0.3m + 0.05m); 

          coin.LastUpdated = DateTime.Now;
        }

        _context.SaveChanges();
        System.Diagnostics.Debug.WriteLine($"Updated {coins.Count} coins");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error updating coins: {ex.Message}");
      }
    }

    public void RecreateAllCoins()
    {
      try
      {
        // Deletar todas as moedas existentes
        _context.Coins.RemoveRange(_context.Coins);
        _context.SaveChanges();

        // Recriar moedas
        CreateFakeCoins();
        System.Diagnostics.Debug.WriteLine("All coins recreated successfully!");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error recreating coins: {ex.Message}");
        throw;
      }
    }
  }
}