using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VPM.Windows
{
    /// <summary>
    /// Enhanced WPF ListView for displaying images with optimized performance and grouping support.
    /// Inspired by the oozcitak/imagelistview library but implemented for WPF.
    /// </summary>
    public class ImageListView : ListView
    {
        public ImageListView()
        {
            // Disable horizontal scrolling, enable vertical
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            
            // Enable virtualization for better performance with large image collections
            VirtualizingStackPanel.SetIsVirtualizing(this, true);
            VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
        }

        /// <summary>
        /// Dependency property for thumbnail size
        /// </summary>
        public static readonly DependencyProperty ThumbnailSizeProperty =
            DependencyProperty.Register(
                "ThumbnailSize",
                typeof(Size),
                typeof(ImageListView),
                new PropertyMetadata(new Size(120, 120)));

        public Size ThumbnailSize
        {
            get { return (Size)GetValue(ThumbnailSizeProperty); }
            set { SetValue(ThumbnailSizeProperty, value); }
        }

        /// <summary>
        /// Dependency property for view mode (Thumbnails, Details, etc.)
        /// </summary>
        public static readonly DependencyProperty ViewModeProperty =
            DependencyProperty.Register(
                "ViewMode",
                typeof(ImageListViewMode),
                typeof(ImageListView),
                new PropertyMetadata(ImageListViewMode.Thumbnails));

        public ImageListViewMode ViewMode
        {
            get { return (ImageListViewMode)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        /// <summary>
        /// Dependency property for showing file icons
        /// </summary>
        public static readonly DependencyProperty ShowFileIconsProperty =
            DependencyProperty.Register(
                "ShowFileIcons",
                typeof(bool),
                typeof(ImageListView),
                new PropertyMetadata(false));

        public bool ShowFileIcons
        {
            get { return (bool)GetValue(ShowFileIconsProperty); }
            set { SetValue(ShowFileIconsProperty, value); }
        }

        /// <summary>
        /// Scrolls to the top of the list
        /// </summary>
        public void ScrollToTop()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            scrollViewer?.ScrollToTop();
        }

        /// <summary>
        /// Scrolls to the bottom of the list
        /// </summary>
        public void ScrollToBottom()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
            }
        }

        /// <summary>
        /// Helper method to find a visual child by type
        /// </summary>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Enumeration for ImageListView display modes
    /// </summary>
    public enum ImageListViewMode
    {
        Thumbnails,
        Gallery,
        Pane,
        Details,
        HorizontalStrip,
        VerticalStrip
    }
}
