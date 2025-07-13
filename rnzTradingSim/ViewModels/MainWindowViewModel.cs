using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace rnzTradingSim.ViewModels
{
  public partial class MainWindowViewModel : ObservableObject
  {
    #region Properties

    [ObservableProperty]
    private decimal balance = 10000; // Starting balance in BUSS

    [ObservableProperty]
    private decimal profitLoss = 0;

    [ObservableProperty]
    private int rank = 1337;

    [ObservableProperty]
    private string currentView = "Gambling"; // Default to gambling view

    [ObservableProperty]
    private GamblingViewModel gamblingViewModel;

    #endregion

    #region Constructor

    public MainWindowViewModel()
    {
      GamblingViewModel = new GamblingViewModel();

      // Subscribe to balance updates from gambling
      GamblingViewModel.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(GamblingViewModel.Balance))
        {
          var oldBalance = Balance;
          Balance = GamblingViewModel.Balance;
          ProfitLoss += (Balance - oldBalance);
        }
      };
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Navigate(string viewName)
    {
      CurrentView = viewName;
    }

    #endregion
  }
}