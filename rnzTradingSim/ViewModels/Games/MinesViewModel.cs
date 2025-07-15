using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using rnzTradingSim.Models;
using rnzTradingSim.Services;
using System.Globalization;
using rnzTradingSim.Helpers;
using System.ComponentModel;
using static GameConstants;

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
    private bool canStartGame = false;

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

    // Propriedades para o botão unificado
    [ObservableProperty]
    private string mainActionButtonText = "Iniciar Jogo";

    [ObservableProperty]
    private bool isMainActionEnabled = false;

    // Propriedades para o card de resultado
    [ObservableProperty]
    private bool showResultCard = false;

    [ObservableProperty]
    private string resultCardTitle = "";

    [ObservableProperty]
    private string resultCardAmount = "";

    [ObservableProperty]
    private string resultCardDescription = "";

    [ObservableProperty]
    private bool isResultPositive = false;

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
      UpdateMainActionButton();

      System.Diagnostics.Debug.WriteLine($"MinesViewModel initialized with balance: {PlayerBalance}");
    }

    [RelayCommand]
    private void MainAction()
    {
      if (!IsGameActive && CanStartGame)
      {
        // Iniciar jogo
        StartGame();
      }
      else if (CanCollectWinnings)
      {
        // Sacar ganhos
        CollectWinnings();
      }
      else if (GameStatus == "PERDEU")
      {
        // Reiniciar após derrota
        RestartGame();
      }
    }

    private void StartGame()
    {
      // Ocultar o card de resultado anterior
      ShowResultCard = false;

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
      GameStatus = "JOGANDO";
      GameStatusDescription = "Clique nos tiles";
      RevealedTiles = 0;
      CurrentMultiplier = 1.00m;

      ResetMineGrid();
      PlaceMines();
      CalculatePotentialWin();
      UpdateMainActionButton();

      // Trigger visual reset
      OnPropertyChanged(nameof(IsGameActive));
    }

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

      // Mostrar card de resultado
      ShowWinResultCard(PotentialWin - BetAmount);

      RecentGames.Insert(0, new GameResultViewModel
      {
        MineCount = NumberOfMines,
        Amount = PotentialWin - BetAmount,
        IsWin = true
      });

      EndGame(true);
      UpdateMainActionButton();
    }

    private void RestartGame()
    {
      // Resetar estado do jogo
      IsGameActive = false;
      CanCollectWinnings = false;
      GameStatus = "AGUARDANDO";
      GameStatusDescription = "Configure sua aposta";
      RevealedTiles = 0;
      CurrentMultiplier = 1.00m;
      ShowResultCard = false;

      // Resetar grid visual
      ResetMineGrid();
      CalculatePotentialWin();
      UpdateMainActionButton();

      // Trigger para resetar o grid visual
      OnPropertyChanged("ForceGridReset");
    }

    [RelayCommand]
    private void RevealTile(MineButtonViewModel button)
    {
      if (!IsGameActive || button.IsRevealed) return;

      button.IsRevealed = true;

      if (button.IsMine)
      {
        HandleGameLoss();
      }
      else
      {
        // Safe tile revealed
        RevealedTiles++;
        CalculateCurrentMultiplier();
        CalculatePotentialWin();
        CanCollectWinnings = true;
        UpdateMainActionButton();

        // Check if all safe tiles are revealed
        int safeTiles = TotalTiles - NumberOfMines;
        if (RevealedTiles >= safeTiles)
        {
          // Auto-collect maximum win
          CollectWinnings();
        }
      }
    }

    private void HandleGameLoss()
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

      // Mostrar card de resultado negativo
      ShowLossResultCard(BetAmount);

      RecentGames.Insert(0, new GameResultViewModel
      {
        MineCount = NumberOfMines,
        Amount = -BetAmount,
        IsWin = false
      });

      EndGame(false);
      UpdateMainActionButton();
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
      if (NumberOfMines > 3 && !IsGameActive)
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

      if (BetAmount < MIN_BET) BetAmount = MIN_BET;
      if (BetAmount > MAX_BET) BetAmount = MAX_BET;

      CalculatePotentialWin();
    }

    partial void OnBetAmountChanged(decimal value)
    {
      // Garantir que seja sempre positivo e dentro dos limites
      if (value < MIN_BET)
      {
        BetAmount = MIN_BET;
        return;
      }

      if (value > MAX_BET)
      {
        BetAmount = MAX_BET;
        return;
      }

      CalculatePotentialWin();
      UpdateGameStatusDescription();
      UpdateMainActionButton();
    }

    [RelayCommand]
    private void UpdateBetAmount(string newValue)
    {
      if (IsGameActive) return;

      // Usar helper para parsing consistente
      decimal amount = CurrencyHelper.ParseCurrency(newValue);

      // Aplicar limites
      if (amount < MIN_BET) amount = MIN_BET;
      if (amount > Math.Min(PlayerBalance, MAX_BET))
        amount = Math.Min(PlayerBalance, MAX_BET);

      BetAmount = amount;
    }

    partial void OnNumberOfMinesChanged(int value)
    {
      CalculateProbability();
      CalculatePotentialWin();
    }

    private void UpdateMainActionButton()
    {
      if (!IsGameActive)
      {
        // Estado: Aguardando iniciar jogo
        CanStartGame = BetAmount > 0 && BetAmount <= PlayerBalance;
        CanCollectWinnings = false;

        if (GameStatus == "PERDEU")
        {
          MainActionButtonText = "Reiniciar";
          IsMainActionEnabled = true;
        }
        else
        {
          MainActionButtonText = "Iniciar Jogo";
          IsMainActionEnabled = CanStartGame;
        }
      }
      else
      {
        // Estado: Jogo ativo
        CanStartGame = false;

        if (CanCollectWinnings)
        {
          MainActionButtonText = $"Sacar {PotentialWin.FormatAbbreviated()}";
          IsMainActionEnabled = true;
        }
        else
        {
          MainActionButtonText = "Aguardando...";
          IsMainActionEnabled = false;
        }
      }
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

    private void ShowWinResultCard(decimal profit)
    {
      ResultCardTitle = "Vitória!";
      ResultCardAmount = $"+{profit.FormatAbbreviated()}";
      ResultCardDescription = $"Você ganhou {profit.FormatAbbreviated()} com {RevealedTiles} tiles revelados";
      IsResultPositive = true;
      ShowResultCard = true;
    }

    private void ShowLossResultCard(decimal loss)
    {
      ResultCardTitle = "Derrota!";
      ResultCardAmount = $"-{loss.FormatAbbreviated()}";
      ResultCardDescription = $"Você perdeu {loss.FormatAbbreviated()} ao acertar uma mina";
      IsResultPositive = false;
      ShowResultCard = true;
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

      // Cálculo usando apenas decimal
      decimal multiplier = 1.0m;

      for (int i = 0; i < RevealedTiles; i++)
      {
        decimal currentSafeTiles = safeTiles - i;
        decimal currentTotalTiles = TotalTiles - i;

        if (currentTotalTiles <= 0) break;

        // Multiplicador com house edge - tudo em decimal
        decimal tileMultiplier = (currentTotalTiles / currentSafeTiles) * MINES_HOUSE_EDGE;
        multiplier *= tileMultiplier;
      }

      // Limitar multiplicador máximo
      CurrentMultiplier = Math.Min(multiplier, 999999m);
    }

    private void CalculatePotentialWin()
    {
      PotentialWin = BetAmount * CurrentMultiplier;

      // Limitar ao pagamento máximo
      if (PotentialWin > MAX_PAYOUT)
      {
        PotentialWin = MAX_PAYOUT;
      }
    }

    private void CalculateProbability()
    {
      int safeTiles = TotalTiles - NumberOfMines;

      // Evitar divisão por zero
      if (safeTiles <= 0)
      {
        MultiplierText = "∞";
        ProbabilityPercentText = "0.00%";
        ProbabilityText = "Impossível ganhar com tantas minas!";
        return;
      }

      // Probabilidade usando decimal
      decimal probability = ((decimal)safeTiles / TotalTiles) * 100m;

      // Multiplicador base por tile com house edge
      decimal multiplierPerTile = ((decimal)TotalTiles / safeTiles) * MINES_HOUSE_EDGE;

      // Limitar valores extremos
      if (multiplierPerTile > 100m)
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

      // Formatação com K, M, B para aposta máxima
      decimal maxBet = Math.Min(PlayerBalance, MAX_BET);
      MaxBetText = $"Aposta máxima: {maxBet.FormatAbbreviated()}";

      // Atualizar estado do botão quando o saldo muda
      UpdateMainActionButton();

      // Debug: verificar se o saldo foi carregado corretamente
      System.Diagnostics.Debug.WriteLine($"Player Balance updated: {PlayerBalance}");
    }

    private void EndGame(bool won)
    {
      IsGameActive = false;
      CanCollectWinnings = false;

      GameStatus = won ? "GANHOU" : "PERDEU";
      GameStatusDescription = won ? "Parabéns!" : "Tente novamente";

      CurrentMultiplier = 1.00m;
      RevealedTiles = 0;
      CalculatePotentialWin();
      UpdateMainActionButton();

      if (won)
      {
        OnPropertyChanged(nameof(GameStatus));
        OnPropertyChanged("GameWon");
      }
      else
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
          MineCount = NumberOfMines,
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