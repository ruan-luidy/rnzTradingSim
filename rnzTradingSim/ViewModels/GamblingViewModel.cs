using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace rnzTradingSim.ViewModels
{
  public partial class GamblingViewModel : ObservableObject
  {
    [ObservableProperty]
    private string selectedGame = "MinesView";

    [ObservableProperty]
    private bool isMinesSelected = true;

    [ObservableProperty]
    private bool isCoinflipSelected = false;

    [ObservableProperty]
    private bool isDiceSelected = false;

    [ObservableProperty]
    private bool isSlotsSelected = false;

    public GamblingViewModel()
    {
      // Initialize with Mines as default selected game
    }

    [RelayCommand]
    private void SelectMines()
    {
      ResetGameSelection();
      IsMinesSelected = true;
      SelectedGame = "MinesView";
    }

    [RelayCommand]
    private void SelectCoinflip()
    {
      ResetGameSelection();
      IsCoinflipSelected = true;
      SelectedGame = "CoinflipView";
    }

    [RelayCommand]
    private void SelectDice()
    {
      ResetGameSelection();
      IsDiceSelected = true;
      SelectedGame = "DiceView";
    }

    [RelayCommand]
    private void SelectSlots()
    {
      ResetGameSelection();
      IsSlotsSelected = true;
      SelectedGame = "SlotsView";
    }

    private void ResetGameSelection()
    {
      IsMinesSelected = false;
      IsCoinflipSelected = false;
      IsDiceSelected = false;
      IsSlotsSelected = false;
    }
  }
}