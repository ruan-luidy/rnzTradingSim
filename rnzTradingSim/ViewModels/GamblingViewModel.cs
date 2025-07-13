using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels
{
  public partial class GamblingViewModel : ObservableObject
  {
    #region Properties

    [ObservableProperty]
    private string selectedGame = "Coinflip";

    [ObservableProperty]
    private decimal balance = 10000;

    [ObservableProperty]
    private decimal totalWinnings = 0;

    [ObservableProperty]
    private decimal totalLosses = 0;

    [ObservableProperty]
    private int gamesPlayed = 0;

    [ObservableProperty]
    private ObservableCollection<GameActivity> recentActivities = new();

    [ObservableProperty]
    private CoinflipViewModel coinflipViewModel;

    [ObservableProperty]
    private SlotsViewModel slotsViewModel;

    public ObservableCollection<GameTab> GameTabs { get; }

    #endregion

    #region Constructor

    public GamblingViewModel()
    {
      GameTabs = new ObservableCollection<GameTab>
            {
                new() { Name = "Coinflip", IsSelected = true, Icon = "🪙" },
                new() { Name = "Slots", IsSelected = false, Icon = "🎰" },
                new() { Name = "Mines", IsSelected = false, Icon = "💣" },
                new() { Name = "Dice", IsSelected = false, Icon = "🎲" }
            };

      // Initialize game ViewModels
      CoinflipViewModel = new CoinflipViewModel(this);
      SlotsViewModel = new SlotsViewModel(this);

      InitializeSampleData();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void SelectGame(string gameName)
    {
      SelectedGame = gameName;

      // Update tab selection
      foreach (var tab in GameTabs)
      {
        tab.IsSelected = tab.Name == gameName;
      }
    }

    #endregion

    #region Methods

    public void UpdateBalance(decimal newBalance)
    {
      Balance = newBalance;

      // Update child ViewModels
      CoinflipViewModel.Balance = newBalance;
      SlotsViewModel.Balance = newBalance;
    }

    public void AddActivity(string game, bool isWin, decimal amount)
    {
      RecentActivities.Insert(0, new GameActivity
      {
        Game = game,
        IsWin = isWin,
        Amount = amount,
        TimeAgo = "Just now"
      });

      // Keep only last 10 activities
      while (RecentActivities.Count > 10)
        RecentActivities.RemoveAt(RecentActivities.Count - 1);

      // Update stats
      if (isWin)
        TotalWinnings += Math.Abs(amount);
      else
        TotalLosses += Math.Abs(amount);

      GamesPlayed++;
    }

    private void InitializeSampleData()
    {
      // Add some sample activities
      RecentActivities.Add(new GameActivity { Game = "Coinflip", IsWin = true, Amount = 95, TimeAgo = "2 min ago" });
      RecentActivities.Add(new GameActivity { Game = "Slots", IsWin = false, Amount = -50, TimeAgo = "5 min ago" });
      RecentActivities.Add(new GameActivity { Game = "Coinflip", IsWin = true, Amount = 190, TimeAgo = "8 min ago" });
    }

    #endregion
  }

  #region Helper Classes

  public partial class GameTab : ObservableObject
  {
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    [ObservableProperty]
    private bool isSelected = false;
  }

  public class GameActivity
  {
    public string Game { get; set; } = string.Empty;
    public bool IsWin { get; set; }
    public decimal Amount { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
  }

  #endregion
}