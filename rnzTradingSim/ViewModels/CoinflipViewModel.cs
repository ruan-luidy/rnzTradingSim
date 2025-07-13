using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace rnzTradingSim.ViewModels
{
  public partial class CoinflipViewModel : ObservableObject
  {
    private readonly GamblingViewModel _parentViewModel;

    #region Properties

    [ObservableProperty]
    private decimal balance;

    [ObservableProperty]
    private decimal betAmount = 100;

    [ObservableProperty]
    private string selectedSide = "HEADS";

    [ObservableProperty]
    private bool isFlipping = false;

    #endregion

    #region Constructor

    public CoinflipViewModel(GamblingViewModel parentViewModel)
    {
      _parentViewModel = parentViewModel;
      Balance = parentViewModel.Balance;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void SelectSide(string side)
    {
      SelectedSide = side;
    }

    [RelayCommand]
    private void SetBetPercentage(string percentageStr)
    {
      if (double.TryParse(percentageStr, out double percentage))
      {
        BetAmount = Math.Round(Balance * (decimal)percentage, 2);
        if (BetAmount < 1) BetAmount = 1;
      }
    }

    [RelayCommand]
    private async Task Flip()
    {
      if (IsFlipping || BetAmount > Balance || BetAmount <= 0)
        return;

      IsFlipping = true;

      try
      {
        // Deduct bet amount
        Balance -= BetAmount;
        _parentViewModel.UpdateBalance(Balance);

        // Simulate coin flip animation delay (1.8 seconds to match animation)
        await Task.Delay(1800);

        // Generate random result
        var random = new Random();
        var result = random.NextDouble() < 0.5 ? "HEADS" : "TAILS";
        var won = result == SelectedSide;

        if (won)
        {
          var winAmount = BetAmount * 1.95m; // 95% return (5% house edge)
          Balance += winAmount;
          _parentViewModel.UpdateBalance(Balance);

          var profit = winAmount - BetAmount;
          _parentViewModel.AddActivity("Coinflip", true, profit);

          ShowResult($"🎉 YOU WON!\nResult: {result}\nWon: ${profit:N2}", true);
        }
        else
        {
          _parentViewModel.AddActivity("Coinflip", false, -BetAmount);
          ShowResult($"😞 You Lost!\nResult: {result}\nLost: ${BetAmount:N2}", false);
        }
      }
      finally
      {
        IsFlipping = false;
      }
    }

    #endregion

    #region Private Methods

    private void ShowResult(string message, bool isWin)
    {
      var icon = isWin ? MessageBoxImage.Information : MessageBoxImage.Warning;
      var title = isWin ? "Congratulations!" : "Better luck next time!";
      MessageBox.Show(message, title, MessageBoxButton.OK, icon);
    }

    #endregion
  }
}