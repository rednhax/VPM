using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace VPM.Windows
{
    /// <summary>
    /// Converts the ListView ActualWidth to the WrapPanel width.
    /// Does NOT subtract scrollbar width - the WrapPanel should use the full available width.
    /// The scrollbar will overlay on top without affecting layout.
    /// </summary>
    public class PanelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && width > 0)
            {
                // Return the full width - WrapPanel will use all available space
                // The vertical scrollbar will overlay without affecting the layout
                double result = Math.Max(100, width);
                return result;
            }
            return 200.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
