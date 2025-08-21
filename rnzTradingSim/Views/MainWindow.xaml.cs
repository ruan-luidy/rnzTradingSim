using System.Windows;
using rnzTradingSim.ViewModels;
using rnzTradingSim.Services;
using rnzTradingSim.Controls;

namespace rnzTradingSim
{
  public partial class MainWindow : HandyControl.Controls.Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainWindowViewModel();
      
      // Subscribe to notifications
      NotificationService.NotificationReceived += OnNotificationReceived;
    }

    private void OnNotificationReceived(string message, NotificationService.NotificationType type)
    {
      Dispatcher.Invoke(() =>
      {
        var toast = new NotificationToast();
        toast.ShowNotification(message, type);
        
        NotificationsContainer.Children.Add(toast);
        
        // Limit to 5 notifications max
        while (NotificationsContainer.Children.Count > 5)
        {
          NotificationsContainer.Children.RemoveAt(0);
        }
      });
    }

    protected override void OnClosed(EventArgs e)
    {
      NotificationService.NotificationReceived -= OnNotificationReceived;
      base.OnClosed(e);
    }
  }
}