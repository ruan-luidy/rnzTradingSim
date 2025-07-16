using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using rnzTradingSim.Services;
using rnzTradingSim.Models;

namespace rnzTradingSim.ViewModels
{
  public partial class MainWindowViewModel : ObservableObject
  {
    private readonly PlayerService _playerService;

    [ObservableProperty]
    private decimal playerBalance = 1000.00m;

    [ObservableProperty]
    private int gamesPlayedToday = 23;

    [ObservableProperty]
    private decimal dailyProfitLoss = 245.50m;

    [ObservableProperty]
    private string selectedView = "MarketView";

    [ObservableProperty]
    private bool isProfileVisible = false;

    public MainWindowViewModel()
    {
      _playerService = new PlayerService();
      LoadPlayerData();
    }

    private void LoadPlayerData()
    {
      try
      {
        var player = _playerService.GetCurrentPlayer();
        PlayerBalance = player.Balance;
        GamesPlayedToday = player.GamesPlayed;
        DailyProfitLoss = player.NetProfit;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading player data: {ex.Message}");
      }
    }

    [RelayCommand]
    private void NavigateToHome()
    {
      SelectedView = "HomeView";
    }

    [RelayCommand]
    private void NavigateToMarket()
    {
      SelectedView = "MarketView";
    }

    [RelayCommand]
    private void NavigateToHopium()
    {
      SelectedView = "HopiumView";
    }

    [RelayCommand]
    private void NavigateToGambling()
    {
      SelectedView = "GamblingView";
    }

    [RelayCommand]
    private void NavigateToLeaderboard()
    {
      SelectedView = "LeaderboardView";
    }

    [RelayCommand]
    private void NavigateToPortfolio()
    {
      SelectedView = "PortfolioView";
    }

    [RelayCommand]
    private void NavigateToTreemap()
    {
      SelectedView = "TreemapView";
    }

    [RelayCommand]
    private void NavigateToCreateCoin()
    {
      SelectedView = "CreateCoinView";
    }

    [RelayCommand]
    private void NavigateToNotifications()
    {
      SelectedView = "NotificationsView";
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
      SelectedView = "AboutView";
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
      SelectedView = "ProfileView";
    }

    [RelayCommand]
    private void ShowProfile()
    {
      IsProfileVisible = !IsProfileVisible;
    }

    public void UpdateBalance(decimal newBalance)
    {
      PlayerBalance = newBalance;
    }

    public void UpdateDailyStats(int games, decimal profitLoss)
    {
      GamesPlayedToday = games;
      DailyProfitLoss = profitLoss;
    }

    public void RefreshPlayerData()
    {
      LoadPlayerData();
    }
  }
}