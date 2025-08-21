using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Media;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.Data;
using static UIConstants;

namespace rnzTradingSim.ViewModels
{
  public partial class MarketViewModel : ObservableObject, IDisposable
  {
    private readonly FakeCoinService _coinService;
    private readonly TradingDbContext _db;
    private List<CoinData> _allCoins;
    private System.Timers.Timer? _updateTimer;
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private bool _disposed = false;

    [ObservableProperty]
    private ObservableCollection<CoinData> filteredCoins;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 100;

    [ObservableProperty]
    private string resultsText = $"Showing 1-{DEFAULT_PAGE_SIZE} of 1200+ coins";

    [ObservableProperty]
    private bool canGoPreviousPage = false;

    [ObservableProperty]
    private bool canGoNextPage = true;

    [ObservableProperty]
    private string apiStatusText = "Simulação de Criptomoedas - Dados 100% Fakes!";

    [ObservableProperty]
    private ObservableCollection<CoinData> topCoins;

    [ObservableProperty]
    private string greetingText = "Welcome to Rugplay!";

    [ObservableProperty]
    private string subtitleText = "Here's the market overview for today.";

    [ObservableProperty]
    private int sortBy = 0; // 0=Market Cap, 1=Price, 2=24h Change, 3=Volume, 4=Name

    public MarketViewModel()
    {
      try
      {
        _db = new TradingDbContext();
        _coinService = new FakeCoinService(_db);
        _allCoins = new List<CoinData>();
        FilteredCoins = new ObservableCollection<CoinData>();
        TopCoins = new ObservableCollection<CoinData>();

        _ = LoadCoinsAsync();

        InitializeTimer();

        LoggingService.Info("MarketViewModel initialized successfully");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing MarketViewModel", ex);

        // Tentar inicialização de fallback
        try
        {
          _db = new TradingDbContext();
          _db.InitializeDatabase();
          _coinService = new FakeCoinService(_db);
          _allCoins = new List<CoinData>();
          FilteredCoins = new ObservableCollection<CoinData>();

          ApiStatusText = "⚠ Reinicialização automática realizada";
          _ = LoadCoinsAsync();
        }
        catch (Exception fallbackEx)
        {
          LoggingService.Error("Fallback initialization failed", fallbackEx);
          ApiStatusText = "⚠ Erro ao carregar dados - Reinicie a aplicação";
          FilteredCoins = new ObservableCollection<CoinData>();
        }
      }
    }

    private void InitializeTimer()
    {
      try
      {
        _updateTimer = new System.Timers.Timer(MARKET_UPDATE_INTERVAL_MS);
        _updateTimer.Elapsed += async (s, e) => {
          if (_disposed) return;

          try
          {
            _coinService?.UpdateAllCoins();
            await LoadCoinsAsync();
          }
          catch (Exception ex)
          {
            LoggingService.Error("Error in market timer update", ex);
          }
        };
        _updateTimer.AutoReset = true;
        _updateTimer.Start();
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error initializing market timer", ex);
      }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
      await LoadCoinsAsync();
    }

    [RelayCommand]
    private void ShowFilters()
    {
      // TODO: Implement filters dialog
    }

    [RelayCommand]
    private void PreviousPage()
    {
      if (CurrentPage > 1)
      {
        CurrentPage--;
        UpdatePaginationState();
        _ = LoadCoinsAsync();
      }
    }

    [RelayCommand]
    private void NextPage()
    {
      if (CurrentPage < TotalPages)
      {
        CurrentPage++;
        UpdatePaginationState();
        _ = LoadCoinsAsync();
      }
    }

    [RelayCommand]
    private void GoToPage(string pageString)
    {
      if (int.TryParse(pageString, out int page))
      {
        if (page >= 1 && page <= TotalPages)
        {
          CurrentPage = page;
          UpdatePaginationState();
          _ = LoadCoinsAsync();
        }
      }
    }

    // Método para reagir automaticamente às mudanças de página da paginação do HandyControl
    partial void OnCurrentPageChanged(int oldValue, int newValue)
    {
      UpdatePaginationState();
      _ = LoadCoinsAsync();
    }

