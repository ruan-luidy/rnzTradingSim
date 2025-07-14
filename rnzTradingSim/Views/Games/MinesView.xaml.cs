using System.Windows.Controls;
using rnzTradingSim.ViewModels.Games;

namespace rnzTradingSim.Views.Games
{
  public partial class MinesView : UserControl
  {
    public MinesView()
    {
      InitializeComponent();
      DataContext = new MinesViewModel();
    }
  }
}