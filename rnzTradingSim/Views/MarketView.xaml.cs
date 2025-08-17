using System.Windows.Controls;
using System.Windows.Input;
using rnzTradingSim.ViewModels;
using rnzTradingSim.Models;

namespace rnzTradingSim.Views
{
  public partial class MarketView : UserControl
  {
    public MarketView()
    {
      InitializeComponent();
      DataContext = new MarketViewModel();
    }

    private void CoinCard_Click(object sender, MouseButtonEventArgs e)
    {
      if (sender is Border border && border.Tag is CoinData coin)
      {
        // Navegar para a página de detalhes da moeda
        var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        if (mainWindow?.DataContext is MainWindowViewModel mainVM)
        {
          // Navegar para CoinDetail - temporariamente usando o ID da moeda fake
          // Quando tivermos UserCoins, vamos usar o ID correto
          mainVM.NavigateToCoinDetail(coin.Id);
        }
      }
    }
  }
}