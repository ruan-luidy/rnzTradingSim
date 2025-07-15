using DebugTool.ViewModels;

namespace DebugTool.Views;

public partial class MainWindow : HandyControl.Controls.Window
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainViewModel();
  }
}