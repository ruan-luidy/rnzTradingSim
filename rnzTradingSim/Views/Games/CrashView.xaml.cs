using System.Windows.Controls;
using rnzTradingSim.ViewModels.Games;

namespace rnzTradingSim.Views.Games
{
    public partial class CrashView : UserControl
    {
        public CrashView()
        {
            InitializeComponent();
            DataContext = new CrashViewModel();
        }
    }
}