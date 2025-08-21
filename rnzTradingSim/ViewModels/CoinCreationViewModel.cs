using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using rnzTradingSim.Services;

namespace rnzTradingSim.ViewModels
{
  public partial class CoinCreationViewModel : ObservableObject
  {
    private readonly CoinCreationService _coinCreationService;
    private readonly PlayerService _playerService;
    private string? _selectedIconPath;

    [ObservableProperty]
    private string coinName = string.Empty;

    [ObservableProperty]
    private string coinSymbol = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal totalSupply = 1_000_000;

    [ObservableProperty]
    private decimal initialLiquidity = 100;

    [ObservableProperty]
    private BitmapImage? coinIconSource;

    [ObservableProperty]
    private bool isCreating = false;

    public CoinCreationViewModel()
    {
      _coinCreationService = new CoinCreationService(new Data.TradingDbContext(), new PlayerService());
      _playerService = new PlayerService();
    }

    // Validation properties
    public string NameValidationMessage => GetNameValidation();
    public string SymbolValidationMessage => GetSymbolValidation();
    
    // Computed properties
    public string TotalCostText => $"Total Cost: ${CoinCreationService.COIN_CREATION_COST + InitialLiquidity:N2}";
    public string InitialPriceText => TotalSupply > 0 ? $"${(InitialLiquidity / (TotalSupply * 0.9m)):F8}" : "$0.00000000";
    public string MarketCapText => TotalSupply > 0 ? $"${(TotalSupply * 0.1m * (InitialLiquidity / (TotalSupply * 0.9m))):N2}" : "$0.00";

    public bool CanCreateCoin => 
      !IsCreating &&
      !string.IsNullOrWhiteSpace(CoinName) &&
      !string.IsNullOrWhiteSpace(CoinSymbol) &&
      !string.IsNullOrWhiteSpace(Description) &&
      string.IsNullOrEmpty(NameValidationMessage) &&
      string.IsNullOrEmpty(SymbolValidationMessage) &&
      HasSufficientFunds();

    private string GetNameValidation()
    {
      if (string.IsNullOrWhiteSpace(CoinName)) return string.Empty;
      
      if (CoinName.Length < 2) return "Name must be at least 2 characters";
      if (CoinName.Length > 100) return "Name must be less than 100 characters";
      
      return string.Empty;
    }

    private string GetSymbolValidation()
    {
      if (string.IsNullOrWhiteSpace(CoinSymbol)) return string.Empty;
      
      if (CoinSymbol.Length < 2) return "Symbol must be at least 2 characters";
      if (CoinSymbol.Length > 10) return "Symbol must be less than 10 characters";
      if (!System.Text.RegularExpressions.Regex.IsMatch(CoinSymbol, @"^[A-Z0-9]+$"))
        return "Symbol can only contain letters and numbers";
      
      return string.Empty;
    }

    private bool HasSufficientFunds()
    {
      try
      {
        var player = _playerService.GetCurrentPlayer();
        return player.Balance >= (CoinCreationService.COIN_CREATION_COST + InitialLiquidity);
      }
      catch
      {
        return false;
      }
    }

    [RelayCommand]
    private async Task SelectIcon()
    {
      var openFileDialog = new OpenFileDialog
      {
        Title = "Select Coin Icon",
        Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp",
        FilterIndex = 1,
        RestoreDirectory = true
      };

      if (openFileDialog.ShowDialog() == true)
      {
        var filePath = openFileDialog.FileName;
        var fileInfo = new FileInfo(filePath);

        // Check file size (max 2MB)
        if (fileInfo.Length > 2 * 1024 * 1024)
        {
          NotificationService.NotifyTradingError("Image file too large. Maximum size is 2MB.");
          return;
        }

        try
        {
          // Load and display the image
          var bitmap = new BitmapImage();
          bitmap.BeginInit();
          bitmap.UriSource = new Uri(filePath);
          bitmap.CacheOption = BitmapCacheOption.OnLoad;
          bitmap.DecodePixelWidth = 100; // Resize for performance
          bitmap.EndInit();
          bitmap.Freeze();

          CoinIconSource = bitmap;
          _selectedIconPath = filePath;

          NotificationService.NotifyTradingSuccess("Icon uploaded successfully!");
        }
        catch (Exception ex)
        {
          NotificationService.NotifyTradingError($"Failed to load image: {ex.Message}");
        }
      }
    }

