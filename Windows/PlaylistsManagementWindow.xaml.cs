using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VPM.Models;
using VPM.Services;

namespace VPM.Windows
{
    public partial class PlaylistsManagementWindow : Window
    {
        private readonly ISettingsManager _settingsManager;
        private readonly PackageManager _packageManager;
        private readonly DependencyGraph _dependencyGraph;
        private readonly string _vamRootFolder;
        private readonly PackageFileManager _packageFileManager;
        private PlaylistManager _playlistManager;
        private ObservableCollection<Playlist> _playlists;
        private ICollectionView _playlistsView;
        private Playlist _currentPlaylist;
        private bool _hasUnsavedChanges = false;

        public PlaylistsManagementWindow(ISettingsManager settingsManager, PackageManager packageManager = null, 
            DependencyGraph dependencyGraph = null, string vamRootFolder = null, PackageFileManager packageFileManager = null)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _packageManager = packageManager;
            _dependencyGraph = dependencyGraph;
            _vamRootFolder = vamRootFolder;
            _packageFileManager = packageFileManager;
            
            if (packageManager != null && dependencyGraph != null && vamRootFolder != null)
            {
                _playlistManager = new PlaylistManager(packageManager, dependencyGraph, vamRootFolder, packageFileManager);
            }
            
            InitializeComponent();
            LoadPlaylists();
            _hasUnsavedChanges = false;
            UpdateUIState();
        }

        private void LoadPlaylists()
        {
            var playlists = _settingsManager?.Settings?.Playlists ?? new List<Playlist>();
            _playlists = new ObservableCollection<Playlist>(playlists.OrderBy(p => p.SortOrder));
            _playlistsView = CollectionViewSource.GetDefaultView(_playlists);
            _playlistsView.Filter = FilterPlaylists;
            PlaylistsListBox.ItemsSource = _playlistsView;
        }

        private bool FilterPlaylists(object item)
        {
            if (string.IsNullOrWhiteSpace(PlaylistSearchBox.Text)) return true;
            var playlist = item as Playlist;
            return playlist != null && playlist.Name.IndexOf(PlaylistSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void PlaylistSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _playlistsView?.Refresh();
        }

        private void PlaylistsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _currentPlaylist = PlaylistsListBox.SelectedItem as Playlist;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            if (_currentPlaylist != null)
            {
                PlaylistNameTextBox.IsEnabled = true;
                PlaylistDescriptionTextBox.IsEnabled = true;
                UnloadOthersCheckBox.IsEnabled = true;
                DeletePlaylistBtn.IsEnabled = true;
                ActivatePlaylistBtn.IsEnabled = true;

                // Unsubscribe from change events to prevent marking as unsaved during UI update
                PlaylistNameTextBox.TextChanged -= PlaylistName_Changed;
                PlaylistDescriptionTextBox.TextChanged -= PlaylistDescription_Changed;
                UnloadOthersCheckBox.Checked -= UnloadOthers_Changed;
                UnloadOthersCheckBox.Unchecked -= UnloadOthers_Changed;

                // Only update text if it's different to avoid cursor jumping or loops
                if (PlaylistNameTextBox.Text != _currentPlaylist.Name)
                    PlaylistNameTextBox.Text = _currentPlaylist.Name;
                    
                if (PlaylistDescriptionTextBox.Text != _currentPlaylist.Description)
                    PlaylistDescriptionTextBox.Text = _currentPlaylist.Description;
                    
                UnloadOthersCheckBox.IsChecked = _currentPlaylist.UnloadOtherPackages;

                // Resubscribe to change events
                PlaylistNameTextBox.TextChanged += PlaylistName_Changed;
                PlaylistDescriptionTextBox.TextChanged += PlaylistDescription_Changed;
                UnloadOthersCheckBox.Checked += UnloadOthers_Changed;
                UnloadOthersCheckBox.Unchecked += UnloadOthers_Changed;
                
                RefreshPackagesList();
                UpdateMoveButtons();
            }
            else
            {
                ClearDetails();
                UpdateMoveButtons();
            }
        }

        private void RefreshPackagesList()
        {
            if (_currentPlaylist != null)
            {
                var packages = new ObservableCollection<string>(_currentPlaylist.PackageKeys);
                PlaylistPackagesListBox.ItemsSource = packages;
                PackageCountTextBlock.Text = $"{packages.Count} items";
            }
        }

