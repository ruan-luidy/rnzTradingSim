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
    private readonly CoinGeckoService _coinGeckoService;
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
    private int totalPages = 602;

    [ObservableProperty]
    private string resultsText = "Showing 1-12 of 7219 coins";

    [ObservableProperty]
    private bool canGoPreviousPage = false;

    [ObservableProperty]
    private bool canGoNextPage = true;

    public MarketViewModel()
    {
      _coinGeckoService = new CoinGeckoService();
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
    private void GoToPage(int page)
    {
      if (page >= 1 && page <= TotalPages)
      {
        CurrentPage = page;
        UpdatePaginationState();
        _ = LoadCoinsAsync();
      }
    }

    private async Task LoadCoinsAsync()
    {
      IsLoading = true;
      try
      {
        var coins = await _coinGeckoService.GetCoinsAsync(CurrentPage, 12);

        _allCoins.Clear();
        _allCoins.AddRange(coins);

        FilterCoins();
        UpdateResultsText();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading coins: {ex.Message}");
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
      int endIndex = Math.Min(CurrentPage * 12, 7219);
      ResultsText = $"Showing {startIndex}-{endIndex} of 7219 coins";
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
  }
}