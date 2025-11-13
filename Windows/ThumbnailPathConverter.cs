using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VPM
{
    /// <summary>
    /// Converts thumbnail path strings to ImageSource objects, handling empty/null paths gracefully
    /// </summary>
    public class ThumbnailPathConverter : IValueConverter
    {
        public static readonly ThumbnailPathConverter Instance = new ThumbnailPathConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string thumbnailPath && !string.IsNullOrWhiteSpace(thumbnailPath))
            {
                try
                {
                    // Check if file exists before creating BitmapImage
                    if (File.Exists(thumbnailPath))
                    {
                        return new BitmapImage(new Uri(thumbnailPath, UriKind.Absolute));
                    }
                }
                catch
                {
                    // If there's any error loading the image, return null
                    return null;
                }
            }

            // Return null for empty/null paths or non-existent files
            // This prevents the binding error when ThumbnailPath is empty
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not needed for this converter
            throw new NotImplementedException();
        }
    }
}
