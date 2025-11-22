using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VPM.Services;

namespace VPM.Windows
{
    /// <summary>
    /// A Border control that lazily loads images only when they become visible in the viewport.
    /// Optimized for 1:1 aspect ratio images with pre-allocated space to avoid layout thrashing.
    /// </summary>
    public class LazyLoadImage : Border
    {
        private bool _isLoaded = false;
        private bool _isLoadingInProgress = false;
        private Image _imageControl;
        private Grid _overlayGrid;
        private Button _extractButton;
        
        // Image data
        public string PackageKey { get; set; }
        public int ImageIndex { get; set; }
        public BitmapImage ImageSource { get; set; }
        
        // Image dimensions for aspect ratio calculations
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        
        // Callback for loading the actual image
        public Func<Task<BitmapImage>> LoadImageCallback { get; set; }
        
        // Extraction data
        public string VarFilePath { get; set; }
        public string InternalImagePath { get; set; }
        
        // Events
        public event EventHandler ImageLoaded;
        public event EventHandler ImageUnloaded;
        public event EventHandler<ExtractionRequestedEventArgs> ExtractionRequested;
        
        public LazyLoadImage()
        {
            // Create the image control upfront (empty, no source yet)
            // This reserves the correct space and avoids layout recalculations
            _imageControl = new Image
            {
                Stretch = Stretch.UniformToFill,
                SnapsToDevicePixels = true,
                Source = null, // Will be set when image loads
                Opacity = 0 // Start invisible for fade-in animation
            };
            
            // Create overlay grid for buttons
            _overlayGrid = new Grid();
            _overlayGrid.Children.Add(_imageControl);
            
            // Create extract button at bottom-right
            _extractButton = new Button
            {
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Color.FromArgb(140, 0, 120, 215)), // Semi-transparent blue
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 6, 6),
                Visibility = Visibility.Collapsed,
                ToolTip = "Extract files from archive",
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            
            // Style the button with rounded corners
            var buttonStyle = new System.Windows.Style(typeof(Button));
            buttonStyle.Setters.Add(new System.Windows.Setter(Button.TemplateProperty, CreateButtonTemplate()));
            _extractButton.Style = buttonStyle;
            
            _extractButton.Click += (s, e) =>
            {
                e.Handled = true;
                ExtractionRequested?.Invoke(this, new ExtractionRequestedEventArgs
                {
                    VarFilePath = this.VarFilePath,
                    InternalImagePath = this.InternalImagePath
                });
            };
            
            _overlayGrid.Children.Add(_extractButton);
            
            // Set child to overlay grid
            this.Child = _overlayGrid;
            
            // Light background visible until image loads
            this.Background = new SolidColorBrush(Color.FromArgb(15, 100, 149, 237));
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }
        