        private void ClearDetails()
        {
            PlaylistNameTextBox.IsEnabled = false;
            PlaylistDescriptionTextBox.IsEnabled = false;
            UnloadOthersCheckBox.IsEnabled = false;
            DeletePlaylistBtn.IsEnabled = false;
            ActivatePlaylistBtn.IsEnabled = false;

            PlaylistNameTextBox.Text = "";
            PlaylistDescriptionTextBox.Text = "";
            UnloadOthersCheckBox.IsChecked = false;
            PlaylistPackagesListBox.ItemsSource = null;
            PackageCountTextBlock.Text = "0 items";
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var newPlaylist = new Playlist
            {
                Name = $"Playlist {_playlists.Count + 1}",
                SortOrder = _playlists.Count
            };

            _playlists.Add(newPlaylist);
            _hasUnsavedChanges = true;
            UpdateSaveButton();
            
            // Clear search to show the new playlist
            if (!string.IsNullOrEmpty(PlaylistSearchBox.Text))
            {
                PlaylistSearchBox.Text = "";
            }
            
            PlaylistsListBox.SelectedItem = newPlaylist;
            PlaylistsListBox.ScrollIntoView(newPlaylist);
        }

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist == null)
                return;

            var result = DarkMessageBox.Show(
                $"Are you sure you want to delete the playlist \"{_currentPlaylist.Name}\"?",
                "Delete Playlist",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _playlists.Remove(_currentPlaylist);
            _hasUnsavedChanges = true;
            UpdateSaveButton();
            // Selection change will handle clearing details
        }

        private void PlaylistName_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_currentPlaylist != null && _currentPlaylist.Name != PlaylistNameTextBox.Text)
            {
                _currentPlaylist.Name = PlaylistNameTextBox.Text;
                _hasUnsavedChanges = true;
                UpdateSaveButton();
                
                // Refresh list to show new name
                _playlistsView.Refresh();
            }
        }

        private void PlaylistDescription_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_currentPlaylist != null && _currentPlaylist.Description != PlaylistDescriptionTextBox.Text)
            {
                _currentPlaylist.Description = PlaylistDescriptionTextBox.Text;
                _hasUnsavedChanges = true;
                UpdateSaveButton();
            }
        }

        private void UnloadOthers_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist != null)
            {
                _currentPlaylist.UnloadOtherPackages = UnloadOthersCheckBox.IsChecked ?? true;
                _hasUnsavedChanges = true;
                UpdateSaveButton();
            }
        }

        private async void ActivatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist == null)
                return;

            var result = DarkMessageBox.Show(
                $"Activate playlist \"{_currentPlaylist.Name}\"?\n\n" +
                $"This will load {_currentPlaylist.PackageKeys.Count} packages and their dependencies." +
                (_currentPlaylist.UnloadOtherPackages ? "\n\nPackages not in this playlist will be unloaded." : ""),
                "Activate Playlist",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            ActivatePlaylistBtn.IsEnabled = false;
            try
            {
                await ActivatePlaylistAsync(_currentPlaylist);
                DarkMessageBox.Show("Playlist activated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Error activating playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ActivatePlaylistBtn.IsEnabled = true;
            }
        }

        private async System.Threading.Tasks.Task ActivatePlaylistAsync(Playlist playlist)
        {
            if (_playlistManager == null)
            {
                throw new InvalidOperationException("PlaylistManager is not initialized. Cannot activate playlist without package manager access.");
            }

            var result = await _playlistManager.ActivatePlaylistAsync(playlist, playlist.UnloadOtherPackages);
            
            if (!result.Success)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsManager != null)
            {
                _settingsManager.Settings.Playlists = _playlists.ToList();
                _settingsManager.SaveSettingsImmediate();
            }
            _hasUnsavedChanges = false;
            UpdateSaveButton();
        }

        private void UpdateSaveButton()
        {
            SavePlaylistBtn.IsEnabled = _hasUnsavedChanges;
            UpdateMoveButtons();
        }

        private void UpdateMoveButtons()
        {
            if (_currentPlaylist == null || _playlists.Count <= 1)
            {
                MoveUpPlaylistBtn.IsEnabled = false;
                MoveDownPlaylistBtn.IsEnabled = false;
                return;
            }

            var index = _playlists.IndexOf(_currentPlaylist);
            MoveUpPlaylistBtn.IsEnabled = index > 0;
            MoveDownPlaylistBtn.IsEnabled = index < _playlists.Count - 1;
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist == null)
                return;

            var index = _playlists.IndexOf(_currentPlaylist);
            if (index <= 0)
                return;

            _playlists.Move(index, index - 1);
            for (int i = 0; i < _playlists.Count; i++)
            {
                _playlists[i].SortOrder = i;
            }

            _hasUnsavedChanges = true;
            UpdateSaveButton();
            PlaylistsListBox.SelectedItem = _currentPlaylist;
            PlaylistsListBox.ScrollIntoView(_currentPlaylist);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist == null)
                return;

            var index = _playlists.IndexOf(_currentPlaylist);
            if (index >= _playlists.Count - 1)
                return;

            _playlists.Move(index, index + 1);
            for (int i = 0; i < _playlists.Count; i++)
            {
                _playlists[i].SortOrder = i;
            }

            _hasUnsavedChanges = true;
            UpdateSaveButton();
            PlaylistsListBox.SelectedItem = _currentPlaylist;
            PlaylistsListBox.ScrollIntoView(_currentPlaylist);
        }

        private void RemovePackage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is string packageKey && _currentPlaylist != null)
            {
                _currentPlaylist.PackageKeys.Remove(packageKey);
                _hasUnsavedChanges = true;
                UpdateSaveButton();
                RefreshPackagesList();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = DarkMessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SavePlaylist_Click(null, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Close();
        }
    }
}
