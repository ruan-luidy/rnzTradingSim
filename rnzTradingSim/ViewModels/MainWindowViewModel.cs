using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

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

      Debug.WriteLine($"MainWindowViewModel initialized with CurrentView: {CurrentView}");
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Navigate(string viewName)
    {
      Debug.WriteLine($"Navigate command called with: {viewName}");
      CurrentView = viewName;
      Debug.WriteLine($"CurrentView changed to: {CurrentView}");

      // Force property change notification
      OnPropertyChanged(nameof(CurrentView));
    }

    #endregion
  }
}