        /// <summary>
        /// Checks if the image tile is visible in the viewport and loads it if needed
        /// </summary>
        public async Task<bool> CheckAndLoadIfVisibleAsync(ScrollViewer scrollViewer, double bufferSize = 200)
        {
            if (_isLoaded || _isLoadingInProgress) return _isLoaded;
            
            try
            {
                // Get the position of this element relative to the ScrollViewer
                var transform = this.TransformToAncestor(scrollViewer);
                var position = transform.Transform(new Point(0, 0));
                
                var elementTop = position.Y;
                // Use RenderSize if ActualHeight is 0 (might happen during layout)
                var elementHeight = this.ActualHeight > 0 ? this.ActualHeight : this.RenderSize.Height;
                if (elementHeight <= 0)
                {
                    // Still no height, assume a reasonable default for grid tiles
                    elementHeight = 200; // Typical grid tile size
                }
                var elementBottom = elementTop + elementHeight;
                
                var viewportTop = scrollViewer.VerticalOffset;
                var viewportBottom = viewportTop + scrollViewer.ViewportHeight;
                
                // Add buffer zone for smoother scrolling
                var loadTop = viewportTop - bufferSize;
                var loadBottom = viewportBottom + bufferSize;
                
                // Check if element is in the load zone
                if (elementBottom >= loadTop && elementTop <= loadBottom)
                {
                    await LoadImageAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                // Element might not be in visual tree yet
            }
            
            return false;
        }
        
        /// <summary>
        /// Loads the actual image using the provided callback
        /// </summary>
        public async Task LoadImageAsync()
        {
            if (_isLoaded || _isLoadingInProgress) return;
            
            _isLoadingInProgress = true;
            
            try
            {
                BitmapImage image = null;
                
                // Use provided ImageSource if available, otherwise use callback
                if (ImageSource != null)
                {
                    image = ImageSource;
                }
                else if (LoadImageCallback != null)
                {
                    image = await LoadImageCallback();
                }
                
                if (image != null)
                {
                    // Set the Source and show immediately without animation for performance
                    await Dispatcher.InvokeAsync(() =>
                    {
                        _imageControl.Source = image;
                        _imageControl.Opacity = 1;
                        _imageControl.Visibility = Visibility.Visible;
                        _isLoaded = true;
                    });
                    
                    ImageLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception)
            {
                // Failed to load image
            }
            finally
            {
                _isLoadingInProgress = false;
            }
        }
        
        /// <summary>
        /// Unloads the image to free memory (used only when clearing entire grid)
        /// </summary>
        public void UnloadImage()
        {
            if (!_isLoaded) return;
            
            try
            {
                Dispatcher.Invoke(() =>
                {
                    _imageControl.Source = null;
                    _imageControl.Opacity = 0; // Reset for potential reload with fade-in
                    _isLoaded = false;
                });
                
                ImageUnloaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                // Ignore errors during unload
            }
        }
        
        /// <summary>
        /// Checks if this image should be unloaded based on viewport position
        /// </summary>
        public bool ShouldUnload(ScrollViewer scrollViewer, double unloadThreshold = 500)
        {
            if (!_isLoaded) return false;
            
            try
            {
                var transform = this.TransformToAncestor(scrollViewer);
                var position = transform.Transform(new Point(0, 0));
                
                var elementTop = position.Y;
                var elementBottom = elementTop + this.ActualHeight;
                
                var viewportTop = scrollViewer.VerticalOffset;
                var viewportBottom = viewportTop + scrollViewer.ViewportHeight;
                
                // Unload if element is far outside the viewport
                var unloadTop = viewportTop - unloadThreshold;
                var unloadBottom = viewportBottom + unloadThreshold;
                
                return elementBottom < unloadTop || elementTop > unloadBottom;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets whether the image is currently loaded
        /// </summary>
        public bool IsImageLoaded => _isLoaded;
        
        /// <summary>
        /// Updates the extract button state (shows button or checkmark)
        /// </summary>
        public void SetExtractionState(bool isExtracted)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Get category name
                    var category = VarContentExtractor.GetCategoryFromPath(InternalImagePath);
                    
                    // Create content with icon and text
                    var stackPanel = new StackPanel 
                    { 
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    
                    var iconBlock = new TextBlock 
                    { 
                        Margin = new Thickness(0, 0, 6, 0),
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol"),
                        FontSize = 12
                    };
                    
                    var textBlock = new TextBlock 
                    { 
                        Text = category,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12
                    };
                    
                    stackPanel.Children.Add(iconBlock);
                    stackPanel.Children.Add(textBlock);

                    if (isExtracted)
                    {
                        // Show checkmark with label
                        iconBlock.Text = "✓";
                        _extractButton.Content = stackPanel;
                        // Neutral green for extracted state (not too bright)
                        _extractButton.Background = new SolidColorBrush(Color.FromArgb(160, 60, 120, 70)); 
                        _extractButton.ToolTip = $"Files for {category} are already extracted";
                        _extractButton.IsEnabled = false;
                    }
                    else
                    {
                        // Determine icon based on category
                        string iconText = "📥"; // Default
                        if (string.Equals(category, "Hair", StringComparison.OrdinalIgnoreCase)) iconText = "✂️";
                        else if (string.Equals(category, "Clothing", StringComparison.OrdinalIgnoreCase)) iconText = "👕";
                        else if (string.Equals(category, "Skin", StringComparison.OrdinalIgnoreCase)) iconText = "🎨";
                        else if (string.Equals(category, "Appearance", StringComparison.OrdinalIgnoreCase)) iconText = "👤";
                        else if (string.Equals(category, "Scene", StringComparison.OrdinalIgnoreCase)) iconText = "🎬";
                        
                        // Show extract button with icon and label
                        iconBlock.Text = iconText; 
                        _extractButton.Content = stackPanel;
                        // Transparent gray for available state
                        _extractButton.Background = new SolidColorBrush(Color.FromArgb(120, 80, 80, 80)); 
                        _extractButton.ToolTip = $"Extract {category} files";
                        _extractButton.IsEnabled = true;
                    }
                    
                    // Update padding for a more stylish look
                    _extractButton.Padding = new Thickness(8, 5, 8, 5);
                    _extractButton.Visibility = Visibility.Visible;
                });
            }
            catch (Exception)
            {
                // Ignore errors during state update
            }
        }
        
        /// <summary>
        /// Creates a custom button template with rounded corners and hover effects
        /// </summary>
        private ControlTemplate CreateButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            
            // Main border with rounded corners
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));
            
            // Content presenter
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            
            // Hover trigger (only when enabled)
            var hoverTrigger = new MultiTrigger();
            hoverTrigger.Conditions.Add(new Condition(Button.IsMouseOverProperty, true));
            hoverTrigger.Conditions.Add(new Condition(Button.IsEnabledProperty, true));
            
            // Lighter transparent gray for hover
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, 
                new SolidColorBrush(Color.FromArgb(180, 100, 100, 100))));
            template.Triggers.Add(hoverTrigger);
            
            // Pressed trigger
            var pressedTrigger = new Trigger
            {
                Property = Button.IsPressedProperty,
                Value = true
            };
            pressedTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.8));
            template.Triggers.Add(pressedTrigger);
            
            return template;
        }
    }
    
    /// <summary>
    /// Event args for extraction requests
    /// </summary>
    public class ExtractionRequestedEventArgs : EventArgs
    {
        public string VarFilePath { get; set; }
        public string InternalImagePath { get; set; }
    }
}
