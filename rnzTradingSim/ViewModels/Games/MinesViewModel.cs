using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Models;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels.Games;

public partial class MinesViewModel : ObservableObject
{
  private readonly Player _player;
  private readonly GamblingViewModel _parent;
  private Random _random = new();

  [ObservableProperty]
  private decimal betAmount = 10m;

  [ObservableProperty]
  private int mineCount = 3;

  [ObservableProperty]
  private bool isGameActive = false;

  [ObservableProperty]
  private bool canCashOut = false;

  [ObservableProperty]
  private decimal currentMultiplier = 1.0m;

  [ObservableProperty]
  private decimal potentialWin = 0m;

  [ObservableProperty]
  private int tilesRevealed = 0;

  [ObservableProperty]
  private int safetilesRemaining = 25;

  [ObservableProperty]
  private string gameStatus = "Place your bet and start playing!";

  public ObservableCollection<MinesTile> GameBoard { get; } = new();

  public MinesViewModel(Player player, GamblingViewModel parent)
  {
    _player = player;
    _parent = parent;
    InitializeBoard();
    UpdateSafeTilesRemaining();
  }

  [RelayCommand]
  private void SetBetPercentage(string percentageStr)
  {
    if (IsGameActive) return;

    if (double.TryParse(percentageStr, out double percentage))
    {
      BetAmount = Math.Round(_player.Balance * (decimal)percentage, 2);
      if (BetAmount < 1) BetAmount = 1;
    }
  }

  [RelayCommand]
  private void StartGame()
  {
    if (IsGameActive || !_parent.CanPlaceBet(BetAmount)) return;

    _parent.DeductBet(BetAmount);

    IsGameActive = true;
    CanCashOut = false;
    TilesRevealed = 0;
    CurrentMultiplier = 1.0m;
    UpdatePotentialWin();
    GameStatus = "Click tiles to reveal them. Avoid the mines!";

    InitializeBoard();
    PlaceMines();
  }

  [RelayCommand]
  private void CashOut()
  {
    if (!CanCashOut || !IsGameActive) return;

    var winAmount = BetAmount * CurrentMultiplier;
    _parent.AddWinnings(winAmount);

    var result = new GameResult
    {
      GameType = "Mines",
      BetAmount = BetAmount,
      WinAmount = winAmount,
      IsWin = true,
      Multiplier = CurrentMultiplier,
      Details = $"Cashed out after {TilesRevealed} tiles with {MineCount} mines"
    };

    _parent.OnGameCompleted(result);

    GameStatus = $"Cashed out! Won ${winAmount:F2} with {CurrentMultiplier:F2}x multiplier!";
    EndGame(false);
  }

  [RelayCommand]
  private void RevealTile(MinesTile tile)
  {
    if (!IsGameActive || tile.IsRevealed) return;

    tile.IsRevealed = true;

    if (tile.IsMine)
    {
      // Hit a mine - game over
      tile.IsExploded = true;
      RevealAllMines();

      var result = new GameResult
      {
        GameType = "Mines",
        BetAmount = BetAmount,
        WinAmount = 0,
        IsWin = false,
        Multiplier = 0,
        Details = $"Hit mine after {TilesRevealed} tiles with {MineCount} mines"
      };

      _parent.OnGameCompleted(result);

      GameStatus = $"💥 BOOM! You hit a mine and lost ${BetAmount:F2}";
      EndGame(true);
    }
    else
    {
      // Safe tile
      TilesRevealed++;
      CalculateMultiplier();
      UpdatePotentialWin();
      CanCashOut = true;

      if (TilesRevealed == SafetilesRemaining)
      {
        // All safe tiles revealed - auto cash out
        GameStatus = "Perfect! You found all safe tiles!";
        CashOut();
      }
      else
      {
        GameStatus = $"Safe! {TilesRevealed} tiles revealed. Multiplier: {CurrentMultiplier:F2}x";
      }
    }
  }

  [RelayCommand]
  private void ResetGame()
  {
    if (IsGameActive) return;

    IsGameActive = false;
    CanCashOut = false;
    TilesRevealed = 0;
    CurrentMultiplier = 1.0m;
    PotentialWin = 0m;
    GameStatus = "Place your bet and start playing!";
    InitializeBoard();
  }

  partial void OnMineCountChanged(int value)
  {
    if (!IsGameActive)
    {
      UpdateSafeTilesRemaining();
      InitializeBoard();
    }
  }

  private void InitializeBoard()
  {
    GameBoard.Clear();
    for (int i = 0; i < 25; i++)
    {
      GameBoard.Add(new MinesTile
      {
        Index = i,
        Row = i / 5,
        Column = i % 5
      });
    }
  }

  private void PlaceMines()
  {
    var minePositions = new HashSet<int>();

    while (minePositions.Count < MineCount)
    {
      var position = _random.Next(0, 25);
      minePositions.Add(position);
    }

    foreach (var position in minePositions)
    {
      GameBoard[position].IsMine = true;
    }
  }

  private void RevealAllMines()
  {
    foreach (var tile in GameBoard.Where(t => t.IsMine))
    {
      tile.IsRevealed = true;
    }
  }

  private void CalculateMultiplier()
  {
    // Mines multiplier formula: (25 - mines) / (25 - mines - tiles_revealed)
    var safeTiles = 25 - MineCount;
    var remainingSafeTiles = safeTiles - TilesRevealed;

    if (remainingSafeTiles > 0)
    {
      CurrentMultiplier = (decimal)safeTiles / remainingSafeTiles;
    }
  }

  private void UpdatePotentialWin()
  {
    PotentialWin = BetAmount * CurrentMultiplier;
  }

  private void UpdateSafeTilesRemaining()
  {
    SafetilesRemaining = 25 - MineCount;
  }

  private void EndGame(bool showMines)
  {
    IsGameActive = false;
    CanCashOut = false;

    if (showMines)
    {
      RevealAllMines();
    }
  }
}

public partial class MinesTile : ObservableObject
{
  [ObservableProperty]
  private int index;

  [ObservableProperty]
  private int row;

  [ObservableProperty]
  private int column;

  [ObservableProperty]
  private bool isRevealed = false;

  [ObservableProperty]
  private bool isMine = false;

  [ObservableProperty]
  private bool isExploded = false;

  public string DisplayText => IsRevealed ? (IsMine ? "💣" : "💎") : "";
}