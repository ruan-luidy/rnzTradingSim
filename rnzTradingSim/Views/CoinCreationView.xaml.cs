using System.Windows.Controls;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim.Views
{
    public partial class CoinCreationView : UserControl
    {
        public CoinCreationView()
        {
            InitializeComponent();
            DataContext = new CoinCreationViewModel();
        }
    }
}