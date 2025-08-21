using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using rnzTradingSim.Services;

namespace rnzTradingSim.Controls
{
    public partial class NotificationToast : UserControl
    {
        public NotificationToast()
        {
            InitializeComponent();
        }

        public void ShowNotification(string message, NotificationService.NotificationType type)
        {
            MessageText.Text = message;

            // Set colors and icons based on type
            switch (type)
            {
                case NotificationService.NotificationType.Success:
                    IconText.Text = "✅";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
                    break;

                case NotificationService.NotificationType.Warning:
                    IconText.Text = "⚠️";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(251, 191, 36)); // Yellow
                    break;

                case NotificationService.NotificationType.Error:
                    IconText.Text = "❌";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    break;

                case NotificationService.NotificationType.RugPull:
                    IconText.Text = "🚨";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38)); // Dark red
                    ToastBorder.Background = new SolidColorBrush(Color.FromArgb(40, 220, 38, 38)); // Semi-transparent red bg
                    MessageText.Foreground = new SolidColorBrush(Color.FromRgb(252, 165, 165)); // Light red text
                    break;

                case NotificationService.NotificationType.BigTrade:
                    IconText.Text = "💰";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(168, 85, 247)); // Purple
                    break;

                default:
                    IconText.Text = "ℹ️";
                    ToastBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // Blue
                    break;
            }

            // Animate in
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slideIn = new ThicknessAnimation(
                new Thickness(50, 20, 20, 20), 
                new Thickness(20, 20, 20, 20), 
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            BeginAnimation(OpacityProperty, fadeIn);
            BeginAnimation(MarginProperty, slideIn);

            // Auto-hide after delay
            var hideDelay = type == NotificationService.NotificationType.RugPull ? 8000 : 4000;
            
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(hideDelay)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                HideNotification();
            };

            timer.Start();
        }

        private void HideNotification()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var slideOut = new ThicknessAnimation(
                new Thickness(20, 20, 20, 20),
                new Thickness(-300, 20, 20, 20),
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, e) =>
            {
                if (Parent is Panel parent)
                {
                    parent.Children.Remove(this);
                }
            };

            BeginAnimation(OpacityProperty, fadeOut);
            BeginAnimation(MarginProperty, slideOut);
        }
    }
}