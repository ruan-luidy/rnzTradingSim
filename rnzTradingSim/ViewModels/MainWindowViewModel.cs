using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels
{
  public partial class MainWindowViewModel : ObservableObject
  {
    [ObservableProperty]
    private decimal playerBalance = 1000.00m;

    [ObservableProperty]
    private int gamesPlayedToday = 23;

    [ObservableProperty]
    private decimal dailyProfitLoss = 245.50m;

    [ObservableProperty]
    private string selectedView = "GamblingView";

    [ObservableProperty]
    private bool isProfileVisible = false;

    public MainWindowViewModel()
    {
      // Initialize any required services or data
    }

    [RelayCommand]
    private void NavigateToHome()
    {
      SelectedView = "HomeView";
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
  }
}