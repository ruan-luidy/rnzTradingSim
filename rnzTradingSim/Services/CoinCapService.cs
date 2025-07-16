using System.Net.Http;
using System.Text.Json;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public class CoinCapService
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "rest.coincap.io/v3/";

    public CoinCapService()
    {
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("User-Agent", "rnzTradingSim/1.0");
    }

    public async Task<List<CoinData>> GetCoinsAsync(int page = 1, int perPage = 12)
    {
      try
      {
        // CoinCap API - TOTALMENTE GRATUITA, SEM LIMITES, SEM API KEY!
        var offset = (page - 1) * perPage;
        var url = $"{_baseUrl}/assets?limit={perPage}&offset={offset}";

        System.Diagnostics.Debug.WriteLine($"Fetching from CoinCap: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
          System.Diagnostics.Debug.WriteLine($"CoinCap API error: {response.StatusCode}");
          return new List<CoinData>();
        }

        var json = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"CoinCap Response: {json.Substring(0, Math.Min(200, json.Length))}...");

        var coinCapResponse = JsonSerializer.Deserialize<CoinCapResponse>(json, new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (coinCapResponse?.Data == null)
        {
          System.Diagnostics.Debug.WriteLine("Invalid CoinCap response structure");
          return new List<CoinData>();
        }

        var mappedCoins = coinCapResponse.Data.Select(MapToCoinData).ToList();

        System.Diagnostics.Debug.WriteLine($"Successfully mapped {mappedCoins.Count} coins from CoinCap");
        return mappedCoins;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error fetching from CoinCap: {ex.Message}");
        return new List<CoinData>();
      }
    }

    private CoinData MapToCoinData(CoinCapAsset asset)
    {
      try
      {
        // Parse valores com tratamento de nulos
        var currentPrice = ParseDecimalSafe(asset.PriceUsd);
        var marketCap = ParseDecimalSafe(asset.MarketCapUsd);
        var volume24h = ParseDecimalSafe(asset.VolumeUsd24Hr);
        var changePercent24h = ParseDecimalSafe(asset.ChangePercent24Hr);
        var supply = ParseDecimalSafe(asset.Supply);

        return new CoinData
        {
          Id = asset.Id ?? string.Empty,
          Name = asset.Name ?? string.Empty,
          Symbol = asset.Symbol?.ToUpper() ?? string.Empty,
          Image = GetCoinImageUrl(asset.Symbol ?? string.Empty),
          CurrentPrice = currentPrice,
          MarketCapValue = marketCap,
          Volume24h = volume24h,
          PriceChange24h = currentPrice * (changePercent24h / 100), // Calcular mudança absoluta
          PriceChangePercentage24h = changePercent24h,
          MarketCapRank = int.TryParse(asset.Rank, out int rank) ? rank : 999,
          LastUpdated = DateTime.Now // CoinCap não fornece timestamp específico
        };
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error mapping coin {asset.Name}: {ex.Message}");
        return null;
      }
    }

    private decimal ParseDecimalSafe(string value)
    {
      if (string.IsNullOrEmpty(value))
        return 0m;

      if (decimal.TryParse(value, out decimal result))
        return result;

      return 0m;
    }

    private string GetCoinImageUrl(string symbol)
    {
      if (string.IsNullOrEmpty(symbol))
        return "https://via.placeholder.com/64x64/ff6b35/white?text=?";

      // URLs de imagens das principais criptomoedas
      var imageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
        {"BTC", "https://assets.coingecko.com/coins/images/1/large/bitcoin.png"},
        {"ETH", "https://assets.coingecko.com/coins/images/279/large/ethereum.png"},
        {"USDT", "https://assets.coingecko.com/coins/images/325/large/Tether.png"},
        {"BNB", "https://assets.coingecko.com/coins/images/825/large/bnb-icon2_2x.png"},
        {"SOL", "https://assets.coingecko.com/coins/images/4128/large/solana.png"},
        {"XRP", "https://assets.coingecko.com/coins/images/44/large/xrp-symbol-white-128.png"},
        {"USDC", "https://assets.coingecko.com/coins/images/6319/large/USD_Coin_icon.png"},
        {"ADA", "https://assets.coingecko.com/coins/images/975/large/cardano.png"},
        {"AVAX", "https://assets.coingecko.com/coins/images/12559/large/coin-round-red.png"},
        {"DOGE", "https://assets.coingecko.com/coins/images/5/large/dogecoin.png"},
        {"TRX", "https://assets.coingecko.com/coins/images/1094/large/tron-logo.png"},
        {"DOT", "https://assets.coingecko.com/coins/images/12171/large/polkadot.png"},
        {"MATIC", "https://assets.coingecko.com/coins/images/4713/large/matic-token-icon.png"},
        {"LTC", "https://assets.coingecko.com/coins/images/2/large/litecoin.png"},
        {"SHIB", "https://assets.coingecko.com/coins/images/11939/large/shiba.png"},
        {"BCH", "https://assets.coingecko.com/coins/images/780/large/bitcoin-cash-circle.png"},
        {"NEAR", "https://assets.coingecko.com/coins/images/10365/large/near_icon.png"},
        {"UNI", "https://assets.coingecko.com/coins/images/12504/large/uniswap-uni.png"},
        {"ATOM", "https://assets.coingecko.com/coins/images/1481/large/cosmos_hub.png"},
        {"XLM", "https://assets.coingecko.com/coins/images/100/large/Stellar_symbol_black_RGB.png"},
        {"LINK", "https://assets.coingecko.com/coins/images/877/large/chainlink-new-logo.png"},
        {"XMR", "https://assets.coingecko.com/coins/images/69/large/monero_logo.png"},
        {"ETC", "https://assets.coingecko.com/coins/images/453/large/ethereum-classic-logo.png"},
        {"ALGO", "https://assets.coingecko.com/coins/images/4380/large/download.png"},
        {"VET", "https://assets.coingecko.com/coins/images/1077/large/vechain.png"},
        {"ICP", "https://assets.coingecko.com/coins/images/14495/large/Internet_Computer_logo.png"},
        {"APT", "https://assets.coingecko.com/coins/images/26455/large/aptos_round.png"},
        {"FIL", "https://assets.coingecko.com/coins/images/12817/large/filecoin.png"},
        {"HBAR", "https://assets.coingecko.com/coins/images/3688/large/hbar.png"},
        {"ARB", "https://assets.coingecko.com/coins/images/16547/large/photo_2023-03-29_21.47.00.jpeg"}
      };

      return imageMap.ContainsKey(symbol)
        ? imageMap[symbol]
        : $"https://via.placeholder.com/64x64/ff6b35/white?text={symbol}";
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }

  // Classes para deserializar a resposta da CoinCap API
  public class CoinCapResponse
  {
    public List<CoinCapAsset> Data { get; set; } = new List<CoinCapAsset>();
    public long Timestamp { get; set; }
  }

  public class CoinCapAsset
  {
    public string Id { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Supply { get; set; } = string.Empty;
    public string MaxSupply { get; set; } = string.Empty;
    public string MarketCapUsd { get; set; } = string.Empty;
    public string VolumeUsd24Hr { get; set; } = string.Empty;
    public string PriceUsd { get; set; } = string.Empty;
    public string ChangePercent24Hr { get; set; } = string.Empty;
    public string Vwap24Hr { get; set; } = string.Empty;
    public string Explorer { get; set; } = string.Empty;
  }
}