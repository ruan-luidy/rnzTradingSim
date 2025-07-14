using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using rnzTradingSim.ViewModels.Games;

namespace rnzTradingSim.ViewModels;

public partial class GamblingViewModel : ObservableObject
{
  private readonly Player _player;
  private readonly PlayerService _playerService;

  public event Action<Player>? PlayerUpdated;

  [ObservableProperty]
  private string selectedGame = "Mines";

  [ObservableProperty]
  private MinesViewModel minesViewModel;

  public List<GameTab> GameTabs { get; }

  public GamblingViewModel(Player player, PlayerService playerService)
  {
    _player = player;
    _playerService = playerService;

    // Initialize game tabs
    GameTabs = new List<GameTab>
        {
            new() { Name = "Mines", Icon = "💣", IsSelected = true },
            new() { Name = "Coinflip", Icon = "🪙", IsSelected = false },
            new() { Name = "Slots", Icon = "🎰", IsSelected = false },
            new() { Name = "Dice", Icon = "🎲", IsSelected = false }
        };

    // Initialize game ViewModels
    MinesViewModel = new MinesViewModel(_player, this);
  }

  [RelayCommand]
  private void SelectGame(string gameName)
  {
    if (SelectedGame == gameName) return;

    // Reset current game session
    ResetCurrentGameSession();

    SelectedGame = gameName;

    // Update tab selection
    foreach (var tab in GameTabs)
    {
      tab.IsSelected = tab.Name == gameName;
    }
  }

  public void OnGameCompleted(GameResult result)
  {
    _playerService.UpdatePlayerStats(_player, result);
    PlayerUpdated?.Invoke(_player);
  }

  public bool CanPlaceBet(decimal amount)
  {
    return amount > 0 && amount <= _player.Balance;
  }

  public void DeductBet(decimal amount)
  {
    if (CanPlaceBet(amount))
    {
      _player.Balance -= amount;
      PlayerUpdated?.Invoke(_player);
    }
  }

  public void AddWinnings(decimal amount)
  {
    _player.Balance += amount;
    PlayerUpdated?.Invoke(_player);
  }

  private void ResetCurrentGameSession()
  {
    switch (SelectedGame)
    {
      case "Mines":
        MinesViewModel.ResetGame();
        break;
        // Add other games later
    }
  }
}

public partial class GameTab : ObservableObject
{
  [ObservableProperty]
  private string name = string.Empty;

  [ObservableProperty]
  private string icon = string.Empty;

  [ObservableProperty]
  private bool isSelected = false;
}