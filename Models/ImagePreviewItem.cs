using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace VPM.Models
{
    public class ImagePreviewItem : INotifyPropertyChanged
    {
        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _packageName;
        public string PackageName
        {
            get => _packageName;
            set
            {
                if (_packageName != value)
                {
                    _packageName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _internalPath;
        public string InternalPath
        {
            get => _internalPath;
            set
            {
                if (_internalPath != value)
                {
                    _internalPath = value;
                    OnPropertyChanged();
                    UpdateExtractionButtonState();
                }
            }
        }

        private Brush _statusBrush;
        public Brush StatusBrush
        {
            get => _statusBrush;
            set
            {
                if (_statusBrush != value)
                {
                    _statusBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        private PackageItem _packageItem;
        public PackageItem PackageItem
        {
            get => _packageItem;
            set
            {
                if (_packageItem != value)
                {
                    _packageItem = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isExtracted;
        public bool IsExtracted
        {
            get => _isExtracted;
            set
            {
                if (_isExtracted != value)
                {
                    _isExtracted = value;
                    OnPropertyChanged();
                    UpdateExtractionButtonState();
                }
            }
        }

        private object _extractionButtonContent;
        public object ExtractionButtonContent
        {
            get => _extractionButtonContent;
            set
            {
                if (_extractionButtonContent != value)
                {
                    _extractionButtonContent = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _extractionButtonBackground;
        public Brush ExtractionButtonBackground
        {
            get => _extractionButtonBackground;
            set
            {
                if (_extractionButtonBackground != value)
                {
                    _extractionButtonBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateExtractionButtonState()
        {
            // Get category name
            var category = GetCategoryFromPath(InternalPath);
            
            // Create content with icon and text
            var stackPanel = new System.Windows.Controls.StackPanel 
            { 
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            
            var iconBlock = new System.Windows.Controls.TextBlock 
            { 
                Margin = new System.Windows.Thickness(0, 0, 6, 0),
                FontWeight = System.Windows.FontWeights.Bold,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol"),
                FontSize = 12
            };
            
            var textBlock = new System.Windows.Controls.TextBlock 
            { 
                Text = category,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontWeight = System.Windows.FontWeights.SemiBold,
                FontSize = 12
            };
            
            stackPanel.Children.Add(iconBlock);
            stackPanel.Children.Add(textBlock);

            if (IsExtracted)
            {
                // Show checkmark with label
                iconBlock.Text = "✓";
                ExtractionButtonContent = stackPanel;
                // Neutral green for extracted state (not too bright)
                ExtractionButtonBackground = new SolidColorBrush(Color.FromArgb(160, 60, 120, 70)); 
            }
            else
            {
                // Determine icon based on category
                string iconText = "📥"; // Default
                if (string.Equals(category, "Hair", System.StringComparison.OrdinalIgnoreCase)) iconText = "✂️";
                else if (string.Equals(category, "Clothing", System.StringComparison.OrdinalIgnoreCase)) iconText = "👕";
                else if (string.Equals(category, "Skin", System.StringComparison.OrdinalIgnoreCase)) iconText = "🎨";
                else if (string.Equals(category, "Appearance", System.StringComparison.OrdinalIgnoreCase)) iconText = "👤";
                else if (string.Equals(category, "Scene", System.StringComparison.OrdinalIgnoreCase)) iconText = "🎬";
                
                // Show extract button with icon and label
                iconBlock.Text = iconText; 
                ExtractionButtonContent = stackPanel;
                // Transparent gray for available state
                ExtractionButtonBackground = new SolidColorBrush(Color.FromArgb(120, 80, 80, 80)); 
            }
        }

        private string GetCategoryFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Unknown";
            
            var parts = path.Split(new[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (string.Equals(part, "Hair", System.StringComparison.OrdinalIgnoreCase)) return "Hair";
                if (string.Equals(part, "Clothing", System.StringComparison.OrdinalIgnoreCase)) return "Clothing";
                if (string.Equals(part, "Skins", System.StringComparison.OrdinalIgnoreCase) || string.Equals(part, "Skin", System.StringComparison.OrdinalIgnoreCase)) return "Skin";
                if (string.Equals(part, "Looks", System.StringComparison.OrdinalIgnoreCase) || string.Equals(part, "Appearance", System.StringComparison.OrdinalIgnoreCase)) return "Appearance";
                if (string.Equals(part, "Saves", System.StringComparison.OrdinalIgnoreCase) || string.Equals(part, "Scene", System.StringComparison.OrdinalIgnoreCase)) return "Scene";
            }
            
            // Fallback to second folder if available (usually Custom/Category/...)
            if (parts.Length > 1 && string.Equals(parts[0], "Custom", System.StringComparison.OrdinalIgnoreCase))
                return parts[1];
                
            return "Content";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
