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
    private decimal sessionBalance = 0; // Ganhos/perdas da sessão atual

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
        var mainBalance = _parentViewModel.Balance;
        BetAmount = Math.Round(mainBalance * (decimal)percentage, 2);
        if (BetAmount < 1) BetAmount = 1;
      }
    }

    [RelayCommand]
    private async Task Flip()
    {
      if (IsFlipping || BetAmount > _parentViewModel.Balance || BetAmount <= 0)
        return;

      IsFlipping = true;

      try
      {
        // Deduct bet amount from main balance
        _parentViewModel.UpdateBalance(_parentViewModel.Balance - BetAmount);

        // Simulate coin flip animation delay (1.8 seconds to match animation)
        await Task.Delay(1800);

        // Generate random result
        var random = new Random();
        var result = random.NextDouble() < 0.5 ? "HEADS" : "TAILS";
        var won = result == SelectedSide;

        if (won)
        {
          var winAmount = BetAmount * 1.95m; // 95% return (5% house edge)
          _parentViewModel.UpdateBalance(_parentViewModel.Balance + winAmount);

          var profit = winAmount - BetAmount;
          SessionBalance += profit; // Update session balance
          _parentViewModel.AddActivity("Coinflip", true, profit);

          ShowResult($"🎉 YOU WON!\nResult: {result}\nWon: ${profit:N2}", true);
        }
        else
        {
          SessionBalance -= BetAmount; // Update session balance
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

    #region Public Methods

    public void ResetSession()
    {
      SessionBalance = 0;
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