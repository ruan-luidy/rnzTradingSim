using System.Windows.Controls;
using System.Windows.Input;
using rnzTradingSim.ViewModels;
using rnzTradingSim.Models;

namespace rnzTradingSim.Views
{
  public partial class HomeView : UserControl
  {
    public HomeView()
    {
      InitializeComponent();
      DataContext = new HomeViewModel();
    }

    private void CoinCard_Click(object sender, MouseButtonEventArgs e)
    {
      if (sender is Border border && border.Tag is CoinData coin)
      {
        NavigateToCoinDetail(coin);
      }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is DataGrid dataGrid && dataGrid.SelectedItem is CoinData coin)
      {
        NavigateToCoinDetail(coin);
      }
    }

    private void NavigateToCoinDetail(CoinData coin)
    {
      var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
      if (mainWindow?.DataContext is MainWindowViewModel mainVM)
      {
        mainVM.NavigateToCoinDetail(coin.Id);
      }
    }
  }
}
