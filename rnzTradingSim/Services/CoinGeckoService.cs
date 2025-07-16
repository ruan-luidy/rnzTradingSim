using System.Net.Http;
using System.Text.Json;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public class CoinGeckoService
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.coingecko.com/api/v3";

    public CoinGeckoService()
    {
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("User-Agent", "rnzTradingSim/1.0");
    }

    public async Task<List<CoinData>> GetCoinsAsync(int page = 1, int perPage = 12)
    {
      try
      {
        var url = $"{_baseUrl}/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={perPage}&page={page}&sparkline=false&price_change_percentage=24h";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
          System.Diagnostics.Debug.WriteLine($"CoinGecko API error: {response.StatusCode}");
          return GetFallbackCoins();
        }

        var json = await response.Content.ReadAsStringAsync();
        var coinGeckoData = JsonSerializer.Deserialize<CoinGeckoResponse[]>(json, new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        return coinGeckoData?.Select(MapToCoinData).ToList() ?? GetFallbackCoins();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error fetching coins: {ex.Message}");
        return GetFallbackCoins();
      }
    }

    private CoinData MapToCoinData(CoinGeckoResponse response)
    {
      return new CoinData
      {
        Id = response.Id ?? string.Empty,
        Name = response.Name ?? string.Empty,
        Symbol = response.Symbol?.ToUpper() ?? string.Empty,
        Image = response.Image ?? string.Empty,
        CurrentPrice = response.CurrentPrice,
        MarketCapValue = response.MarketCap,
        Volume24h = response.TotalVolume,
        PriceChange24h = response.PriceChange24h,
        PriceChangePercentage24h = response.PriceChangePercentage24h,
        MarketCapRank = response.MarketCapRank,
        LastUpdated = response.LastUpdated
      };
    }

    private List<CoinData> GetFallbackCoins()
    {
      // Fallback data when API is unavailable
      return new List<CoinData>
      {
        new CoinData
        {
          Id = "cardano",
          Name = "Cardano",
          Symbol = "ADA",
          Image = "https://assets.coingecko.com/coins/images/975/large/cardano.png",
          CurrentPrice = 0.45m,
          MarketCapValue = 15_000_000_000m,
          Volume24h = 500_000_000m,
          PriceChangePercentage24h = -3.2m,
          MarketCapRank = 8,
          LastUpdated = DateTime.Now.AddMinutes(-2)
        },
        new CoinData
        {
          Id = "dogecoin",
          Name = "Dogecoin",
          Symbol = "DOGE",
          Image = "https://assets.coingecko.com/coins/images/5/large/dogecoin.png",
          CurrentPrice = 0.08m,
          MarketCapValue = 11_000_000_000m,
          Volume24h = 800_000_000m,
          PriceChangePercentage24h = 35.4m,
          MarketCapRank = 9,
          LastUpdated = DateTime.Now.AddMinutes(-4)
        },
        new CoinData
        {
          Id = "chainlink",
          Name = "Chainlink",
          Symbol = "LINK",
          Image = "https://assets.coingecko.com/coins/images/877/large/chainlink-new-logo.png",
          CurrentPrice = 14.50m,
          MarketCapValue = 8_000_000_000m,
          Volume24h = 400_000_000m,
          PriceChangePercentage24h = 8.7m,
          MarketCapRank = 12,
          LastUpdated = DateTime.Now.AddMinutes(-6)
        },
        new CoinData
        {
          Id = "polygon",
          Name = "Polygon",
          Symbol = "MATIC",
          Image = "https://assets.coingecko.com/coins/images/4713/large/matic-token-icon.png",
          CurrentPrice = 0.85m,
          MarketCapValue = 7_500_000_000m,
          Volume24h = 350_000_000m,
          PriceChangePercentage24h = -5.1m,
          MarketCapRank = 15,
          LastUpdated = DateTime.Now.AddMinutes(-7)
        },
        new CoinData
        {
          Id = "avalanche-2",
          Name = "Avalanche",
          Symbol = "AVAX",
          Image = "https://assets.coingecko.com/coins/images/12559/large/coin-round-red.png",
          CurrentPrice = 35.20m,
          MarketCapValue = 13_000_000_000m,
          Volume24h = 600_000_000m,
          PriceChangePercentage24h = 12.3m,
          MarketCapRank = 11,
          LastUpdated = DateTime.Now.AddMinutes(-3)
        },
        new CoinData
        {
          Id = "shiba-inu",
          Name = "Shiba Inu",
          Symbol = "SHIB",
          Image = "https://assets.coingecko.com/coins/images/11939/large/shiba.png",
          CurrentPrice = 0.000008m,
          MarketCapValue = 4_500_000_000m,
          Volume24h = 200_000_000m,
          PriceChangePercentage24h = 42.8m,
          MarketCapRank = 18,
          LastUpdated = DateTime.Now.AddMinutes(-5)
        },
        new CoinData
        {
          Id = "litecoin",
          Name = "Litecoin",
          Symbol = "LTC",
          Image = "https://assets.coingecko.com/coins/images/2/large/litecoin.png",
          CurrentPrice = 72.30m,
          MarketCapValue = 5_300_000_000m,
          Volume24h = 300_000_000m,
          PriceChangePercentage24h = -2.1m,
          MarketCapRank = 16,
          LastUpdated = DateTime.Now.AddMinutes(-8)
        },
        new CoinData
        {
          Id = "uniswap",
          Name = "Uniswap",
          Symbol = "UNI",
          Image = "https://assets.coingecko.com/coins/images/12504/large/uniswap-uni.png",
          CurrentPrice = 6.20m,
          MarketCapValue = 4_700_000_000m,
          Volume24h = 180_000_000m,
          PriceChangePercentage24h = 6.4m,
          MarketCapRank = 17,
          LastUpdated = DateTime.Now.AddMinutes(-4)
        },
        new CoinData
        {
          Id = "polkadot",
          Name = "Polkadot",
          Symbol = "DOT",
          Image = "https://assets.coingecko.com/coins/images/12171/large/polkadot.png",
          CurrentPrice = 5.80m,
          MarketCapValue = 7_800_000_000m,
          Volume24h = 250_000_000m,
          PriceChangePercentage24h = -1.8m,
          MarketCapRank = 13,
          LastUpdated = DateTime.Now.AddMinutes(-6)
        }
      };
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }

  public class CoinGeckoResponse
  {
    public string Id { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal MarketCap { get; set; }
    public int MarketCapRank { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal PriceChange24h { get; set; }
    public decimal PriceChangePercentage24h { get; set; }
    public DateTime LastUpdated { get; set; }
  }
}
d = "bitcoin",
          Name = "Bitcoin",
          Symbol = "BTC",
          Image = "https://assets.coingecko.com/coins/images/1/large/bitcoin.png",
          CurrentPrice = 42000m,
          MarketCapValue = 800_000_000_000m,
          Volume24h = 20_000_000_000m,
          PriceChangePercentage24h = 2.5m,
          MarketCapRank = 1,
          LastUpdated = DateTime.Now.AddMinutes(-5)
        },
        new CoinData
        {
          Id = "ethereum",
          Name = "Ethereum",
          Symbol = "ETH",
          Image = "https://assets.coingecko.com/coins/images/279/large/ethereum.png",
          CurrentPrice = 2500m,
          MarketCapValue = 300_000_000_000m,
          Volume24h = 15_000_000_000m,
          PriceChangePercentage24h = -1.2m,
          MarketCapRank = 2,
          LastUpdated = DateTime.Now.AddMinutes(-3)
        },
        new CoinData
        {
          Id = "solana",
          Name = "Solana",
          Symbol = "SOL",
          Image = "https://assets.coingecko.com/coins/images/4128/large/solana.png",
          CurrentPrice = 95m,
          MarketCapValue = 40_000_000_000m,
          Volume24h = 2_000_000_000m,
          PriceChangePercentage24h = 15.8m,
          MarketCapRank = 5,
          LastUpdated = DateTime.Now.AddMinutes(-1)
        },
        new CoinData
        {
          I