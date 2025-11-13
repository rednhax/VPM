using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VPM
{
    /// <summary>
    /// Converts DataGridHeadersVisibility values to Visibility enum values
    /// </summary>
    public class DataGridHeadersVisibilityConverter : IValueConverter
    {
        public static readonly DataGridHeadersVisibilityConverter Instance = new DataGridHeadersVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridHeadersVisibility headersVisibility)
            {
                return headersVisibility == DataGridHeadersVisibility.None ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None;
            }
            return DataGridHeadersVisibility.Column;
        }
    }
}
