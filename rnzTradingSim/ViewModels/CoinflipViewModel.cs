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

    [ObservableProperty]
    private string finalResult = "HEADS"; // Resultado final para sincronizar com a animação

    // Propriedade para acessar o balance do parent
    public decimal ParentBalance => _parentViewModel.Balance;

    #endregion

    #region Constructor

    public CoinflipViewModel(GamblingViewModel parentViewModel)
    {
      _parentViewModel = parentViewModel;

      // Subscribe to parent balance changes to notify our ParentBalance property
      _parentViewModel.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(GamblingViewModel.Balance))
        {
          OnPropertyChanged(nameof(ParentBalance));
        }
      };
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
      if (double.TryParse(percentageStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double percentage))
      {
        var mainBalance = _parentViewModel.Balance;
        BetAmount = Math.Round(mainBalance * (decimal)percentage, 2);
        if (BetAmount < 1) BetAmount = 1;

        // Debug para verificar os valores
        System.Diagnostics.Debug.WriteLine($"Percentage string: '{percentageStr}', parsed: {percentage}");
        System.Diagnostics.Debug.WriteLine($"Balance: {mainBalance}, Calculation: {mainBalance} * {percentage} = {BetAmount}");
      }
      else
      {
        System.Diagnostics.Debug.WriteLine($"Failed to parse percentage: '{percentageStr}'");
      }
    }

    [RelayCommand]
    private async Task Flip()
    {
      if (IsFlipping || BetAmount > _parentViewModel.Balance || BetAmount <= 0)
        return;

      try
      {
        // Deduct bet amount from main balance
        _parentViewModel.UpdateBalance(_parentViewModel.Balance - BetAmount);

        // Generate random result FIRST
        var random = new Random();
        var result = random.NextDouble() < 0.5 ? "HEADS" : "TAILS";

        // Set the final result BEFORE starting animation
        FinalResult = result;

        // Start flipping animation
        IsFlipping = true;

        // Wait for animation to complete (1.4 seconds)
        await Task.Delay(1400);

        // Stop animation - this will trigger the final result display
        IsFlipping = false;

        // Small delay to show result clearly
        await Task.Delay(300);

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
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error in Flip: {ex.Message}");
        IsFlipping = false;
      }
    }

    #endregion

    #region Public Methods

    public void ResetSession()
    {
      SessionBalance = 0;
      // Resetar para HEADS quando resetar a sessão
      FinalResult = "HEADS";
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