    [RelayCommand]
    private async Task CreateCoin()
    {
      if (!CanCreateCoin) return;

      IsCreating = true;
      
      try
      {
        // Copy icon to app directory if selected
        string? iconPath = null;
        if (!string.IsNullOrEmpty(_selectedIconPath))
        {
          iconPath = await CopyIconToAppDirectory(_selectedIconPath, CoinSymbol);
        }

        var result = await _coinCreationService.CreateCoinAsync(
          CoinName.Trim(),
          CoinSymbol.Trim().ToUpper(),
          Description.Trim(),
          TotalSupply,
          InitialLiquidity
        );

        if (result.success)
        {
          // If we have an icon, update the coin with the icon path
          if (result.coin != null && !string.IsNullOrEmpty(iconPath))
          {
            result.coin.ImageUrl = iconPath;
            // Save the updated coin (you might need to add this to CoinCreationService)
          }

          NotificationService.NotifyTradingSuccess($"🚀 {CoinSymbol} created successfully!");
          NotificationService.ShowNotification($"💰 New coin launched: {CoinName} ({CoinSymbol})!", NotificationService.NotificationType.Success);
          
          // Clear form
          ClearForm();
        }
        else
        {
          NotificationService.NotifyTradingError(result.message);
        }
      }
      catch (Exception ex)
      {
        NotificationService.NotifyTradingError($"Failed to create coin: {ex.Message}");
        LoggingService.Error("Error creating coin", ex);
      }
      finally
      {
        IsCreating = false;
      }
    }

    [RelayCommand]
    private void ClearForm()
    {
      CoinName = string.Empty;
      CoinSymbol = string.Empty;
      Description = string.Empty;
      TotalSupply = 1_000_000;
      InitialLiquidity = 100;
      CoinIconSource = null;
      _selectedIconPath = null;
    }

    private async Task<string?> CopyIconToAppDirectory(string sourcePath, string coinSymbol)
    {
      try
      {
        var appDataPath = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
          "RnzTradingSim",
          "CoinIcons"
        );

        Directory.CreateDirectory(appDataPath);

        var extension = Path.GetExtension(sourcePath);
        var fileName = $"{coinSymbol}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
        var destPath = Path.Combine(appDataPath, fileName);

        File.Copy(sourcePath, destPath, true);
        
        return destPath;
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error copying icon", ex);
        return null;
      }
    }

    // Property change notifications for computed properties
    partial void OnTotalSupplyChanged(decimal value) => OnPropertyChanged(nameof(InitialPriceText));
    partial void OnInitialLiquidityChanged(decimal value)
    {
      OnPropertyChanged(nameof(TotalCostText));
      OnPropertyChanged(nameof(InitialPriceText));
      OnPropertyChanged(nameof(MarketCapText));
      OnPropertyChanged(nameof(CanCreateCoin));
    }
    partial void OnCoinNameChanged(string value)
    {
      OnPropertyChanged(nameof(NameValidationMessage));
      OnPropertyChanged(nameof(CanCreateCoin));
    }
    partial void OnCoinSymbolChanged(string value)
    {
      OnPropertyChanged(nameof(SymbolValidationMessage));
      OnPropertyChanged(nameof(CanCreateCoin));
    }
    partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(CanCreateCoin));
    partial void OnIsCreatingChanged(bool value) => OnPropertyChanged(nameof(CanCreateCoin));
  }
}