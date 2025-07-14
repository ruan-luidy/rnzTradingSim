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
    private bool[,] _mineField;
    private bool _gameStarted = false;
    private int _mineCount = 3;
    private Random _random = new Random();

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
      _mineField = new bool[5, 5];

      MineGrid.Children.Clear();

      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          var button = new ToggleButton
          {
            Style = Application.Current.FindResource("ToggleButtonFlip") as Style,
            Margin = new System.Windows.Thickness(3),
            Tag = new { Row = row, Col = col }
          };

          button.Checked += MineButton_Checked;
          button.Unchecked += MineButton_Unchecked;

          _mineButtons[row, col] = button;
          MineGrid.Children.Add(button);
        }
      }
    }

    private void MineButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
      if (!_gameStarted) return;

      var button = sender as ToggleButton;
      var position = (dynamic)button.Tag;
      int row = position.Row;
      int col = position.Col;

      // Check if it's a mine
      if (_mineField[row, col])
      {
        // Hit a mine - show red background
        button.Background = new SolidColorBrush(Colors.Red);
        button.Content = "💣";
        GameOver(false);
      }
      else
      {
        // Safe tile - show green background
        button.Background = new SolidColorBrush(Colors.Green);
        button.Content = "💎";
        CheckWinCondition();
      }
    }

    private void MineButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
      if (!_gameStarted) return;

      var button = sender as ToggleButton;
      // Reset to default state
      button.Background = new SolidColorBrush(Color.FromRgb(58, 58, 58));
      button.Content = null;
    }

    private void StartGame()
    {
      _gameStarted = true;
      PlaceMines();
      ResetAllButtons();
    }

    private void PlaceMines()
    {
      // Clear previous mines
      for (int i = 0; i < 5; i++)
      {
        for (int j = 0; j < 5; j++)
        {
          _mineField[i, j] = false;
        }
      }

      // Place new mines randomly
      int minesPlaced = 0;
      while (minesPlaced < _mineCount)
      {
        int row = _random.Next(5);
        int col = _random.Next(5);

        if (!_mineField[row, col])
        {
          _mineField[row, col] = true;
          minesPlaced++;
        }
      }
    }

    private void ResetAllButtons()
    {
      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          var button = _mineButtons[row, col];
          button.IsChecked = false;
          button.Background = new SolidColorBrush(Color.FromRgb(58, 58, 58));
          button.Content = null;
          button.IsEnabled = true;
        }
      }
    }

    private void CheckWinCondition()
    {
      int safeTilesRevealed = 0;
      int totalSafeTiles = 25 - _mineCount;

      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          var button = _mineButtons[row, col];
          if (button.IsChecked == true && !_mineField[row, col])
          {
            safeTilesRevealed++;
          }
        }
      }

      if (safeTilesRevealed >= totalSafeTiles)
      {
        GameOver(true);
      }
    }

    private void GameOver(bool won)
    {
      _gameStarted = false;

      // Disable all buttons
      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          _mineButtons[row, col].IsEnabled = false;

          // Reveal all mines if game lost
          if (!won && _mineField[row, col])
          {
            _mineButtons[row, col].Background = new SolidColorBrush(Colors.Red);
            _mineButtons[row, col].Content = "💣";
          }
        }
      }

      // Show result message
      string message = won ? "Parabéns! Você ganhou!" : "Game Over! Você perdeu!";
      HandyControl.Controls.MessageBox.Show(message, "Mines",
          System.Windows.MessageBoxButton.OK,
          won ? System.Windows.MessageBoxImage.Information : System.Windows.MessageBoxImage.Warning);
    }

    // Event handler for Start Game button (to be connected in XAML)
    private void StartGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      StartGame();
    }
  }
}