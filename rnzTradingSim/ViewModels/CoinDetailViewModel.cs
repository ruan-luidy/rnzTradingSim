using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels
{
  public partial class CoinDetailViewModel : ObservableObject
  {
    private readonly TradingService _tradingService;
    private readonly PlayerService _playerService;
    private readonly CoinCreationService _coinCreationService;

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

    public CoinDetailViewModel()
    {
      var context = new rnzTradingSim.Data.TradingDbContext();
      _playerService = new PlayerService();
      _tradingService = new TradingService(context, _playerService);
      _coinCreationService = new CoinCreationService(context, _playerService);

      LoadPlayerData();
    }

    public async Task LoadCoinAsync(string coinId)
    {
      CoinId = coinId;
      IsLoading = true;

      try
      {
        // Load coin data
        CurrentCoin = await _coinCreationService.GetCoinAsync(coinId);
        
        if (CurrentCoin == null)
        {
          LastTradeResult = "Coin not found";
          return;
        }

        // Load user portfolio for this coin
        var player = _playerService.GetCurrentPlayer();
        UserPortfolio = await _tradingService.GetPortfolioAsync(player.Id, coinId);

        // Load recent trades
        var trades = await _tradingService.GetRecentTradesAsync(coinId, 20);
        RecentTrades.Clear();
        foreach (var trade in trades)
        {
          RecentTrades.Add(trade);
        }

        UpdateTradeEstimates();
      }
      catch (Exception ex)
      {
        LastTradeResult = $"Error loading coin: {ex.Message}";
        LoggingService.Error("Error loading coin details", ex);
      }
      finally
      {
        IsLoading = false;
      }
    }

    partial void OnTradeAmountChanged(string value)
    {
      UpdateTradeEstimates();
    }

    partial void OnTradingModeChanged(string value)
    {
      UpdateTradeEstimates();
    }

    private void UpdateTradeEstimates()
    {
      if (CurrentCoin == null || !decimal.TryParse(TradeAmount, out var amount) || amount <= 0)
      {
        EstimatedTokens = 0;
        EstimatedUsd = 0;
        PriceImpact = 0;
        return;
      }

      try
      {
        if (TradingMode == "Buy")
        {
          // Calculating tokens out for USD in
          var (tokensOut, newPrice) = CurrentCoin.SimulateBuy(amount);
          EstimatedTokens = tokensOut;
          EstimatedUsd = amount;
          PriceImpact = CurrentCoin.CalculatePriceImpact(amount, true);
        }
        else
        {
          // Calculating USD out for tokens in
          var (usdOut, newPrice) = CurrentCoin.SimulateSell(amount);
          EstimatedTokens = amount;
          EstimatedUsd = usdOut;
          PriceImpact = CurrentCoin.CalculatePriceImpact(amount, false);
        }
      }
      catch
      {
        EstimatedTokens = 0;
        EstimatedUsd = 0;
        PriceImpact = 0;
      }
    }

    [RelayCommand]
    private async Task ExecuteTradeAsync()
    {
      if (CurrentCoin == null || !decimal.TryParse(TradeAmount, out var amount) || amount <= 0)
      {
        LastTradeResult = "Invalid trade amount";
        return;
      }

      IsLoading = true;
      LastTradeResult = "";

      try
      {
        if (TradingMode == "Buy")
        {
          var result = await _tradingService.BuyTokenAsync(CoinId, amount);
          LastTradeResult = result.message;
          
          if (result.success)
          {
            TradeAmount = "0";
            await RefreshDataAsync();
          }
        }
        else
        {
          var result = await _tradingService.SellTokenAsync(CoinId, amount);
          LastTradeResult = result.message;
          
          if (result.success)
          {
            TradeAmount = "0";
            await RefreshDataAsync();
          }
        }
      }
      catch (Exception ex)
      {
        LastTradeResult = $"Trade failed: {ex.Message}";
        LoggingService.Error("Trade execution failed", ex);
      }
      finally
      {
        IsLoading = false;
      }
    }

    [RelayCommand]
    private async Task RugPullAsync()
    {
      if (CurrentCoin == null)
        return;

      var player = _playerService.GetCurrentPlayer();
      if (CurrentCoin.CreatorId != player.Id.ToString())
      {
        LastTradeResult = "You can only rug pull your own coins";
        return;
      }

      var confirmation = HandyControl.Controls.MessageBox.Show(
        $"Are you sure you want to RUG PULL {CurrentCoin.Symbol}?\n\nThis will steal ${CurrentCoin.PoolBaseAmount:N2} and destroy the coin!",
        "Confirm Rug Pull",
        System.Windows.MessageBoxButton.YesNo,
        System.Windows.MessageBoxImage.Warning);

      if (confirmation != System.Windows.MessageBoxResult.Yes)
        return;

      IsLoading = true;

      try
      {
        var result = await _tradingService.RugPullAsync(CoinId);
        LastTradeResult = result.message;
        
        if (result.success)
        {
          await RefreshDataAsync();
        }
      }
      catch (Exception ex)
      {
        LastTradeResult = $"Rug pull failed: {ex.Message}";
        LoggingService.Error("Rug pull failed", ex);
      }
      finally
      {
        IsLoading = false;
      }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
      if (!string.IsNullOrEmpty(CoinId))
      {
        await LoadCoinAsync(CoinId);
      }
      LoadPlayerData();
    }

    [RelayCommand]
    private void SetMaxBuy()
    {
      TradeAmount = PlayerBalance.ToString("0.00");
    }

    [RelayCommand]
    private void SetMaxSell()
    {
      if (UserPortfolio?.TokenBalance > 0)
      {
        TradeAmount = UserPortfolio.TokenBalance.ToString("0.00000000");
      }
    }

    [RelayCommand]
    private void SwitchTradingMode()
    {
      TradingMode = TradingMode == "Buy" ? "Sell" : "Buy";
      TradeAmount = "0";
    }

    private void LoadPlayerData()
    {
      try
      {
        var player = _playerService.GetCurrentPlayer();
        PlayerBalance = player.Balance;
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error loading player data", ex);
      }
    }

    // Computed properties for UI
    public string FormattedPrice => CurrentCoin?.CurrentPrice.ToString("C8") ?? "$0";
    public string FormattedMarketCap => CurrentCoin?.MarketCap.ToString("C0") ?? "$0";
    public string FormattedVolume => CurrentCoin?.Volume24h.ToString("C0") ?? "$0";
    public string FormattedPriceChange => CurrentCoin?.PriceChange24h.ToString("P2") ?? "0%";
    public bool IsPriceUp => CurrentCoin?.PriceChange24h >= 0;
    public string FormattedBalance => PlayerBalance.ToString("C2");
    public string FormattedHoldings => UserPortfolio?.TokenBalance.ToString("N8") ?? "0";
    public string FormattedHoldingsValue => UserPortfolio != null && CurrentCoin != null 
      ? (UserPortfolio.TokenBalance * CurrentCoin.CurrentPrice).ToString("C2") 
      : "$0";
    public string FormattedPnL => UserPortfolio?.UnrealizedPnL.ToString("C2") ?? "$0";
    public bool HasHoldings => UserPortfolio?.TokenBalance > 0;
    public bool CanRugPull => CurrentCoin != null && 
                             CurrentCoin.CreatorId == _playerService.GetCurrentPlayer().Id.ToString() &&
                             !CurrentCoin.IsRugged;
    public bool IsRugged => CurrentCoin?.IsRugged ?? false;
    public string EstimatedTokensFormatted => EstimatedTokens.ToString("N8");
    public string EstimatedUsdFormatted => EstimatedUsd.ToString("C2");
    public string PriceImpactFormatted => PriceImpact.ToString("P2");
    public bool CanTrade => CurrentCoin != null && !CurrentCoin.IsRugged;
  }
}