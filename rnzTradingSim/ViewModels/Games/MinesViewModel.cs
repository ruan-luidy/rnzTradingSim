using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using System.Globalization;

namespace rnzTradingSim.ViewModels.Games
{
  public partial class MinesViewModel : ObservableObject
  {
    private readonly PlayerService _playerService;
    private Player _currentPlayer;

    [ObservableProperty]
    private decimal betAmount = 10.00m;

    [ObservableProperty]
    private int numberOfMines = 3;

    [ObservableProperty]
    private decimal currentMultiplier = 1.00m;

    [ObservableProperty]
    private decimal potentialWin = 10.00m;

    [ObservableProperty]
    private decimal playerBalance = 0m;

    [ObservableProperty]
    private string gameStatus = "WAITING";

    [ObservableProperty]
    private string gameStatusDescription = "Configure your bet";

    [ObservableProperty]
    private int revealedTiles = 0;

    [ObservableProperty]
    private int totalTiles = 25;

    [ObservableProperty]
    private bool isGameActive = false;

    [ObservableProperty]
    private bool canStartGame = true;

    [ObservableProperty]
    private bool canCollectWinnings = false;

    [ObservableProperty]
    private string probabilityText = "You will get 1.08x per tile, probability of winning: 88.00%";

    [ObservableProperty]
    private string maxBetText = "Max bet: 1.000.000";

    [ObservableProperty]
    private string collectButtonText = "Collect Winnings";

    public ObservableCollection<MineButtonViewModel> MineButtons { get; }
    public ObservableCollection<GameResultViewModel> RecentGames { get; }

    public MinesViewModel()
    {
      _playerService = new PlayerService();
      _currentPlayer = _playerService.GetCurrentPlayer();

      MineButtons = new ObservableCollection<MineButtonViewModel>();
      RecentGames = new ObservableCollection<GameResultViewModel>();

      InitializeMineGrid();
      LoadRecentGames();
      UpdatePlayerData();
      CalculateProbability();
      CalculatePotentialWin();
    }

    [RelayCommand]
    private void StartGame()
    {
      if (!CanStartGame || BetAmount > PlayerBalance || BetAmount <= 0)
      {
        if (BetAmount > PlayerBalance)
        {
          GameStatusDescription = "Insufficient balance!";
        }
        else if (BetAmount <= 0)
        {
          GameStatusDescription = "Invalid bet amount!";
        }
        return;
      }

      IsGameActive = true;
      CanStartGame = false;
      CanCollectWinnings = false;
      GameStatus = "IN GAME";
      GameStatusDescription = "Click the tiles";
      RevealedTiles = 0;
      CurrentMultiplier = 1.00m;

      ResetMineGrid();
      PlaceMines();
      CalculatePotentialWin();

      // Trigger visual reset
      OnPropertyChanged(nameof(IsGameActive));
    }

    [RelayCommand]
    private void CollectWinnings()
    {
      if (!CanCollectWinnings) return;

      var gameResult = new GameResult
      {
        GameType = "Mines",
        BetAmount = BetAmount,
        WinAmount = PotentialWin,
        IsWin = true,
        Multiplier = CurrentMultiplier,
        Details = $"{{\"mines\":{NumberOfMines},\"revealed\":{RevealedTiles}}}"
      };

      _playerService.UpdatePlayerStats(_currentPlayer, gameResult);
      UpdatePlayerData();

      RecentGames.Insert(0, new GameResultViewModel
      {
        MineCount = NumberOfMines,
        Amount = PotentialWin - BetAmount,
        IsWin = true
      });

      EndGame(true);
    }

    [RelayCommand]
    private void RevealTile(MineButtonViewModel button)
    {
      if (!IsGameActive || button.IsRevealed) return;

      button.IsRevealed = true;

      if (button.IsMine)
      {
        // Game over - hit a mine
        RevealAllMines();

        var gameResult = new GameResult
        {
          GameType = "Mines",
          BetAmount = BetAmount,
          WinAmount = 0,
          IsWin = false,
          Multiplier = 0,
          Details = $"{{\"mines\":{NumberOfMines},\"revealed\":{RevealedTiles}}}"
        };

        _playerService.UpdatePlayerStats(_currentPlayer, gameResult);
        UpdatePlayerData();

        RecentGames.Insert(0, new GameResultViewModel
        {
          MineCount = NumberOfMines,
          Amount = -BetAmount,
          IsWin = false
        });

        EndGame(false);
      }
      else
      {
        // Safe tile revealed
        RevealedTiles++;
        CalculateCurrentMultiplier();
        CalculatePotentialWin();
        CanCollectWinnings = true;
        CollectButtonText = $"Collect {PotentialWin:C}";

        // Check if all safe tiles are revealed
        int safeTiles = TotalTiles - NumberOfMines;
        if (RevealedTiles >= safeTiles)
        {
          // Auto-collect maximum win
          CollectWinnings();
        }
      }
    }

    [RelayCommand]
    private void IncreaseMines()
    {
      if (NumberOfMines < 20 && !IsGameActive)
      {
        NumberOfMines++;
        CalculateProbability();
        CalculatePotentialWin();
      }
    }

