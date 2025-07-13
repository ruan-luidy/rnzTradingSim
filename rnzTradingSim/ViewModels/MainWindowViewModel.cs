using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using rnzTradingSim.Models;

namespace rnzTradingSim.ViewModels
{
  public partial class MainWindowViewModel : ObservableObject
  {
    [ObservableProperty]
    private decimal balance = 10000m;

    [ObservableProperty]
    private decimal profitLoss = 2450m;

    [ObservableProperty]
    private int rank = 127;

    [ObservableProperty]
    private decimal coinflipBetAmount = 100m;

    [ObservableProperty]
    private decimal slotsBetAmount = 50m;

    [ObservableProperty]
    private string selectedCoinflipSide = "HEADS";

    [ObservableProperty]
    private bool isFlipping = false;

    [ObservableProperty]
    private bool isSpinning = false;

    [ObservableProperty]
    private string lastCoinflipResult = "";

    [ObservableProperty]
    private string[] slotReels = { "🍒", "🍋", "🍒" };

    [ObservableProperty]
    private decimal totalWinnings = 3247m;

    [ObservableProperty]
    private decimal totalLosses = 797m;

    [ObservableProperty]
    private int gamesPlayed = 89;

    public ObservableCollection<GameActivity> RecentActivities { get; }
    public ObservableCollection<string> CoinflipHistory { get; }

    private readonly Random _random = new();
    private readonly string[] _slotSymbols = { "🍒", "🍋", "⭐", "🍊", "💎", "🔔" };

    public MainWindowViewModel()
    {
      RecentActivities = new ObservableCollection<GameActivity>
            {
                new GameActivity { Game = "Coinflip - HEADS", TimeAgo = "2 min ago", Amount = 200m, IsWin = true },
                new GameActivity { Game = "Slots - No Win", TimeAgo = "5 min ago", Amount = -50m, IsWin = false },
                new GameActivity { Game = "Coinflip - TAILS", TimeAgo = "8 min ago", Amount = -100m, IsWin = false },
                new GameActivity { Game = "Slots - 🍒🍒🍒", TimeAgo = "12 min ago", Amount = 500m, IsWin = true }
            };

      CoinflipHistory = new ObservableCollection<string> { "H", "T", "H", "T", "H" };
    }

