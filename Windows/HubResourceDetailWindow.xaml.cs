using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VPM.Models;
using VPM.Services;

namespace VPM.Windows
{
    /// <summary>
    /// Detail view for a Hub resource with download functionality
    /// </summary>
    public partial class HubResourceDetailWindow : Window
    {
        private readonly HubResourceDetail _resource;
        private readonly HubService _hubService;
        private readonly string _destinationFolder;
        private List<HubFileViewModel> _files;

        public HubResourceDetailWindow(HubResourceDetail resource, HubService hubService, string destinationFolder)
        {
            InitializeComponent();

            _resource = resource;
            _hubService = hubService;
            _destinationFolder = destinationFolder;

            LoadResourceDetails();
        }

        private void LoadResourceDetails()
        {
            // Set header info
            TitleText.Text = _resource.Title ?? "Unknown";
            CreatorText.Text = $"by {_resource.Creator ?? "Unknown"}";
            TagLineText.Text = _resource.TagLine ?? "";
            TypeText.Text = _resource.Type ?? "Unknown";
            var category = _resource.Category ?? "Free";
            CategoryText.Text = category switch
            {
                "Free" => "🎁 Free",
                "Paid" => "💰 Paid",
                _ => category
            };
            DownloadCountRun.Text = _resource.DownloadCount.ToString("N0");
            RatingRun.Text = _resource.RatingAvg.ToString("F1");

            // Load thumbnail
            if (!string.IsNullOrEmpty(_resource.ImageUrl))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_resource.ImageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ThumbnailImage.Source = bitmap;
                }
                catch
                {
                    // Ignore thumbnail loading errors
                }
            }

            // Build files list
            _files = new List<HubFileViewModel>();

            if (_resource.HubFiles != null)
            {
                foreach (var file in _resource.HubFiles)
                {
                    _files.Add(CreateFileViewModel(file, isDependency: false));
                }
            }

            // Add dependencies
            if (_resource.Dependencies != null)
            {
                foreach (var depGroup in _resource.Dependencies)
                {
                    foreach (var depFile in depGroup.Value)
                    {
                        _files.Add(CreateFileViewModel(depFile, isDependency: true));
                    }
                }
            }

            FilesItemsControl.ItemsSource = _files;
            UpdateDownloadAllButton();
        }

        private HubFileViewModel CreateFileViewModel(HubFile file, bool isDependency)
        {
            var vm = new HubFileViewModel
            {
                Filename = file.Filename ?? "Unknown",
                FileSize = file.FileSize,
                FileSizeFormatted = HubService.FormatFileSize(file.FileSize),
                LicenseType = file.LicenseType ?? "",
                DownloadUrl = file.EffectiveDownloadUrl,
                LatestUrl = file.LatestUrl,
                IsDependency = isDependency,
                NotOnHub = string.IsNullOrEmpty(file.EffectiveDownloadUrl) || file.EffectiveDownloadUrl == "null"
            };

            // Check if already in library
            var packageName = file.PackageName;
            var localPath = Path.Combine(_destinationFolder, file.Filename ?? "");
            vm.AlreadyHave = File.Exists(localPath);

            vm.CanDownload = !vm.AlreadyHave && !vm.NotOnHub;

            return vm;
        }

        private void UpdateDownloadAllButton()
        {
            var downloadableCount = _files?.Count(f => f.CanDownload) ?? 0;
            DownloadAllButton.IsEnabled = downloadableCount > 0;
            DownloadAllButton.Content = downloadableCount > 0 
                ? $"⬇ Download All ({downloadableCount})" 
                : "⬇ Download All";
        }

        private async void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is HubFileViewModel file)
            {
                await DownloadFileAsync(file);
            }
        }

        private async void DownloadAll_Click(object sender, RoutedEventArgs e)
        {
            var filesToDownload = _files.Where(f => f.CanDownload).ToList();
            
            foreach (var file in filesToDownload)
            {
                await DownloadFileAsync(file);
            }
        }

        private async Task DownloadFileAsync(HubFileViewModel file)
        {
            if (!file.CanDownload)
                return;

            try
            {
                file.IsDownloading = true;
                StatusText.Text = $"Downloading {file.Filename}...";

                var progress = new Progress<HubDownloadProgress>(p =>
                {
                    file.Progress = p.Progress;
                    StatusText.Text = $"Downloading {file.Filename}: {p.Progress:P0}";
                });

                var success = await _hubService.DownloadPackageAsync(
                    file.DownloadUrl,
                    _destinationFolder,
                    file.Filename.Replace(".var", ""),
                    progress);

                if (success)
                {
                    file.AlreadyHave = true;
                    file.CanDownload = false;
                    file.IsDownloading = false;
                    StatusText.Text = $"Downloaded {file.Filename}";
                }
                else
                {
                    file.IsDownloading = false;
                    StatusText.Text = $"Failed to download {file.Filename}";
                }

                // Refresh the list
                FilesItemsControl.Items.Refresh();
                UpdateDownloadAllButton();
            }
            catch (Exception ex)
            {
                file.IsDownloading = false;
                StatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Failed to download {file.Filename}:\n\n{ex.Message}", "Download Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
