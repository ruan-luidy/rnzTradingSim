using System.Windows.Controls;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim.Views
{
  public partial class CoinDetailView : UserControl
  {
    public CoinDetailView()
    {
      InitializeComponent();
      DataContext = new CoinDetailViewModel();
    }

    public async void LoadCoin(string coinId)
    {
      if (DataContext is CoinDetailViewModel viewModel)
      {
        await viewModel.LoadCoinAsync(coinId);
      }
    }
  }
}