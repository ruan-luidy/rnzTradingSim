using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace rnzTradingSim.ViewModels
{
  public partial class SlotsViewModel : ObservableObject
  {
    private readonly GamblingViewModel _parentViewModel;

    #region Properties

    [ObservableProperty]
    private decimal sessionBalance = 0; // Ganhos/perdas da sessão atual

    [ObservableProperty]
    private decimal betAmount = 50;

    [ObservableProperty]
    private bool isSpinning = false;

    [ObservableProperty]
    private ObservableCollection<string> reels = new() { "🍒", "🍋", "⭐" };

    #endregion

    #region Constructor

    public SlotsViewModel(GamblingViewModel parentViewModel)
    {
      _parentViewModel = parentViewModel;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void SetBetPercentage(string percentageStr)
    {
      if (double.TryParse(percentageStr, out double percentage))
      {
        var mainBalance = _parentViewModel.Balance;
        BetAmount = Math.Round(mainBalance * (decimal)percentage, 2);
        if (BetAmount < 1) BetAmount = 1;
      }
    }

    [RelayCommand]
    private async Task Spin()
    {
      if (IsSpinning || BetAmount > _parentViewModel.Balance || BetAmount <= 0)
        return;

      IsSpinning = true;

      try
      {
        // Deduct bet amount from main balance
        _parentViewModel.UpdateBalance(_parentViewModel.Balance - BetAmount);

        // Simulate spinning animation
        var random = new Random();
        for (int i = 0; i < 20; i++)
        {
          Reels[0] = GetRandomSlotSymbol(random);
          Reels[1] = GetRandomSlotSymbol(random);
          Reels[2] = GetRandomSlotSymbol(random);
          await Task.Delay(100);
        }

        // Final result with weighted symbols
        var finalReels = new string[3];
        for (int i = 0; i < 3; i++)
        {
          finalReels[i] = GetWeightedSlotSymbol(random);
          Reels[i] = finalReels[i];
        }

        // Check for wins
        var multiplier = GetSlotMultiplier(finalReels);
        if (multiplier > 0)
        {
          var winAmount = BetAmount * multiplier;
          _parentViewModel.UpdateBalance(_parentViewModel.Balance + winAmount);

          var profit = winAmount - BetAmount;
          SessionBalance += profit; // Update session balance
          _parentViewModel.AddActivity("Slots", true, profit);

          ShowResult($"🎰 JACKPOT! 🎰\n{string.Join(" ", finalReels)}\n{multiplier}x Multiplier!\nWon: ${profit:N2}", true);
        }
        else
        {
          SessionBalance -= BetAmount; // Update session balance
          _parentViewModel.AddActivity("Slots", false, -BetAmount);
          ShowResult($"No luck this time!\n{string.Join(" ", finalReels)}\nLost: ${BetAmount:N2}", false);
        }
      }
      finally
      {
        IsSpinning = false;
      }
    }

    #endregion

    #region Public Methods

    public void ResetSession()
    {
      SessionBalance = 0;
    }

    #endregion

    #region Private Methods

    private string GetRandomSlotSymbol(Random random)
    {
      var symbols = new[] { "🍒", "🍋", "⭐", "💎", "🍊", "🔔", "7️⃣" };
      return symbols[random.Next(symbols.Length)];
    }

    private string GetWeightedSlotSymbol(Random random)
    {
      // Weighted symbol selection for realistic slot machine odds
      var value = random.NextDouble();

      if (value < 0.4) return "🍒"; // 40% chance
      if (value < 0.7) return "🍋"; // 30% chance
      if (value < 0.85) return "🍊"; // 15% chance
      if (value < 0.95) return "🔔"; // 10% chance
      if (value < 0.99) return "⭐"; // 4% chance
      return "💎"; // 1% chance
    }

    private decimal GetSlotMultiplier(string[] reels)
    {
      // Check for three of a kind
      if (reels[0] == reels[1] && reels[1] == reels[2])
      {
        return reels[0] switch
        {
          "💎" => 100m,
          "⭐" => 50m,
          "🔔" => 25m,
          "🍊" => 15m,
          "🍋" => 10m,
          "🍒" => 5m,
          "7️⃣" => 75m,
          _ => 2m
        };
      }

      // Check for two of a kind (smaller multiplier)
      if ((reels[0] == reels[1]) || (reels[1] == reels[2]) || (reels[0] == reels[2]))
      {
        var symbol = reels[0] == reels[1] ? reels[0] :
                    reels[1] == reels[2] ? reels[1] : reels[0];

        return symbol switch
        {
          "💎" => 10m,
          "⭐" => 5m,
          "🔔" => 3m,
          "7️⃣" => 8m,
          _ => 0m
        };
      }

      return 0m; // No win
    }

    private void ShowResult(string message, bool isWin)
    {
      var icon = isWin ? MessageBoxImage.Information : MessageBoxImage.Warning;
      var title = isWin ? "Congratulations!" : "Better luck next time!";
      MessageBox.Show(message, title, MessageBoxButton.OK, icon);
    }

    #endregion
  }
}