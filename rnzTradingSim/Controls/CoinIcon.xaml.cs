using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace rnzTradingSim.Controls
{
    public partial class CoinIcon : UserControl
    {
        public static readonly DependencyProperty CoinSymbolProperty =
            DependencyProperty.Register("CoinSymbol", typeof(string), typeof(CoinIcon), 
                new PropertyMetadata(string.Empty, OnCoinSymbolChanged));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(CoinIcon), 
                new PropertyMetadata(null, OnIconSourceChanged));

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(double), typeof(CoinIcon), 
                new PropertyMetadata(60.0, OnIconSizeChanged));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(CoinIcon), 
                new PropertyMetadata(16.0));

        public string CoinSymbol
        {
            get { return (string)GetValue(CoinSymbolProperty); }
            set { SetValue(CoinSymbolProperty, value); }
        }

        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public CoinIcon()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private static void OnCoinSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CoinIcon control)
            {
                control.UpdateDisplay();
            }
        }

        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CoinIcon control)
            {
                control.UpdateDisplay();
            }
        }

        private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CoinIcon control)
            {
                control.UpdateFontSize();
            }
        }

        private void UpdateDisplay()
        {
            if (IconSource != null)
            {
                // Show image icon
                CoinImage.Visibility = Visibility.Visible;
                CoinText.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Show text fallback
                CoinImage.Visibility = Visibility.Collapsed;
                CoinText.Visibility = Visibility.Visible;
                
                // Use first 2 characters of symbol
                var symbol = CoinSymbol ?? "";
                CoinText.Text = symbol.Length >= 2 ? symbol.Substring(0, 2).ToUpper() : symbol.ToUpper();
            }
        }

        private void UpdateFontSize()
        {
            // Auto-scale font size based on icon size
            FontSize = IconSize * 0.3; // 30% of icon size
        }
    }
}