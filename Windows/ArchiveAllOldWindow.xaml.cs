using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VPM.Models;
using VPM.Services;

namespace VPM
{
    public partial class ArchiveAllOldWindow : Window
    {
        public ArchiveAllOldWindow(List<VarMetadata> oldPackages, string destinationPath)
        {
            InitializeComponent();
            Loaded += (s, e) => DarkTitleBarHelper.Apply(this);
            
            MessageTextBlock.Text = $"The following {oldPackages.Count} old version package(s) will be moved to:\n{destinationPath}\n\nDo you want to continue?";
            
            // Create display items
            var displayItems = oldPackages.Select(p => new
            {
                DisplayName = $"{p.CreatorName}.{p.PackageName}.{p.Version}",
                Version = p.Version,
                LatestVersionNumber = p.LatestVersionNumber
            }).OrderBy(p => p.DisplayName).ToList();
            
            PackageListControl.ItemsSource = displayItems;
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

