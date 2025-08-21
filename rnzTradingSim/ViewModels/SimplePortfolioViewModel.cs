using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.Data;

namespace rnzTradingSim.ViewModels
{
  public partial class SimplePortfolioViewModel : ObservableObject, IDisposable
  {
    private static readonly CultureInfo UsdCulture = new CultureInfo("en-US");
    
    private readonly TradingService _tradingService;
    private readonly PlayerService _playerService;
    private readonly TradingDbContext _db;
    private bool _disposed = false;

    [ObservableProperty]
    private ObservableCollection<PortfolioHolding> holdings;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private decimal totalPortfolioValue = 0m;

    [ObservableProperty]
    private decimal cashBalance = 0m;

    [ObservableProperty]
    private decimal coinHoldingsValue = 0m;

    [ObservableProperty]
    private bool hasHoldings = false;

    [ObservableProperty]
    private bool hasTransactions = false;

    [ObservableProperty]
    private bool canSendMoney = false;

    public SimplePortfolioViewModel()
    {
      try
      {
        _db = new TradingDbContext();
        _playerService = new PlayerService();
        _tradingService = new TradingService(_db, _playerService);
        Holdings = new ObservableCollection<PortfolioHolding>();

        _ = LoadPortfolioDataAsync();

        LoggingService.Info("SimplePortfolioViewModel initialized successfully");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing SimplePortfolioViewModel", ex);
        Holdings = new ObservableCollection<PortfolioHolding>();
      }
    }

    private async Task LoadPortfolioDataAsync()
    {
      if (_disposed) return;

      IsLoading = true;

      try
      {
        await Task.Delay(200);

        // Load player data
        var player = _playerService.GetCurrentPlayer();
        CashBalance = player.Balance;

        // Load portfolio holdings
        var portfolios = await _tradingService.GetPlayerPortfoliosAsync(player.Id);
        
        Holdings.Clear();
        CoinHoldingsValue = 0m;

        foreach (var portfolio in portfolios.Where(p => p.TokenBalance > 0))
        {
          try
          {
            // Get current coin data (simulated since we don't have real-time data)
            var currentPrice = GenerateCurrentPrice(portfolio.Coin?.Symbol ?? portfolio.CoinId);
            var change24h = GenerateChange24h();
            
            var holding = new PortfolioHolding
            {
              CoinId = portfolio.CoinId,
              CoinName = portfolio.Coin?.Name ?? portfolio.CoinId,
              Symbol = portfolio.Coin?.Symbol ?? portfolio.CoinId,
              Quantity = portfolio.TokenBalance,
              CurrentPrice = currentPrice,
              AverageBuyPrice = portfolio.AverageBuyPrice,
              Value = portfolio.TokenBalance * currentPrice,
              PnLAmount = portfolio.UnrealizedPnL,
              PnLPercentage = portfolio.AverageBuyPrice > 0 
                ? ((currentPrice - portfolio.AverageBuyPrice) / portfolio.AverageBuyPrice) * 100 
                : 0,
              Change24h = change24h
            };

            Holdings.Add(holding);
            CoinHoldingsValue += holding.Value;
          }
          catch (Exception ex)
          {
            LoggingService.Warning($"Error processing portfolio for {portfolio.Coin?.Symbol ?? portfolio.CoinId}: {ex.Message}");
          }
        }

        // Calculate portfolio percentages
        TotalPortfolioValue = CashBalance + CoinHoldingsValue;
        foreach (var holding in Holdings)
        {
          holding.PortfolioPercentage = TotalPortfolioValue > 0 
            ? (holding.Value / TotalPortfolioValue) * 100 
            : 0;
        }

        HasHoldings = Holdings.Count > 0;
        CanSendMoney = CashBalance > 0; // Simple check for demo

        LoggingService.Info($"Loaded portfolio with {Holdings.Count} holdings, total value: {TotalPortfolioValue:C2}");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error loading portfolio data", ex);
      }
      finally
      {
        IsLoading = false;
      }
    }

    // Simulate current price based on historical data
    private decimal GenerateCurrentPrice(string symbol)
    {
      var random = new Random(symbol.GetHashCode() + DateTime.Today.DayOfYear);
      var basePrice = symbol.Length * 0.1m + 0.01m; // Simple price based on symbol
      var variation = (decimal)(random.NextDouble() * 0.4 - 0.2); // ±20% variation
      return Math.Max(basePrice * (1 + variation), 0.000001m);
    }

    // Simulate 24h change
    private decimal GenerateChange24h()
    {
      var random = new Random(DateTime.Now.Millisecond);
      return (decimal)(random.NextDouble() * 40 - 20); // ±20% change
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
      await LoadPortfolioDataAsync();
    }

    [RelayCommand]
    private void SendMoney()
    {
      // TODO: Implement send money dialog
      LoggingService.Info("Send money feature not yet implemented");
    }

    [RelayCommand]
    private void BrowseCoins()
    {
      var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
      if (mainWindow?.DataContext is MainWindowViewModel mainVM)
      {
        mainVM.NavigateToMarket();
      }
    }

    [RelayCommand]
    private void ViewAllTransactions()
    {
      // TODO: Navigate to transactions view
      LoggingService.Info("View all transactions feature not yet implemented");
    }

    // Computed properties for UI binding
    public string TotalPortfolioValueFormatted => TotalPortfolioValue.ToString("C2", UsdCulture);
    public string CashBalanceFormatted => CashBalance.ToString("C2", UsdCulture);
    public string CoinHoldingsValueFormatted => CoinHoldingsValue.ToString("C2", UsdCulture);
    
    public string CashPercentageText => TotalPortfolioValue > 0 
      ? $"{((CashBalance / TotalPortfolioValue) * 100):F1}% of portfolio"
      : "100% of portfolio";
    
    public string HoldingsCountText => $"{Holdings.Count} positions";

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
          Holdings?.Clear();
          LoggingService.Info("SimplePortfolioViewModel disposed successfully");
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error disposing SimplePortfolioViewModel", ex);
        }
      }
    }

    ~SimplePortfolioViewModel()
    {
      Dispose(false);
    }
  }
}