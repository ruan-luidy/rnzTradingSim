using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using rnzTradingSim.ViewModels.Games;

namespace rnzTradingSim.Views.Games
{
  public partial class MinesView : UserControl
  {
    private MinesViewModel _viewModel;
    private Button[,] _mineButtons;

    public MinesView()
    {
      InitializeComponent();
      _viewModel = new MinesViewModel();
      DataContext = _viewModel;

      InitializeMineGrid();
    }

    private void InitializeMineGrid()
    {
      _mineButtons = new Button[5, 5];
      MineGrid.Children.Clear();

      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          var button = new Button
          {
            Style = Application.Current.FindResource("MineButtonStyle") as Style,
            Tag = new MineButtonData
            {
              Row = row,
              Col = col,
              IsRevealed = false,
              RevealedContent = "",
              RevealedBackground = new SolidColorBrush(Colors.Transparent)
            }
          };

          int buttonIndex = row * 5 + col;
          var mineButtonVM = _viewModel.MineButtons[buttonIndex];

          button.Click += (s, e) => MineButton_Click(s, e, mineButtonVM);

          _mineButtons[row, col] = button;
          MineGrid.Children.Add(button);
        }
      }
    }

    private void MineButton_Click(object sender, RoutedEventArgs e, MineButtonViewModel mineButtonVM)
    {
      var button = sender as Button;
      var buttonData = button.Tag as MineButtonData;

      if (buttonData.IsRevealed || !_viewModel.IsGameActive)
        return;

      // Execute the command first to update ViewModel
      mineButtonVM.ClickCommand?.Execute(mineButtonVM);

      // Update visual state
      buttonData.IsRevealed = true;

      if (mineButtonVM.IsMine)
      {
        // Hit a mine - show red background with X symbol
        buttonData.RevealedBackground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // #dc3545
        buttonData.RevealedContent = "✕";
      }
      else
      {
        // Safe tile - show green background with diamond symbol
        buttonData.RevealedBackground = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // #28a745
        buttonData.RevealedContent = "♦";
      }

      // Trigger the visual update by setting Tag again (force binding update)
      button.Tag = null;
      button.Tag = buttonData;
    }

    public void ResetGrid()
    {
      foreach (var button in _mineButtons)
      {
        if (button?.Tag is MineButtonData buttonData)
        {
          buttonData.IsRevealed = false;
          buttonData.RevealedContent = "";
          buttonData.RevealedBackground = new SolidColorBrush(Colors.Transparent);

          // Force visual reset
          button.Tag = null;
          button.Tag = buttonData;
        }
      }
    }
  }

  public class MineButtonData
  {
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsRevealed { get; set; }
    public string RevealedContent { get; set; } = "";
    public SolidColorBrush RevealedBackground { get; set; } = new SolidColorBrush(Colors.Transparent);
  }
}