﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DebugTool.Converters;

public class BooleanToBrushConverter : IValueConverter
{
  public Brush TrueBrush { get; set; } = Brushes.Green;
  public Brush FalseBrush { get; set; } = Brushes.Red;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is bool boolValue)
    {
      return boolValue ? TrueBrush : FalseBrush;
    }
    return FalseBrush;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}