    private async Task LoadCoinsAsync()
    {
      if (_disposed) return;

      // Verificar cache antes de carregar
      if (DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes < COIN_CACHE_DURATION_MINUTES)
      {
        return; // Usar dados em cache
      }

      IsLoading = true;
      ApiStatusText = "Carregando dados simulados...";

      try
      {
        await Task.Delay(200); // Simula delay de rede

        if (_coinService == null)
        {
          throw new InvalidOperationException("Coin service not initialized");
        }

        var coins = _coinService.GetCoins(CurrentPage, DEFAULT_PAGE_SIZE);

        _allCoins.Clear();
        _allCoins.AddRange(coins);

        FilterCoins();
        UpdateTopCoins();
        UpdateResultsText();
        _lastCacheUpdate = DateTime.Now;

        if (coins.Count > 0)
        {
          ApiStatusText = $"✓ Dados simulados ({coins.Count} moedas carregadas)";
        }
        else
        {
          ApiStatusText = "⚠ Nenhuma moeda encontrada";

          // Se não encontrou moedas, tentar recriar os dados
          try
          {
            System.Diagnostics.Debug.WriteLine("No coins found, attempting to reinitialize...");
            DatabaseInitializer.Initialize();

            // Tentar carregar novamente
            coins = _coinService.GetCoins(CurrentPage, DEFAULT_PAGE_SIZE);
            _allCoins.Clear();
            _allCoins.AddRange(coins);
            FilterCoins();

            if (coins.Count > 0)
            {
              ApiStatusText = $"✓ Dados recarregados ({coins.Count} moedas)";
            }
          }
          catch (Exception reinitEx)
          {
            System.Diagnostics.Debug.WriteLine($"Failed to reinitialize: {reinitEx.Message}");
            ApiStatusText = "⚠ Erro ao recarregar dados";
          }
        }
      }
      catch (Exception ex)
      {
        LoggingService.Error("Erro ao carregar moedas", ex);
        ApiStatusText = "⚠ Erro ao carregar dados simulados";
        _allCoins.Clear();
        FilterCoins();
        UpdateResultsText();
      }
      finally
      {
        IsLoading = false;
      }
    }

    private void FilterCoins()
    {
      var filtered = _allCoins.AsEnumerable();

      // Apply search filter
      if (!string.IsNullOrWhiteSpace(SearchText))
      {
        filtered = filtered.Where(c =>
          c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
          c.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
      }

      // Apply sorting
      filtered = SortBy switch
      {
        0 => filtered.OrderByDescending(c => c.MarketCapValue), // Market Cap
        1 => filtered.OrderByDescending(c => c.CurrentPrice), // Price
        2 => filtered.OrderByDescending(c => c.PriceChangePercentage24h), // 24h Change
        3 => filtered.OrderByDescending(c => c.Volume24h), // Volume
        4 => filtered.OrderBy(c => c.Name), // Name
        _ => filtered.OrderByDescending(c => c.MarketCapValue)
      };

      // Pagination
      var totalCount = filtered.Count();
      var pagedCoins = filtered.Skip((CurrentPage - 1) * DEFAULT_PAGE_SIZE).Take(DEFAULT_PAGE_SIZE);

      FilteredCoins.Clear();
      foreach (var coin in pagedCoins)
      {
        FilteredCoins.Add(coin);
      }

      // Update pagination info
      TotalPages = (int)Math.Ceiling((double)totalCount / DEFAULT_PAGE_SIZE);
      CanGoPreviousPage = CurrentPage > 1;
      CanGoNextPage = CurrentPage < TotalPages;
    }

    private void UpdateResultsText()
    {
      var totalCoins = _coinService?.GetTotalCoinsCount() ?? 0;
      int startIndex = (CurrentPage - 1) * DEFAULT_PAGE_SIZE + 1;
      int endIndex = Math.Min(CurrentPage * DEFAULT_PAGE_SIZE, totalCoins);
      ResultsText = $"Showing {startIndex}-{endIndex} of {totalCoins} coins";
    }

    private void UpdatePaginationState()
    {
      var totalCoins = _coinService?.GetTotalCoinsCount() ?? 0;
      TotalPages = Math.Max(1, (int)Math.Ceiling(totalCoins / (double)DEFAULT_PAGE_SIZE));

      CanGoPreviousPage = CurrentPage > 1;
      CanGoNextPage = CurrentPage < TotalPages;

      // Garantir que CurrentPage está dentro dos limites
      if (CurrentPage < 1) CurrentPage = 1;
      if (CurrentPage > TotalPages) CurrentPage = TotalPages;
    }

    partial void OnSearchTextChanged(string value)
    {
      FilterCoins();
    }

    partial void OnSortByChanged(int value)
    {
      FilterCoins();
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

          if (_updateTimer != null)
          {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= async (s, e) => { };
            _updateTimer.Dispose();
            _updateTimer = null;
          }

          _db?.Dispose();

          _allCoins?.Clear();
          FilteredCoins?.Clear();

          LoggingService.Info("MarketViewModel disposed successfully");
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error disposing MarketViewModel", ex);
        }
      }
    }

    ~MarketViewModel()
    {
      Dispose(false);
    }
  }
}