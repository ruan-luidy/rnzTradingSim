using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.Helpers;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels
{
  public partial class CoinDetailViewModel : ObservableObject, IDisposable
  {
    private readonly TradingService _tradingService;
    private readonly PlayerService _playerService;
    private readonly CoinCreationService _coinCreationService;
    private readonly Data.TradingDbContext _db;
    private bool _disposed = false;

    [ObservableProperty]
    private string coinId = string.Empty;

    [ObservableProperty]
    private UserCoin? currentCoin;

    [ObservableProperty]
    private Portfolio? userPortfolio;

    [ObservableProperty]
    private decimal playerBalance = 0m;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isChartLoading = false;

    [ObservableProperty]
    private string tradingMode = "Buy"; // Buy or Sell

    [ObservableProperty]
    private string tradeAmount = "0";

    [ObservableProperty]
    private decimal estimatedTokens = 0m;

    [ObservableProperty]
    private decimal estimatedUsd = 0m;

    [ObservableProperty]
    private decimal priceImpact = 0m;

    [ObservableProperty]
    private string lastTradeResult = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Trade> recentTrades = new();

    // Chart properties
    [ObservableProperty]
    private ISeries[] chartSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] xAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] yAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private string selectedTimeframe = "1m";

    // Trading properties
    [ObservableProperty]
    private bool isBuyMode = true;

    [ObservableProperty]
    private bool isSellMode = false;

    public CoinDetailViewModel()
    {
      try
      {
        _db = new Data.TradingDbContext();
        _playerService = new PlayerService();
        _tradingService = new TradingService(_db, _playerService);
        _coinCreationService = new CoinCreationService(_db, _playerService);
        
        InitializeChart();
        LoggingService.Info("CoinDetailViewModel initialized successfully");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing CoinDetailViewModel", ex);
      }
    }

    public async Task LoadCoinDataAsync(string coinId)
    {
      if (_disposed) return;

      CoinId = coinId;
      IsLoading = true;

      try
      {
        // Load coin data
        var coin = await _db.UserCoins.FindAsync(coinId);
        if (coin != null)
        {
          CurrentCoin = coin;
          
          // Load player data
          var player = _playerService.GetCurrentPlayer();
          PlayerBalance = player.Balance;

          // Load user portfolio for this coin
          UserPortfolio = await _tradingService.GetPortfolioAsync(player.Id, coinId);

          // Load recent trades
          await LoadRecentTradesAsync();

          // Load chart data
          await LoadChartDataAsync();

          LoggingService.Info($"Loaded coin data for {coin.Symbol}");
        }
      }
      catch (Exception ex)
      {
        LoggingService.Error($"Error loading coin data for {coinId}", ex);
      }
      finally
      {
        IsLoading = false;
      }
    }

    private void InitializeChart()
    {
      // Configure X-axis (time)
      XAxes = new[]
      {
        new Axis
        {
          Name = "Time",
          TextSize = 12,
          LabelsPaint = new SolidColorPaint(SKColors.Gray),
          SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 1 },
          ShowSeparatorLines = true
        }
      };

      // Configure Y-axis (price)
      YAxes = new[]
      {
        new Axis
        {
          Name = "Price (USD)",
          Position = LiveChartsCore.Measure.AxisPosition.Start,
          TextSize = 12,
          LabelsPaint = new SolidColorPaint(SKColors.Gray),
          SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 1 },
          ShowSeparatorLines = true,
          Labeler = value => value.ToString("C8")
        }
      };
    }

    private async Task LoadChartDataAsync()
    {
      if (CurrentCoin == null) return;

      IsChartLoading = true;

      try
      {
        await Task.Delay(200); // Simulate loading

        // Generate sample candlestick data
        var candlesticks = GenerateSampleCandlestickData();

        ChartSeries = new ISeries[]
        {
          new CandlesticksSeries<FinancialPoint>
          {
            Values = candlesticks,
            Name = $"{CurrentCoin.Symbol} Price",
            UpFill = new SolidColorPaint(SKColor.Parse("#16a34a")),
            UpStroke = new SolidColorPaint(SKColor.Parse("#16a34a")) { StrokeThickness = 2 },
            DownFill = new SolidColorPaint(SKColor.Parse("#ef4444")),
            DownStroke = new SolidColorPaint(SKColor.Parse("#ef4444")) { StrokeThickness = 2 }
          }
        };

        LoggingService.Info($"Loaded chart data for {CurrentCoin.Symbol}");
      }
      catch (Exception ex)
      {
        LoggingService.Error($"Error loading chart data for {CurrentCoin?.Symbol}", ex);
      }
      finally
      {
        IsChartLoading = false;
      }
    }

    private List<FinancialPoint> GenerateSampleCandlestickData()
    {
      if (CurrentCoin == null) return new List<FinancialPoint>();

      var data = new List<FinancialPoint>();
      var basePrice = (double)CurrentCoin.CurrentPrice;
      var random = new Random(CurrentCoin.Symbol.GetHashCode());
      var currentTime = DateTime.Now.AddHours(-24);

      for (int i = 0; i < 100; i++)
      {
        var open = basePrice + (random.NextDouble() - 0.5) * basePrice * 0.1;
        var volatility = random.NextDouble() * 0.05; // 5% max volatility
        
        var high = open + random.NextDouble() * open * volatility;
        var low = open - random.NextDouble() * open * volatility;
        var close = low + random.NextDouble() * (high - low);

        basePrice = close; // Next candle starts where this one ended

        data.Add(new FinancialPoint
        {
          Date = currentTime.AddMinutes(i * 15),
          High = high,
          Open = open,
          Close = close,
          Low = low
        });
      }

      return data;
    }

    private async Task LoadRecentTradesAsync()
    {
      if (CurrentCoin == null) return;

      try
      {
        var trades = await _tradingService.GetRecentTradesForCoinAsync(CurrentCoin.Id, 20);
        RecentTrades.Clear();
        foreach (var trade in trades)
        {
          RecentTrades.Add(trade);
        }

        LoggingService.Info($"Loaded {trades.Count} recent trades for {CurrentCoin.Symbol}");
      }
      catch (Exception ex)
      {
        LoggingService.Error($"Error loading recent trades for {CurrentCoin?.Symbol}", ex);
      }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
      if (!string.IsNullOrEmpty(CoinId))
      {
        await LoadCoinDataAsync(CoinId);
      }
    }

    [RelayCommand]
    private async Task ExecuteTradeAsync()
    {
      if (CurrentCoin == null || string.IsNullOrEmpty(TradeAmount)) return;

      try
      {
        var amount = decimal.Parse(TradeAmount);
        if (amount <= 0) return;

        var player = _playerService.GetCurrentPlayer();
        bool success = false;

        if (IsBuyMode)
        {
          var result = await _tradingService.BuyTokenAsync(CurrentCoin.Id, amount);
          success = result.success;
          if (success)
          {
            LastTradeResult = $"Successfully bought {amount:C2} worth of {CurrentCoin.Symbol}";
            NotificationService.NotifyTradingSuccess($"Bought {CurrentCoin.Symbol} for {amount:C2}");
          }
        }
        else
        {
          // For sell, amount is in tokens
          var result = await _tradingService.SellTokenAsync(CurrentCoin.Id, amount);
          success = result.success;
          if (success)
          {
            LastTradeResult = $"Successfully sold {amount:N8} {CurrentCoin.Symbol}";
            NotificationService.NotifyTradingSuccess($"Sold {amount:N8} {CurrentCoin.Symbol}");
          }
        }

        if (success)
        {
          // Refresh data after successful trade
          await LoadCoinDataAsync(CoinId);
          TradeAmount = "0";
        }
        else
        {
          LastTradeResult = "Trade failed. Please check your balance and try again.";
        }
      }
      catch (Exception ex)
      {
        LastTradeResult = $"Error: {ex.Message}";
        LoggingService.Error("Error executing trade", ex);
      }
    }

    [RelayCommand]
    private void SetMaxAmount()
    {
      if (CurrentCoin == null) return;

      if (IsBuyMode)
      {
        TradeAmount = PlayerBalance.ToString("F2");
      }
      else if (UserPortfolio != null)
      {
        TradeAmount = UserPortfolio.TokenBalance.ToString("F8");
      }
    }

    [RelayCommand]
    private async Task RugPullAsync()
    {
      if (CurrentCoin == null) return;

      try
      {
        var success = await _coinCreationService.RugPullAsync(CurrentCoin.Id);
        if (success)
        {
          LastTradeResult = $"💀 {CurrentCoin.Symbol} has been rugged! All liquidity stolen.";
          NotificationService.NotifyRugPull(CurrentCoin.Symbol, CurrentCoin.Name, 100m, (int)CurrentCoin.TotalSupply);
          await LoadCoinDataAsync(CoinId);
        }
      }
      catch (Exception ex)
      {
        LastTradeResult = $"Rug pull failed: {ex.Message}";
        LoggingService.Error("Error executing rug pull", ex);
      }
    }

    // Computed properties
    public string FormattedPrice => CurrentCoin?.CurrentPrice.ToString("C8") ?? "$0.00000000";
    public string FormattedPriceChange => CurrentCoin?.PriceChange24h.ToString("+0.00%;-0.00%;0.00%") ?? "0.00%";
    public bool IsPriceUp => CurrentCoin?.PriceChange24h >= 0;
    public string FormattedMarketCap => (CurrentCoin?.CurrentPrice * CurrentCoin?.CirculatingSupply ?? 0).FormatAbbreviated();
    public string FormattedVolume => CurrentCoin?.Volume24h.FormatAbbreviated() ?? "$0";
    public string FormattedHoldings => UserPortfolio?.TokenBalance.ToString("N8") ?? "0.00000000";
    public string FormattedHoldingsValue => UserPortfolio != null && CurrentCoin != null 
      ? (UserPortfolio.TokenBalance * CurrentCoin.CurrentPrice).ToString("C2") 
      : "$0.00";
    public string FormattedAvailableBalance => IsBuyMode 
      ? PlayerBalance.ToString("C2") 
      : FormattedHoldings;
    public string EstimatedReceiveFormatted => IsBuyMode 
      ? EstimatedTokens.ToString("N8") 
      : EstimatedUsd.ToString("C2");
    public string PriceImpactFormatted => $"{PriceImpact:F2}%";
    public bool CanTrade => CurrentCoin != null && !IsRugged && decimal.TryParse(TradeAmount, out var amount) && amount > 0;
    public bool IsRugged => CurrentCoin?.IsRugged ?? false;
    public bool CanRugPull => CurrentCoin?.CreatorId.ToString() == _playerService.GetCurrentPlayer().Id.ToString() && !IsRugged;

    // Update trading mode
    partial void OnIsBuyModeChanged(bool value)
    {
      if (value) IsSellMode = false;
      CalculateTradeEstimates();
    }

    partial void OnIsSellModeChanged(bool value)
    {
      if (value) IsBuyMode = false;
      CalculateTradeEstimates();
    }

    partial void OnTradeAmountChanged(string value)
    {
      CalculateTradeEstimates();
    }

    private void CalculateTradeEstimates()
    {
      if (CurrentCoin == null || !decimal.TryParse(TradeAmount, out var amount) || amount <= 0)
      {
        EstimatedTokens = 0;
        EstimatedUsd = 0;
        PriceImpact = 0;
        return;
      }

      if (IsBuyMode)
      {
        // Buying: amount is USD, estimate tokens received
        EstimatedTokens = amount / CurrentCoin.CurrentPrice;
        EstimatedUsd = amount;
        PriceImpact = Math.Min(amount / 10000m, 15m); // Simple price impact calculation
      }
      else
      {
        // Selling: amount is tokens, estimate USD received
        EstimatedUsd = amount * CurrentCoin.CurrentPrice;
        EstimatedTokens = amount;
        PriceImpact = Math.Min(EstimatedUsd / 10000m, 15m);
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
        try
        {
          _disposed = true;
          _db?.Dispose();
          RecentTrades?.Clear();
          LoggingService.Info("CoinDetailViewModel disposed successfully");
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error disposing CoinDetailViewModel", ex);
        }
      }
    }

    ~CoinDetailViewModel()
    {
      Dispose(false);
    }
  }
}