using System.Windows.Controls;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim.Views
{
  public partial class GamblingView : UserControl
  {
    public GamblingView()
    {
      InitializeComponent();
      DataContext = new GamblingViewModel();
    }
  }
}