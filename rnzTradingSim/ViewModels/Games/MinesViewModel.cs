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
    private decimal playerBalance;

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

    [ObservableProperty]
    private string multiplierText = "1.08x";

    [ObservableProperty]
    private string probabilityPercentText = "88.00%";

    [ObservableProperty]
    private string probabilityText = "Você receberá 1.08x por tile, probabilidade de ganhar: 88.00%";

    [ObservableProperty]
    private string maxBetText = "Aposta máxima: R$ 1.000.000";

    [ObservableProperty]
    private string collectButtonText = "Sacar Ganhos";

    public ObservableCollection<MineButtonViewModel> MineButtons { get; }
    public ObservableCollection<GameResultViewModel> RecentGames { get; }

    public MinesViewModel()
    {
      _playerService = new PlayerService();

      MineButtons = new ObservableCollection<MineButtonViewModel>();
      RecentGames = new ObservableCollection<GameResultViewModel>();

      // Primeiro carrega o player e atualiza os dados
      _currentPlayer = _playerService.GetCurrentPlayer();
      UpdatePlayerData();

      // Se ainda estiver com saldo 0, força a criação de um novo player
      if (PlayerBalance <= 0)
      {
        System.Diagnostics.Debug.WriteLine("Balance is 0, resetting player...");
        _playerService.ResetPlayer();
        _currentPlayer = _playerService.GetCurrentPlayer();
        UpdatePlayerData();
      }

      InitializeMineGrid();
      LoadRecentGames();
      CalculateProbability();
      CalculatePotentialWin();

      // Atualiza a descrição inicial baseada no saldo atual
      UpdateGameStatusDescription();

      System.Diagnostics.Debug.WriteLine($"MinesViewModel initialized with balance: {PlayerBalance}");
    }

    [RelayCommand]
    private void StartGame()
    {
      // Validações mais específicas
      if (IsGameActive)
      {
        GameStatusDescription = "Jogo já em andamento!";
        return;
      }

      if (BetAmount <= 0)
      {
        GameStatusDescription = "Digite um valor de aposta válido!";
        return;
      }

      if (BetAmount > PlayerBalance)
      {
        GameStatusDescription = "Saldo insuficiente!";
        return;
      }

      // Inicia o jogo
      IsGameActive = true;
      CanStartGame = false;
      CanCollectWinnings = false;
      GameStatus = "JOGANDO";
      GameStatusDescription = "Clique nos tiles";
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
        CollectButtonText = $"Sacar R$ {PotentialWin:F2}";

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
      if (NumberOfMines < 23 && !IsGameActive)
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
      // Remove validação que resetava valores negativos
      // Apenas garante que seja positivo
      if (value < 0)
      {
        BetAmount = 0.01m;
        return;
      }

      // Atualiza o potencial win sempre que o bet amount muda
      CalculatePotentialWin();

      // Atualiza a descrição do status se necessário
      UpdateGameStatusDescription();
    }

    [RelayCommand]
    private void UpdateBetAmount(string newValue)
    {
      if (IsGameActive) return;

      if (decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal amount))
      {
        // Aplicar limites mínimos
        if (amount < 0.01m) amount = 0.01m;
        if (amount > 100000) amount = 100000;

        BetAmount = amount;
      }
      else
      {
        // Se não conseguir converter, mantém valor anterior ou define mínimo
        if (BetAmount <= 0) BetAmount = 0.01m;
      }
    }

    partial void OnNumberOfMinesChanged(int value)
    {
      CalculateProbability();
      CalculatePotentialWin();
    }

    private void UpdateGameStatusDescription()
    {
      if (!IsGameActive)
      {
        if (BetAmount > PlayerBalance)
        {
          GameStatusDescription = "Saldo insuficiente!";
        }
        else if (BetAmount <= 0)
        {
          GameStatusDescription = "Digite um valor de aposta válido!";
        }
        else
        {
          GameStatusDescription = "Configure sua aposta";
        }
      }
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

      if (safeTiles <= 0)
      {
        CurrentMultiplier = 1.00m;
        return;
      }

      // Cálculo mais preciso do multiplicador baseado em combinações
      double multiplier = 1.0;

      for (int i = 0; i < RevealedTiles; i++)
      {
        double currentSafeTiles = safeTiles - i;
        double currentTotalTiles = TotalTiles - i;

        if (currentTotalTiles <= 0) break;

        // Multiplicador com house edge
        double tileMultiplier = currentTotalTiles / currentSafeTiles * 0.97;
        multiplier *= tileMultiplier;
      }

      // Limitar multiplicador máximo
      CurrentMultiplier = (decimal)Math.Min(multiplier, 999999);
    }

    private void CalculatePotentialWin()
    {
      PotentialWin = BetAmount * CurrentMultiplier;
    }

    private void CalculateProbability()
    {
      int safeTiles = TotalTiles - NumberOfMines;

      // Evitar divisão por zero e valores inválidos
      if (safeTiles <= 0)
      {
        MultiplierText = "∞";
        ProbabilityPercentText = "0.00%";
        ProbabilityText = "Impossível ganhar com tantas minas!";
        return;
      }

      // Probabilidade de acertar o primeiro tile seguro
      double probability = (double)safeTiles / TotalTiles * 100;

      // Multiplicador base por tile (house edge de ~3%)
      double multiplierPerTile = (double)TotalTiles / safeTiles * 0.97;

      // Limitar valores extremos
      if (multiplierPerTile > 100)
      {
        MultiplierText = "99.99x";
        ProbabilityPercentText = $"{probability:F2}%";
        ProbabilityText = $"Multiplicador muito alto! Probabilidade: {probability:F2}%";
      }
      else
      {
        MultiplierText = $"{multiplierPerTile:F2}x";
        ProbabilityPercentText = $"{probability:F2}%";
        ProbabilityText = $"Você receberá {multiplierPerTile:F2}x por tile, probabilidade de ganhar: {probability:F2}%";
      }
    }

    private void UpdatePlayerData()
    {
      _currentPlayer = _playerService.GetCurrentPlayer();
      PlayerBalance = _currentPlayer.Balance;
      MaxBetText = $"Aposta máxima: R$ {Math.Min(PlayerBalance, 100000):N2}";

      // Debug: verificar se o saldo foi carregado corretamente
      System.Diagnostics.Debug.WriteLine($"Player Balance updated: {PlayerBalance}");
    }

    private void EndGame(bool won)
    {
      IsGameActive = false;
      CanStartGame = true;
      CanCollectWinnings = false;

      GameStatus = won ? "GANHOU" : "PERDEU";
      GameStatusDescription = won ? "Parabéns!" : "Tente novamente";

      CurrentMultiplier = 1.00m;
      RevealedTiles = 0;
      CollectButtonText = "Sacar Ganhos";
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
    public string AmountText => IsWin ? $"+R$ {Amount:F2}" : $"-R$ {Math.Abs(Amount):F2}";
  }
}