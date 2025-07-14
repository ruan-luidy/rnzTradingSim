using System.Windows;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainWindowViewModel();
    }
  }
}