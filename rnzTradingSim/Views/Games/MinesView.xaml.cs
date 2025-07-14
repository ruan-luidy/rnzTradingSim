using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

      // Subscribe to property changes to reset grid when game starts
      _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(_viewModel.IsGameActive))
      {
        // Reset the visual grid only when game becomes active (starts a new game)
        if (_viewModel.IsGameActive)
        {
          ResetGrid();
        }
      }
      else if (e.PropertyName == nameof(_viewModel.GameStatus))
      {
        // When game status changes to "WON" (collected winnings), reset the grid
        if (_viewModel.GameStatus == "WON")
        {
          ResetGrid();
        }
        // When game status is "LOST", reveal all mines but don't reset
        else if (_viewModel.GameStatus == "LOST")
        {
          RevealAllMines();
        }
      }
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
              RevealedContent = null,
              FallbackText = "",
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
        // Hit a mine - show red background with mine image
        buttonData.RevealedBackground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // #dc3545
        buttonData.RevealedContent = LoadImageFromResource("/Resources/Images/mine.png");
        buttonData.FallbackText = "✕"; // Fallback text if image fails to load
      }
      else
      {
        // Safe tile - show green background with gem image
        buttonData.RevealedBackground = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // #28a745
        buttonData.RevealedContent = LoadImageFromResource("/Resources/Images/gem.png");
        buttonData.FallbackText = "♦"; // Fallback text if image fails to load
      }

      // Trigger the visual update by setting Tag again (force binding update)
      button.Tag = null;
      button.Tag = buttonData;
    }

    private ImageSource LoadImageFromResource(string resourcePath)
    {
      try
      {
        // Primeira tentativa: URI relativo
        var uri = new System.Uri(resourcePath, System.UriKind.Relative);
        return new BitmapImage(uri);
      }
      catch
      {
        try
        {
          // Segunda tentativa: URI absoluto pack
          var packUri = new System.Uri($"pack://application:,,,{resourcePath}", System.UriKind.Absolute);
          return new BitmapImage(packUri);
        }
        catch
        {
          try
          {
            // Terceira tentativa: Buscar no assembly atual
            var assemblyUri = new System.Uri($"pack://application:,,,/rnzTradingSim;component{resourcePath}", System.UriKind.Absolute);
            return new BitmapImage(assemblyUri);
          }
          catch (System.Exception ex)
          {
            // Log do erro para debug
            System.Diagnostics.Debug.WriteLine($"Failed to load image {resourcePath}: {ex.Message}");
            // Fallback to text if image is not found
            return null;
          }
        }
      }
    }

    public void ResetGrid()
    {
      foreach (var button in _mineButtons)
      {
        if (button?.Tag is MineButtonData buttonData)
        {
          buttonData.IsRevealed = false;
          buttonData.RevealedContent = null;
          buttonData.FallbackText = "";
          buttonData.RevealedBackground = new SolidColorBrush(Colors.Transparent);

          // Force visual reset
          button.Tag = null;
          button.Tag = buttonData;
        }
      }
    }

    private void RevealAllMines()
    {
      for (int row = 0; row < 5; row++)
      {
        for (int col = 0; col < 5; col++)
        {
          int buttonIndex = row * 5 + col;
          var mineButtonVM = _viewModel.MineButtons[buttonIndex];
          var button = _mineButtons[row, col];
          var buttonData = button.Tag as MineButtonData;

          // If this button is a mine and not already revealed, reveal it
          if (mineButtonVM.IsMine && !buttonData.IsRevealed)
          {
            buttonData.IsRevealed = true;
            buttonData.RevealedBackground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // #dc3545
            buttonData.RevealedContent = LoadImageFromResource("/Resources/Images/mine.png");
            buttonData.FallbackText = "✕"; // Fallback text

            // Force visual update
            button.Tag = null;
            button.Tag = buttonData;
          }
        }
      }
    }
  }

  public class MineButtonData
  {
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsRevealed { get; set; }
    public ImageSource RevealedContent { get; set; } = null;
    public string FallbackText { get; set; } = "";
    public SolidColorBrush RevealedBackground { get; set; } = new SolidColorBrush(Colors.Transparent);
  }
}