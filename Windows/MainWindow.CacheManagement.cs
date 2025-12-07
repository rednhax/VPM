using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VPM.Services;

namespace VPM
{
    public partial class MainWindow
    {
        /// <summary>
        /// Opens the cache folder in Windows Explorer
        /// Contains PackageMetadata.cache, PackageImages.cache, and HubResources.cache
        /// </summary>
        private void OpenCacheFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cacheDir = _packageManager.GetCacheDirectory();
                
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = cacheDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening cache folder: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clears all metadata cache and reloads packages
        /// </summary>
        private void ClearAllMetadataCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Clear all metadata cache? This will force all packages to be re-parsed on next load.\n\nThe application will need to be restarted for changes to take effect.",
                    "Clear Metadata Cache",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    _packageManager.ClearMetadataCache();
                    MessageBox.Show(
                        "Metadata cache cleared successfully.\n\nPlease restart the application to reload packages with fresh metadata.",
                        "Cache Cleared",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing metadata cache: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Clears the Hub resources cache (packages.json binary cache)
        /// Forces a fresh download from Hub on next access
        /// </summary>
        private void ClearHubCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Clear Hub resources cache? This will force a fresh download of the Hub packages index on next access.\n\nThis is useful if you're experiencing issues with package updates or Hub browsing.",
                    "Clear Hub Cache",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    // Create a temporary HubService to access the cache
                    using var hubService = new HubService();
                    var cleared = hubService.ClearResourcesCache();
                    
                    if (cleared)
                    {
                        MessageBox.Show(
                            "Hub resources cache cleared successfully.\n\nThe Hub packages index will be downloaded fresh on next access.",
                            "Cache Cleared",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Hub cache was already empty or could not be cleared.",
                            "Cache Status",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing Hub cache: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Gets Hub cache statistics for display
        /// </summary>
        private string GetHubCacheStatistics()
        {
            try
            {
                using var hubService = new HubService();
                var stats = hubService.GetCacheStatistics();
                
                if (stats == null)
                    return "Hub cache: Not initialized";
                
                return $"Hub cache: {stats.PackageCount:N0} packages, {stats.CacheSizeFormatted}, " +
                       $"{stats.ConditionalHitRate:F0}% conditional hits";
            }
            catch
            {
                return "Hub cache: Unable to read statistics";
            }
        }
    }
}