    [RelayCommand]
    private async Task FlipCoin()
    {
      if (IsFlipping || CoinflipBetAmount > Balance)
        return;

      if (CoinflipBetAmount <= 0)
      {
        MessageBox.Show("Please enter a valid bet amount.", "Invalid Bet", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      IsFlipping = true;

      try
      {
        // Simulate coin flip animation delay
        await Task.Delay(2000);

        // Generate random result
        string result = _random.Next(2) == 0 ? "HEADS" : "TAILS";
        bool won = result == SelectedCoinflipSide;

        LastCoinflipResult = result;

        // Update history
        CoinflipHistory.Insert(0, result.Substring(0, 1));
        if (CoinflipHistory.Count > 10)
          CoinflipHistory.RemoveAt(CoinflipHistory.Count - 1);

        // Calculate winnings/losses
        decimal amount = won ? CoinflipBetAmount : -CoinflipBetAmount;

        Balance += amount;
        ProfitLoss += amount;
        GamesPlayed++;

        if (won)
        {
          TotalWinnings += CoinflipBetAmount;
        }
        else
        {
          TotalLosses += CoinflipBetAmount;
        }

        // Add to activity
        var activity = new GameActivity
        {
          Game = $"Coinflip - {result}",
          TimeAgo = "Just now",
          Amount = amount,
          IsWin = won
        };

        RecentActivities.Insert(0, activity);
        if (RecentActivities.Count > 10)
          RecentActivities.RemoveAt(RecentActivities.Count - 1);

        // Show result message
        string message = won ? $"YOU WON! +${CoinflipBetAmount}" : $"You lost! -${CoinflipBetAmount}";
        MessageBox.Show(message, result, MessageBoxButton.OK,
            won ? MessageBoxImage.Information : MessageBoxImage.Warning);
      }
      finally
      {
        IsFlipping = false;
      }
    }

    [RelayCommand]
    private async Task SpinSlots()
    {
      if (IsSpinning || SlotsBetAmount > Balance)
        return;

      if (SlotsBetAmount <= 0)
      {
        MessageBox.Show("Please enter a valid bet amount.", "Invalid Bet", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      IsSpinning = true;

      try
      {
        // Simulate spinning animation
        for (int i = 0; i < 10; i++)
        {
          SlotReels = new string[]
          {
                        _slotSymbols[_random.Next(_slotSymbols.Length)],
                        _slotSymbols[_random.Next(_slotSymbols.Length)],
                        _slotSymbols[_random.Next(_slotSymbols.Length)]
          };
          await Task.Delay(200);
        }

        // Final result
        SlotReels = new string[]
        {
                    _slotSymbols[_random.Next(_slotSymbols.Length)],
                    _slotSymbols[_random.Next(_slotSymbols.Length)],
                    _slotSymbols[_random.Next(_slotSymbols.Length)]
        };

        // Check for wins
        decimal multiplier = 0m;
        bool won = false;
        string resultText = "No Win";

        if (SlotReels[0] == SlotReels[1] && SlotReels[1] == SlotReels[2])
        {
          won = true;
          switch (SlotReels[0])
          {
            case "🍒":
              multiplier = 50m;
              resultText = "🍒🍒🍒";
              break;
            case "🍋":
              multiplier = 25m;
              resultText = "🍋🍋🍋";
              break;
            case "⭐":
              multiplier = 100m;
              resultText = "⭐⭐⭐";
              break;
            case "💎":
              multiplier = 200m;
              resultText = "💎💎💎";
              break;
            default:
              multiplier = 10m;
              resultText = $"{SlotReels[0]}{SlotReels[0]}{SlotReels[0]}";
              break;
          }
        }

        decimal amount = won ? (SlotsBetAmount * multiplier) - SlotsBetAmount : -SlotsBetAmount;

        Balance += amount;
        ProfitLoss += amount;
        GamesPlayed++;

        if (won)
        {
          TotalWinnings += Math.Abs(amount);
        }
        else
        {
          TotalLosses += SlotsBetAmount;
        }

        // Add to activity
        var activity = new GameActivity
        {
          Game = $"Slots - {resultText}",
          TimeAgo = "Just now",
          Amount = amount,
          IsWin = won
        };

        RecentActivities.Insert(0, activity);
        if (RecentActivities.Count > 10)
          RecentActivities.RemoveAt(RecentActivities.Count - 1);

        // Show result message
        if (won)
        {
          MessageBox.Show($"JACKPOT! You won ${Math.Abs(amount):F0} ({multiplier}x multiplier)!",
              "YOU WON!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
          MessageBox.Show($"No win this time. You lost ${SlotsBetAmount}",
              "Try Again!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }
      finally
      {
        IsSpinning = false;
      }
    }

    [RelayCommand]
    private void SelectCoinflipSide(string side)
    {
      if (!IsFlipping)
      {
        SelectedCoinflipSide = side;
      }
    }

    [RelayCommand]
    private void ShowLeaderboard()
    {
      MessageBox.Show("Leaderboard feature coming soon!", "Leaderboard", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ShowHistory()
    {
      MessageBox.Show("Detailed history feature coming soon!", "History", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    partial void OnCoinflipBetAmountChanged(decimal value)
    {
      if (value < 10m) CoinflipBetAmount = 10m;
      if (value > 5000m) CoinflipBetAmount = 5000m;
      if (value > Balance) CoinflipBetAmount = Balance;
    }

    partial void OnSlotsBetAmountChanged(decimal value)
    {
      if (value < 5m) SlotsBetAmount = 5m;
      if (value > 2500m) SlotsBetAmount = 2500m;
      if (value > Balance) SlotsBetAmount = Balance;
    }
  }
}