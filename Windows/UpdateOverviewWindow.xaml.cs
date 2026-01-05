using System;
using System.Diagnostics;
using System.Windows;
using VPM.Services;

namespace VPM.Windows
{
    public partial class UpdateOverviewWindow : Window
    {
        public string VpmReleaseUrl { get; set; }
        public string VpbDownloadUrl { get; set; }

        public UpdateOverviewWindow()
        {
            InitializeComponent();
            DarkTitleBarHelper.Apply(this);
        }

        public void SetVpmStatus(AppUpdateChecker.AppUpdateInfo info)
        {
            if (info.IsUpdateAvailable)
            {
                VpmStatusText.Text = "Update Available";
                VpmStatusText.Foreground = System.Windows.Media.Brushes.Green;
                VpmVersionText.Text = $"{info.CurrentVersion} ➜ {info.LatestVersion}";
                VpmVersionText.Visibility = Visibility.Visible;
                VpmUpdateButton.Visibility = Visibility.Visible;
                VpmReleaseUrl = info.ReleaseUrl;
            }
            else
            {
                VpmStatusText.Text = "Up to Date";
                VpmStatusText.Foreground = System.Windows.Media.Brushes.Gray;
                VpmVersionText.Text = $"Version: {info.CurrentVersion}";
                VpmVersionText.Visibility = Visibility.Visible;
                VpmUpdateButton.Visibility = Visibility.Collapsed;
            }
        }

        public void SetVpbStatus(VpbPluginCheckResult info)
        {
            VpbDetailsText.Text = ""; // Reset details
            
            if (info.IsUpdateAvailable)
            {
                VpbStatusText.Text = "Version Mismatch";
                VpbStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                
                string details = "";
                if (!string.IsNullOrEmpty(info.LocalVersion))
                    details += $"Local: v{info.LocalVersion}  ";
                
                if (info.RemoteLastModified.HasValue)
                    details += $"GitHub: {info.RemoteLastModified.Value.ToLocalTime():yyyy-MM-dd}";
                
                VpbDetailsText.Text = details.Trim();
                VpbDetailsText.Visibility = Visibility.Visible;
                
                VpbUpdateButton.Visibility = Visibility.Visible;
                VpbUpdateButton.Content = "Sync"; // "Sync" implies matching remote state, up or down
                VpbDownloadUrl = info.DownloadUrl;
            }
            else if (!info.IsInstalled)
            {
                VpbStatusText.Text = "Not Installed";
                VpbStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                VpbUpdateButton.Content = "Install";
                VpbUpdateButton.Visibility = Visibility.Visible;
                VpbDownloadUrl = info.DownloadUrl;
                VpbDetailsText.Visibility = Visibility.Collapsed;
            }
            else
            {
                VpbStatusText.Text = "Up to Date";
                VpbStatusText.Foreground = System.Windows.Media.Brushes.Gray;
                VpbUpdateButton.Visibility = Visibility.Collapsed;
                
                string details = "";
                if (!string.IsNullOrEmpty(info.LocalVersion))
                    details += $"Local: v{info.LocalVersion}";
                VpbDetailsText.Text = details;
                VpbDetailsText.Visibility = string.IsNullOrEmpty(details) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void VpmUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VpmReleaseUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = VpmReleaseUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void VpbUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                // Close the overview window first so we don't have stacked update windows
                Close();
                mainWindow.OpenVpbPatcher();
            }
            else if (!string.IsNullOrEmpty(VpbDownloadUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = VpbDownloadUrl, 
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