    [RelayCommand]
    private void DecreaseMines()
    {
      if (NumberOfMines > 1 && !IsGameActive)
      {
        NumberOfMines--;
        CalculateProbability();
        CalculatePotentialWin();
      }
    }

    [RelayCommand]
    private void SetBetPercentage(string percentage)
    {
      if (IsGameActive) return;

      var percent = int.Parse(percentage);
      BetAmount = Math.Round(PlayerBalance * percent / 100, 2);

      if (BetAmount < 0.01m) BetAmount = 0.01m;
      if (BetAmount > 100000) BetAmount = 100000;

      CalculatePotentialWin();
    }

    partial void OnBetAmountChanged(decimal value)
    {
      if (value < 0)
      {
        BetAmount = Math.Abs(value);
        return;
      }

      CalculatePotentialWin();
    }

    [RelayCommand]
    private void UpdateBetAmount(string newValue)
    {
      if (IsGameActive) return;

      if (decimal.TryParse(newValue, out decimal amount))
      {
        // Aplicar limites sem resetar
        if (amount < 0.01m) amount = 0.01m;
        if (amount > Math.Min(PlayerBalance, 100000)) amount = Math.Min(PlayerBalance, 100000);

        BetAmount = amount;
      }
    }

    partial void OnNumberOfMinesChanged(int value)
    {
      CalculateProbability();
      CalculatePotentialWin();
    }

    private void InitializeMineGrid()
    {
      MineButtons.Clear();
      for (int i = 0; i < TotalTiles; i++)
      {
        var button = new MineButtonViewModel
        {
          Index = i,
          Row = i / 5,
          Column = i % 5,
          ClickCommand = new RelayCommand<MineButtonViewModel>(RevealTile)
        };
        MineButtons.Add(button);
      }
    }

    private void ResetMineGrid()
    {
      foreach (var button in MineButtons)
      {
        button.Reset();
      }
    }

    private void PlaceMines()
    {
      var random = new Random();
      var minePositions = new HashSet<int>();

      while (minePositions.Count < NumberOfMines)
      {
        int position = random.Next(TotalTiles);
        minePositions.Add(position);
      }

      foreach (int position in minePositions)
      {
        MineButtons[position].IsMine = true;
      }
    }

    private void RevealAllMines()
    {
      foreach (var button in MineButtons.Where(b => b.IsMine))
      {
        button.IsRevealed = true;
      }
    }

    private void CalculateCurrentMultiplier()
    {
      if (RevealedTiles == 0)
      {
        CurrentMultiplier = 1.00m;
        return;
      }

      int safeTiles = TotalTiles - NumberOfMines;
      double baseMultiplier = (double)TotalTiles / safeTiles;
      CurrentMultiplier = (decimal)Math.Pow(baseMultiplier, RevealedTiles / (double)safeTiles);
    }

    private void CalculatePotentialWin()
    {
      PotentialWin = BetAmount * CurrentMultiplier;
    }

    private void CalculateProbability()
    {
      int safeTiles = TotalTiles - NumberOfMines;
      double probability = (double)safeTiles / TotalTiles * 100;
      double multiplierPerTile = (double)TotalTiles / safeTiles;
      ProbabilityText = $"You will get {multiplierPerTile:F2}x per tile, probability of winning: {probability:F2}%";
    }

    private void UpdatePlayerData()
    {
      _currentPlayer = _playerService.GetCurrentPlayer();
      PlayerBalance = _currentPlayer.Balance;
      MaxBetText = $"Max bet: {Math.Min(PlayerBalance, 100000):C}";
    }

    private void EndGame(bool won)
    {
      IsGameActive = false;
      CanStartGame = true;
      CanCollectWinnings = false;

      GameStatus = won ? "WON" : "LOST";
      GameStatusDescription = won ? "Congratulations!" : "Try again";

      CurrentMultiplier = 1.00m;
      RevealedTiles = 0;
      CollectButtonText = "Collect Winnings";
      CalculatePotentialWin();

      // Trigger visual update for loss (to show all mines)
      if (!won)
      {
        OnPropertyChanged(nameof(GameStatus));
      }
    }

    private void LoadRecentGames()
    {
      var history = _playerService.GetGameHistory()
        .Where(g => g.GameType == "Mines")
        .Take(5)
        .OrderByDescending(g => g.PlayedAt);

      foreach (var game in history)
      {
        RecentGames.Add(new GameResultViewModel
        {
          MineCount = NumberOfMines, // Could parse from Details JSON
          Amount = game.NetResult,
          IsWin = game.IsWin
        });
      }
    }
  }

  public partial class MineButtonViewModel : ObservableObject
  {
    [ObservableProperty]
    private bool isRevealed = false;

    [ObservableProperty]
    private bool isMine = false;

    public int Index { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public RelayCommand<MineButtonViewModel> ClickCommand { get; set; }

    public void Reset()
    {
      IsRevealed = false;
      IsMine = false;
    }
  }

  public class GameResultViewModel
  {
    public int MineCount { get; set; }
    public decimal Amount { get; set; }
    public bool IsWin { get; set; }

    public string DisplayText => $"{MineCount} Minas";
    public string AmountText => IsWin ? $"+{Amount:C}" : $"-{Math.Abs(Amount):C}";
  }
}