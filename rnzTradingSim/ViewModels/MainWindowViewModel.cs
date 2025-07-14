using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Models;
using rnzTradingSim.Services;

namespace rnzTradingSim.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
  private readonly PlayerService _playerService;

  [ObservableProperty]
  private Player currentPlayer;

  [ObservableProperty]
  private string currentView = "Home";

  [ObservableProperty]
  private HomeViewModel homeViewModel;

  [ObservableProperty]
  private GamblingViewModel gamblingViewModel;

  [ObservableProperty]
  private LeaderboardViewModel leaderboardViewModel;

  [ObservableProperty]
  private ProfileViewModel profileViewModel;

  public MainWindowViewModel()
  {
    _playerService = new PlayerService();
    CurrentPlayer = _playerService.GetCurrentPlayer();

    // Initialize ViewModels
    HomeViewModel = new HomeViewModel(CurrentPlayer);
    GamblingViewModel = new GamblingViewModel(CurrentPlayer, _playerService);
    LeaderboardViewModel = new LeaderboardViewModel();
    ProfileViewModel = new ProfileViewModel(CurrentPlayer);

    // Subscribe to player updates
    GamblingViewModel.PlayerUpdated += OnPlayerUpdated;
  }

  [RelayCommand]
  private void Navigate(string viewName)
  {
    CurrentView = viewName;

    // Update ViewModels when navigating
    switch (viewName)
    {
      case "Home":
        HomeViewModel.RefreshData();
        break;
      case "Leaderboard":
        LeaderboardViewModel.RefreshData();
        break;
      case "Profile":
        ProfileViewModel.RefreshData();
        break;
    }
  }

  private void OnPlayerUpdated(Player updatedPlayer)
  {
    CurrentPlayer = updatedPlayer;
    _playerService.SavePlayer(updatedPlayer);

    // Notify other ViewModels
    HomeViewModel.UpdatePlayer(updatedPlayer);
    ProfileViewModel.UpdatePlayer(updatedPlayer);
  }
}