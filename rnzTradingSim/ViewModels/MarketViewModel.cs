using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Media;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.Data;

namespace rnzTradingSim.ViewModels
{
  public partial class MarketViewModel : ObservableObject
  {
    private readonly FakeCoinService _coinService;
    private readonly TradingDbContext _db;
    private List<CoinData> _allCoins;
    private System.Timers.Timer? _updateTimer;

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
    private string apiStatusText = "Simulação de Criptomoedas - Dados 100% Fakes!";

    public MarketViewModel()
    {
      _db = new TradingDbContext();
      _coinService = new FakeCoinService(_db);
      _allCoins = new List<CoinData>();
      FilteredCoins = new ObservableCollection<CoinData>();

      _ = LoadCoinsAsync();

      // Atualiza preços a cada 10 minutos
      _updateTimer = new System.Timers.Timer(10 * 60 * 1000);
      _updateTimer.Elapsed += (s, e) => {
        _coinService.UpdateAllCoins();
        _ = LoadCoinsAsync();
      };
      _updateTimer.Start();
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
      ApiStatusText = "Carregando dados simulados...";

      try
      {
        await Task.Delay(200); // Simula delay

        var coins = _coinService.GetCoins(CurrentPage, 12);

        _allCoins.Clear();
        _allCoins.AddRange(coins);

        FilterCoins();
        UpdateResultsText();

        if (coins.Count > 0)
        {
          ApiStatusText = $"✓ Dados simulados ({coins.Count} moedas carregadas)";
        }
        else
        {
          ApiStatusText = "⚠ Nenhuma moeda cadastrada";
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Erro ao carregar moedas: {ex.Message}");
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
      _updateTimer?.Dispose();
      _db?.Dispose();
    }
  }
}