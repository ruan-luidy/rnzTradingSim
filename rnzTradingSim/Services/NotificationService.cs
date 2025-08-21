using System.Windows;
using rnzTradingSim.Models;

namespace rnzTradingSim.Services
{
  public static class NotificationService
  {
    public static event Action<string, NotificationType>? NotificationReceived;

    public enum NotificationType
    {
      Info,
      Warning,
      Success,
      RugPull,
      BigTrade,
      Error
    }

    public static void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
      try
      {
        // Dispatch to UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
          NotificationReceived?.Invoke(message, type);
        });

        // Log the notification
        var logLevel = type switch
        {
          NotificationType.Error => "Error",
          NotificationType.Warning => "Warning",
          NotificationType.RugPull => "Warning",
          _ => "Info"
        };

        LoggingService.Info($"[{logLevel}] {message}");
      }
      catch (Exception ex)
      {
        LoggingService.Error("Error showing notification", ex);
      }
    }

    public static void NotifyRugPull(string coinSymbol, string coinName, decimal crashPercent, decimal amountLost)
    {
      var message = $"🚨 RUG PULL DETECTED! {coinName} ({coinSymbol}) crashed {crashPercent:N1}%! ${amountLost:N0} stolen from liquidity!";
      ShowNotification(message, NotificationType.RugPull);
      
      // Also log as warning for debugging
      LoggingService.Warning($"RUG PULL: {coinSymbol} - Price Impact: {crashPercent:N2}% - Amount: ${amountLost:N2}");
    }

    public static void NotifyBigTrade(string tradeType, string coinSymbol, decimal amount, decimal value)
    {
      var emoji = tradeType == "BUY" ? "🟢" : "🔴";
      var action = tradeType == "BUY" ? "bought" : "sold";
      var message = $"{emoji} Big {action}! {amount:N0} {coinSymbol} for ${value:N0}";
      ShowNotification(message, NotificationType.BigTrade);
    }

    public static void NotifyTradingSuccess(string message)
    {
      ShowNotification($"✅ {message}", NotificationType.Success);
    }

    public static void NotifyTradingError(string message)
    {
      ShowNotification($"❌ {message}", NotificationType.Error);
    }

    public static void NotifyPriceAlert(string coinSymbol, decimal newPrice, decimal changePercent)
    {
      var emoji = changePercent > 0 ? "📈" : "📉";
      var direction = changePercent > 0 ? "up" : "down";
      var message = $"{emoji} {coinSymbol} is {direction} {Math.Abs(changePercent):N1}%! Now ${newPrice:N4}";
      ShowNotification(message, NotificationType.Info);
    }
  }
}