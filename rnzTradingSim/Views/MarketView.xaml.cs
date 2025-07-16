using System.Windows.Controls;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim.Views
{
  public partial class MarketView : UserControl
  {
    public MarketView()
    {
      InitializeComponent();
      DataContext = new MarketViewModel();
    }
  }
}