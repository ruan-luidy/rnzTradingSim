using System.Windows.Controls;
using System.Windows.Input;
using rnzTradingSim.ViewModels;
using rnzTradingSim.Models;

namespace rnzTradingSim.Views
{
  public partial class SimplePortfolioView : UserControl
  {
    public SimplePortfolioView()
    {
      InitializeComponent();
      DataContext = new SimplePortfolioViewModel();
    }

    private void HoldingsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is DataGrid dataGrid && dataGrid.SelectedItem is PortfolioHolding holding)
      {
        NavigateToCoinDetail(holding);
      }
    }

    private void NavigateToCoinDetail(PortfolioHolding holding)
    {
      var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
      if (mainWindow?.DataContext is MainWindowViewModel mainVM)
      {
        mainVM.NavigateToCoinDetail(holding.CoinId);
      }
    }
  }
}