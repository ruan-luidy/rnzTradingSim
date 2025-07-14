using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using HandyControl.Controls;
using rnzTradingSim.ViewModels.Games;

namespace rnzTradingSim.Views.Games
{
  public partial class MinesView : UserControl
  {
    private MinesViewModel _viewModel;
    private ToggleButton[,] _mineButtons;

    public MinesView()
    {
      InitializeComponent();
      _viewModel = new MinesViewModel();
      DataContext = _viewModel;

      InitializeMineGrid();
    }

    private void InitializeMineGrid()
    {
      _mineButtons = new ToggleButton[5, 5];
      MineGrid.Children.Clear();

      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          var button = new ToggleButton
          {
            Style = Application.Current.FindResource("ToggleButtonFlip") as Style,
            Margin = new Thickness(3),
            Tag = new { Row = row, Col = col }
          };

          int buttonIndex = row * 5 + col;
          var mineButtonVM = _viewModel.MineButtons[buttonIndex];

          // Bind to ViewModel
          button.SetBinding(ToggleButton.IsCheckedProperty,
            new System.Windows.Data.Binding("IsRevealed") { Source = mineButtonVM });

          button.Checked += (s, e) => MineButton_Checked(s, e, mineButtonVM);

          _mineButtons[row, col] = button;
          MineGrid.Children.Add(button);
        }
      }
    }

    private void MineButton_Checked(object sender, RoutedEventArgs e, MineButtonViewModel mineButtonVM)
    {
      var button = sender as ToggleButton;

      if (mineButtonVM.IsMine)
      {
        // Hit a mine - show red background with bomb symbol
        button.Background = new SolidColorBrush(Colors.DarkRed);
        button.Content = "✕"; // Using X instead of emoji
        button.Foreground = new SolidColorBrush(Colors.White);
      }
      else
      {
        // Safe tile - show green background with gem symbol
        button.Background = new SolidColorBrush(Colors.DarkGreen);
        button.Content = "♦"; // Using diamond symbol instead of emoji
        button.Foreground = new SolidColorBrush(Colors.White);
      }

      // Execute the command
      mineButtonVM.ClickCommand?.Execute(mineButtonVM);
    }
  }
}