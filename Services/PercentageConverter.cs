using System;
using System.Collections.Generic;
using System.Globalization;

using System.Windows.Data;

namespace ZiraceVideoPlayer.Services
{
    public class PercentageConverter : IValueConverter
    {
        public double Percentage { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double originalWidth)
            {
                return originalWidth * Percentage;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
