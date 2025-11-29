using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace VPM.Windows
{
    public class ImageWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return 200.0;

            try
            {
                double actualWidth = 0;
                if (values[0] is double d) actualWidth = d;
                else if (values[0] != null) double.TryParse(values[0].ToString(), out actualWidth);

                int desiredColumns = 3;
                if (values[1] is int i) desiredColumns = i;
                else if (values[1] != null) 
                {
                    if (double.TryParse(values[1].ToString(), out double dCols))
                        desiredColumns = (int)dCols;
                }

                // Get match width flag (values[2] if provided)
                bool matchWidth = false;
                if (values.Length > 2 && values[2] is bool b)
                {
                    matchWidth = b;
                }

                if (actualWidth <= 0 || desiredColumns <= 0)
                    return 200.0;

                // Parse parameter for margin and border values (format: "margin,border")
                // itemMargin represents the margin on each side of the Grid wrapper
                double itemMargin = 3.0;
                double borderThickness = 1.0;
                
                if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr))
                {
                    var parts = paramStr.Split(',');
                    if (parts.Length >= 1 && double.TryParse(parts[0].Trim(), out double margin))
                        itemMargin = margin;
                    if (parts.Length >= 2 && double.TryParse(parts[1].Trim(), out double border))
                        borderThickness = border;
                }

                // The Grid wrapper has Margin="3" on each item, creating spacing between items
                // Each item takes: imageWidth + 2*itemMargin (left and right margins of Grid)
                // But WrapPanel arranges items horizontally, so we need to account for:
                // - Left margin of first item: itemMargin
                // - Right margin of last item: itemMargin
                // - Gaps between items: (actualColumns - 1) * itemMargin
                // Total horizontal space = actualWidth
                // Available for images = actualWidth - (actualColumns * 2 * itemMargin) + itemMargin
                // Simplified: actualWidth - (2 * actualColumns * itemMargin - itemMargin)
                
                double scrollbarWidth = 0.0; // PanelWidthConverter returns full width, scrollbar overlays
                double minImageWidth = 100.0; // Minimum reasonable image width
                
                // Calculate available width accounting for Grid wrapper margins
                // Each Grid has Margin="3" (3px on all sides)
                // Total margin per row: left margin of first item + (gaps between items) + right margin of last item
                // = itemMargin + (actualColumns - 1) * itemMargin + itemMargin
                // = (actualColumns + 1) * itemMargin
                double totalMarginWidth = (desiredColumns + 1) * itemMargin;
                double availableRowWidth = actualWidth - totalMarginWidth;
                
                // Dynamically reduce columns if the window is too narrow
                // Each column needs: imageWidth + (2 * itemMargin) of space
                int actualColumns = desiredColumns;
                double minWidthPerColumn = minImageWidth + (2 * itemMargin);
                
                while (actualColumns > 1 && (actualColumns * minWidthPerColumn) > availableRowWidth)
                {
                    actualColumns--;
                }
                
                // Calculate the image width for the actual number of columns
                // Total space per row: availableRowWidth
                // Gaps between columns: (actualColumns - 1) * itemMargin
                // Solving: imageWidth = (availableRowWidth - gaps) / actualColumns
                
                double imageWidth;
                
                // Normal mode: use actual columns
                imageWidth = (availableRowWidth - ((actualColumns - 1) * itemMargin)) / actualColumns;
                
                // Ensure we don't return a negative width
                if (imageWidth <= 0) return minImageWidth;

                // Use Floor to ensure we don't exceed available width due to rounding
                double result = Math.Floor(Math.Max(minImageWidth, imageWidth));
                
                // Return the same value for both width and height to create square images
                return result;
            }
            catch
            {
                return 200.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
