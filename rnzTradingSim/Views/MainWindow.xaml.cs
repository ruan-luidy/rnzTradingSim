using rnzTradingSim.ViewModels;

namespace rnzTradingSim.Views;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainWindowViewModel();
  }
}