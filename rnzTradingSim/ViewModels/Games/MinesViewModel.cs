using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace rnzTradingSim.ViewModels.Games
{
  public partial class MinesViewModel : ObservableObject
  {
    [ObservableProperty]
    private decimal betAmount = 10.00m;

    [ObservableProperty]
    private int numberOfMines = 3;

    [ObservableProperty]
    private decimal currentMultiplier = 1.00m;

    [ObservableProperty]
    private decimal potentialWin = 10.00m;

    [ObservableProperty]
    private string gameStatus = "AGUARDANDO";

    [ObservableProperty]
    private string gameStatusDescription = "Configure sua aposta";

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

    public ObservableCollection<MineButtonViewModel> MineButtons { get; }
    public ObservableCollection<GameResultViewModel> RecentGames { get; }

    public MinesViewModel()
    {
      MineButtons = new ObservableCollection<MineButtonViewModel>();
      RecentGames = new ObservableCollection<GameResultViewModel>();

      InitializeMineGrid();
      LoadRecentGames();
    }

    [RelayCommand]
    private void StartGame()
    {
      if (!CanStartGame) return;

      IsGameActive = true;
      CanStartGame = false;
      CanCollectWinnings = false;
      GameStatus = "EM JOGO";
      GameStatusDescription = "Clique nos quadrados";
      RevealedTiles = 0;

      ResetMineGrid();
      PlaceMines();
      CalculatePotentialWin();
    }

    [RelayCommand]
    private void CollectWinnings()
    {
      if (!CanCollectWinnings) return;

      // Add win to recent games
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

        // Check if all safe tiles are revealed
        int safeTiles = TotalTiles - NumberOfMines;
        if (RevealedTiles >= safeTiles)
        {
          // Auto-collect maximum win
          CollectWinnings();
        }
      }
    }

    partial void OnBetAmountChanged(decimal value)
    {
      CalculatePotentialWin();
    }

    partial void OnNumberOfMinesChanged(int value)
    {
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
      // Basic multiplier calculation (can be made more sophisticated)
      int safeTiles = TotalTiles - NumberOfMines;
      double baseMultiplier = 1.0 + (0.25 * NumberOfMines / safeTiles);
      CurrentMultiplier = (decimal)Math.Pow(baseMultiplier, RevealedTiles);
    }

    private void CalculatePotentialWin()
    {
      PotentialWin = BetAmount * CurrentMultiplier;
    }

    private void EndGame(bool won)
    {
      IsGameActive = false;
      CanStartGame = true;
      CanCollectWinnings = false;

      GameStatus = won ? "GANHOU" : "PERDEU";
      GameStatusDescription = won ? "Parabens!" : "Tente novamente";

      CurrentMultiplier = 1.00m;
      RevealedTiles = 0;
    }

    private void LoadRecentGames()
    {
      // Sample recent games data
      RecentGames.Add(new GameResultViewModel { MineCount = 3, Amount = 45.30m, IsWin = true });
      RecentGames.Add(new GameResultViewModel { MineCount = 5, Amount = -20.00m, IsWin = false });
      RecentGames.Add(new GameResultViewModel { MineCount = 2, Amount = 15.75m, IsWin = true });
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
    public string AmountText => IsWin ? $"+R$ {Amount:F2}" : $"-R$ {Math.Abs(Amount):F2}";
  }
}