using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rnzTradingSim.Services;
using System.Windows.Threading;
using System.Linq;

namespace rnzTradingSim.ViewModels.Games
{
  public partial class CrashViewModel : ObservableObject
  {
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;
    private readonly DispatcherTimer _gameTimer;
    private readonly Random _random = new();

    private int _gameId;
    private decimal _crashPoint;
    private DateTime _gameStartTime;
    private bool _hasCashedOut;

    [ObservableProperty]
    private decimal betAmount = 10m;

    [ObservableProperty]
    private decimal currentMultiplier = 1.00m;

    [ObservableProperty]
    private decimal potentialPayout = 10m;

    [ObservableProperty]
    private bool isGameActive = false;

    [ObservableProperty]
    private bool isWaitingForNextGame = false;

    [ObservableProperty]
    private bool hasBet = false;

    [ObservableProperty]
    private bool canCashOut = false;

    [ObservableProperty]
    private string gameStatusText = "Place your bet and wait for next round";

    [ObservableProperty]
    private string lastResultText = "";

    [ObservableProperty]
    private decimal playerBalance = 1000m;

    [ObservableProperty]
    private int nextGameCountdown = 0;

    // Recent crashes for pattern analysis
    [ObservableProperty]
    private string recentCrashes = "2.34x, 1.08x, 15.67x, 1.24x, 3.45x";

    public CrashViewModel()
    {
      _gameService = new GameService(new PlayerService());
      _playerService = new PlayerService();

      // Timer for game updates (60 FPS for smooth multiplier)
      _gameTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
      };
      _gameTimer.Tick += OnGameTick;

      LoadPlayerBalance();
      ScheduleNextGame();
    }

    private void LoadPlayerBalance()
    {
      try
      {
        var player = _playerService.GetCurrentPlayer();
        PlayerBalance = player.Balance;
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error loading player balance", ex);
      }
    }

    [RelayCommand]
    private async Task PlaceBet()
    {
      if (BetAmount < 1 || BetAmount > PlayerBalance)
      {
        NotificationService.NotifyTradingError("Invalid bet amount!");
        return;
      }

      try
      {
        var game = await _gameService.StartGameAsync(Models.GameType.Mines, BetAmount,
          $"{{\"crashGame\": true, \"betAmount\": {BetAmount}}}");

        _gameId = game.Id;
        HasBet = true;
        _hasCashedOut = false;

        // Deduct bet from balance
        var player = _playerService.GetCurrentPlayer();
        player.Balance -= BetAmount;
        _playerService.SavePlayer(player);
        PlayerBalance = player.Balance;

        GameStatusText = "Bet placed! Waiting for game to start...";
        NotificationService.NotifyTradingSuccess($"Bet placed: ${BetAmount:N2}");

        OnPropertyChanged(nameof(CanPlaceBet));
      }
      catch (Exception ex)
      {
        NotificationService.NotifyTradingError($"Failed to place bet: {ex.Message}");
        LoggingService.Error("Error placing crash bet", ex);
      }
    }

    [RelayCommand]
    private void SetBet(object parameter)
    {
      if (parameter is string str && decimal.TryParse(str, out decimal amount))
      {
        BetAmount = Math.Min(amount, PlayerBalance);
      }
    }

    [RelayCommand]
    private async Task CashOut()
    {
      if (!CanCashOut || _hasCashedOut) return;

      try
      {
        _hasCashedOut = true;
        var payout = BetAmount * CurrentMultiplier;

        // Finish the game
        await _gameService.FinishGameAsync(_gameId, Models.GameStatus.Won, payout, CurrentMultiplier);

        // Add winnings to balance
        var player = _playerService.GetCurrentPlayer();
        player.Balance += payout;
        _playerService.SavePlayer(player);
        PlayerBalance = player.Balance;

        CanCashOut = false;
        GameStatusText = $"Cashed out at {CurrentMultiplier:F2}x!";
        LastResultText = $"Won ${payout:N2} at {CurrentMultiplier:F2}x";

        NotificationService.NotifyTradingSuccess($"Cashed out ${payout:N2} at {CurrentMultiplier:F2}x!");

        OnPropertyChanged(nameof(CanPlaceBet));
      }
      catch (Exception ex)
      {
        NotificationService.NotifyTradingError($"Failed to cash out: {ex.Message}");
        LoggingService.Error("Error cashing out", ex);
      }
    }

    public bool CanPlaceBet => !IsGameActive && !IsWaitingForNextGame && !HasBet && BetAmount > 0 && BetAmount <= PlayerBalance;

