using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.Data;
using static UIConstants;

namespace rnzTradingSim.ViewModels
{
  public partial class HomeViewModel : ObservableObject, IDisposable
  {
    private readonly FakeCoinService _coinService;
    private readonly TradingDbContext _db;
    private List<CoinData> _allCoins;
    private bool _disposed = false;

    [ObservableProperty]
    private ObservableCollection<CoinData> topCoins;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string greetingText = "Welcome to Rugplay!";

    [ObservableProperty]
    private string subtitleText = "Here's the market overview for today.";

    public HomeViewModel()
    {
      try
      {
        _db = new TradingDbContext();
        _coinService = new FakeCoinService(_db);
        _allCoins = new List<CoinData>();
        TopCoins = new ObservableCollection<CoinData>();

        _ = LoadCoinsAsync();

        LoggingService.Info("HomeViewModel initialized successfully");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing HomeViewModel", ex);
        
        try
        {
          _db = new TradingDbContext();
          _db.InitializeDatabase();
          _coinService = new FakeCoinService(_db);
          _allCoins = new List<CoinData>();
          TopCoins = new ObservableCollection<CoinData>();
          
          _ = LoadCoinsAsync();
        }
        catch (Exception fallbackEx)
        {
          LoggingService.Error("Fallback initialization failed for HomeViewModel", fallbackEx);
          TopCoins = new ObservableCollection<CoinData>();
        }
      }
    }

    private async Task LoadCoinsAsync()
    {
      if (_disposed) return;

      IsLoading = true;

      try
      {
        await Task.Delay(200);

        if (_coinService == null)
        {
          throw new InvalidOperationException("Coin service not initialized");
        }

        // Get more coins for better top selection
        var coins = _coinService.GetCoins(1, 50);

        _allCoins.Clear();
        _allCoins.AddRange(coins);

        UpdateTopCoins();

        if (coins.Count > 0)
        {
          LoggingService.Info($"HomeViewModel loaded {coins.Count} coins successfully");
        }
        else
        {
          LoggingService.Warning("HomeViewModel loaded 0 coins");
        }
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error loading coins in HomeViewModel", ex);
        TopCoins.Clear();
      }
      finally
      {
        IsLoading = false;
      }
    }

    private void UpdateTopCoins()
    {
      TopCoins.Clear();
      var topCoins = _allCoins.OrderByDescending(c => c.MarketCapValue).Take(6);
      foreach (var coin in topCoins)
      {
        TopCoins.Add(coin);
      }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
      await LoadCoinsAsync();
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
          _allCoins?.Clear();
          TopCoins?.Clear();
          LoggingService.Info("HomeViewModel disposed successfully");
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error disposing HomeViewModel", ex);
        }
      }
    }

    ~HomeViewModel()
    {
      Dispose(false);
    }
  }
}
