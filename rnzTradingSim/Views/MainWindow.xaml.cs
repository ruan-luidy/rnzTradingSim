using System.Windows;
using rnzTradingSim.ViewModels;

namespace rnzTradingSim
{
  public partial class MainWindow : HandyControl.Controls.Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainWindowViewModel();
    }
  }
}