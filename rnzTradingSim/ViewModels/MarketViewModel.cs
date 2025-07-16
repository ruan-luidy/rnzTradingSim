using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Media;
using rnzTradingSim.Models;
using rnzTradingSim.Services;

namespace rnzTradingSim.ViewModels
{
  public partial class MarketViewModel : ObservableObject
  {
    private readonly CoinCapService _coinCapService;
    private List<CoinData> _allCoins;

    [ObservableProperty]
    private ObservableCollection<CoinData> filteredCoins;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 100; // CoinCap tem muitas moedas

    [ObservableProperty]
    private string resultsText = "Showing 1-12 of 1200+ coins";

    [ObservableProperty]
    private bool canGoPreviousPage = false;

    [ObservableProperty]
    private bool canGoNextPage = true;

    [ObservableProperty]
    private string apiStatusText = "Using CoinCap API - 100% Free, No Limits!";

    public MarketViewModel()
    {
      _coinCapService = new CoinCapService();
      _allCoins = new List<CoinData>();
      FilteredCoins = new ObservableCollection<CoinData>();

      _ = LoadCoinsAsync();
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

    private async Task LoadCoinsAsync()
    {
      IsLoading = true;
      ApiStatusText = "Loading from CoinCap API...";

      try
      {
        System.Diagnostics.Debug.WriteLine($"Loading page {CurrentPage} from CoinCap API");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var coins = await _coinCapService.GetCoinsAsync(CurrentPage, 12);

        stopwatch.Stop();
        System.Diagnostics.Debug.WriteLine($"API call completed in {stopwatch.ElapsedMilliseconds}ms, got {coins.Count} coins");

        _allCoins.Clear();
        _allCoins.AddRange(coins);

        FilterCoins();
        UpdateResultsText();

        // Status baseado no resultado da API
        if (coins.Count > 0)
        {
          ApiStatusText = $"✓ Live data from CoinCap API ({coins.Count} coins loaded) - 100% Free!";
        }
        else
        {
          ApiStatusText = "⚠ No data loaded - check internet connection";
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading coins: {ex.Message}");
        ApiStatusText = "⚠ Error loading data - check internet connection";

        // Limpar dados em caso de erro
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

      if (!string.IsNullOrWhiteSpace(SearchText))
      {
        filtered = filtered.Where(c =>
          c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
          c.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
      }

      FilteredCoins.Clear();
      foreach (var coin in filtered)
      {
        FilteredCoins.Add(coin);
      }
    }

    private void UpdateResultsText()
    {
      int startIndex = (CurrentPage - 1) * 12 + 1;
      int endIndex = Math.Min(CurrentPage * 12, TotalPages * 12);
      ResultsText = $"Showing {startIndex}-{endIndex} of 1200+ coins";
    }

    private void UpdatePaginationState()
    {
      CanGoPreviousPage = CurrentPage > 1;
      CanGoNextPage = CurrentPage < TotalPages;
    }

    partial void OnSearchTextChanged(string value)
    {
      FilterCoins();
    }

    public void Dispose()
    {
      _coinCapService?.Dispose();
    }
  }
}