    private void ScheduleNextGame()
    {
      IsWaitingForNextGame = true;
      NextGameCountdown = 10; // 10 second countdown
      GameStatusText = "Next game starting in 10 seconds...";

      var countdownTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(1)
      };

      countdownTimer.Tick += (s, e) =>
      {
        NextGameCountdown--;
        GameStatusText = $"Next game starting in {NextGameCountdown} seconds...";

        if (NextGameCountdown <= 0)
        {
          countdownTimer.Stop();
          StartGame();
        }
      };

      countdownTimer.Start();
    }

    private void StartGame()
    {
      IsWaitingForNextGame = false;
      IsGameActive = true;
      CurrentMultiplier = 1.00m;
      _gameStartTime = DateTime.Now;

      // Generate crash point (house edge ~4%)
      _crashPoint = GenerateCrashPoint();

      if (HasBet)
      {
        CanCashOut = true;
        GameStatusText = "Game is live! Cash out any time!";
      }
      else
      {
        GameStatusText = "Game is live! (No bet placed)";
      }

      _gameTimer.Start();

      LoggingService.Info($"Crash game started - will crash at {_crashPoint:F2}x");
    }

    private decimal GenerateCrashPoint()
    {
      // Use inverse exponential distribution for realistic crash points
      // More crashes at low multipliers, rare high multipliers
      var random = _random.NextDouble();

      // House edge: 4% of games crash before 1.04x (instant crash)
      if (random < 0.04)
        return 1.00m + (decimal)(_random.NextDouble() * 0.04); // 1.00-1.04x

      // Exponential distribution for remaining 96%
      var lambda = 0.04; // Lower = more high multipliers
      var result = -Math.Log(1 - random) / lambda + 1;

      // Cap at reasonable maximum
      return Math.Min((decimal)result, 1000m);
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
      if (!IsGameActive) return;

      var elapsed = DateTime.Now - _gameStartTime;
      var seconds = elapsed.TotalSeconds;

      // Exponential growth: multiplier = e^(0.1 * seconds)
      var rawMultiplier = Math.Exp(0.06 * seconds); // Slightly slower growth
      CurrentMultiplier = Math.Max(1.00m, (decimal)rawMultiplier);

      // Update potential payout
      if (HasBet && !_hasCashedOut)
      {
        PotentialPayout = BetAmount * CurrentMultiplier;
      }

      // Check if we've reached the crash point
      if (CurrentMultiplier >= _crashPoint)
      {
        CrashGame();
      }
    }

    private async void CrashGame()
    {
      _gameTimer.Stop();
      IsGameActive = false;

      var crashedAt = _crashPoint;
      GameStatusText = $"💥 CRASHED at {crashedAt:F2}x";

      // Update recent crashes
      UpdateRecentCrashes(crashedAt);

      // Handle players who didn't cash out
      if (HasBet && !_hasCashedOut)
      {
        try
        {
          // Finish game as lost
          await _gameService.FinishGameAsync(_gameId, Models.GameStatus.Lost, 0, crashedAt);
          LastResultText = $"Lost ${BetAmount:N2} - crashed at {crashedAt:F2}x";

          NotificationService.ShowNotification($"💥 Crashed at {crashedAt:F2}x! Lost ${BetAmount:N2}",
            NotificationService.NotificationType.Warning);
        }
        catch (Exception ex)
        {
          LoggingService.Error("Error finishing crash game", ex);
        }
      }

      // Reset game state
      HasBet = false;
      CanCashOut = false;
      PotentialPayout = 0;
      OnPropertyChanged(nameof(CanPlaceBet));

      // Schedule next game after 3 seconds
      await Task.Delay(3000);
      ScheduleNextGame();
    }

    private void UpdateRecentCrashes(decimal crashPoint)
    {
      var crashes = RecentCrashes.Split(new[] { ", " }, StringSplitOptions.None).ToList();
      crashes.Insert(0, $"{crashPoint:F2}x");

      if (crashes.Count > 5)
        crashes = crashes.Take(5).ToList();

      RecentCrashes = string.Join(", ", crashes);
    }

    partial void OnBetAmountChanged(decimal value)
    {
      if (value > 0 && !_hasCashedOut)
      {
        PotentialPayout = value * CurrentMultiplier;
      }
      OnPropertyChanged(nameof(CanPlaceBet));
    }

    public void Dispose()
    {
      _gameTimer?.Stop();
      _gameService?.Dispose();
    }
  }
}