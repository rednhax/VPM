using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VPM.Models;
using VPM.Services;

namespace VPM
{
    /// <summary>
    /// Renaming functionality event handlers for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private FileRenamingService _fileRenamingService;

        /// <summary>
        /// Initialize the file renaming service
        /// </summary>
        private void InitializeRenamingService()
        {
            _fileRenamingService = new FileRenamingService();
            
            // Process any pending deletions from previous sessions
            _ = Task.Run(async () =>
            {
                try
                {
                    await _fileRenamingService.ProcessPendingDeletionsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to process pending deletions: {ex.Message}");
                }
            });
        }


        #region Scene Renaming

        /// <summary>
        /// Handles double-clicking on scene name to start editing
        /// </summary>
        private void SceneName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is Border border && border.Parent is Grid grid)
            {
                // Look for the edit border (new structure) or fallback to TextBox (old structure)
                var editBorder = grid.Children.OfType<Border>().FirstOrDefault(b => b.Name == "SceneEditBorder");
                var editTextBox = editBorder?.Child is DockPanel dockPanel 
                    ? dockPanel.Children.OfType<TextBox>().FirstOrDefault() 
                    : grid.Children.OfType<TextBox>().FirstOrDefault();
                
                if (editTextBox != null)
                {
                    // Get the SceneItem from the DataContext (binding context)
                    var sceneItem = border.DataContext as SceneItem;
                    
                    if (sceneItem != null)
                    {
                        // Store original name for cancellation
                        editTextBox.SetValue(FrameworkElement.TagProperty, new { Item = sceneItem, OriginalName = sceneItem.DisplayName });
                        
                        // Switch to edit mode
                        border.Visibility = Visibility.Collapsed;
                        if (editBorder != null)
                        {
                            editBorder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            editTextBox.Visibility = Visibility.Visible;
                        }
                        editTextBox.Focus();
                        editTextBox.SelectAll();
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles key presses in scene name edit box
        /// </summary>
        private async void SceneNameEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Enter)
                {
                    await CommitSceneRename(textBox);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CancelEdit(textBox);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles losing focus on scene name edit box - just cancel without saving
        /// </summary>
        private void SceneNameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                CancelEdit(textBox);
            }
        }

        /// <summary>
        /// Commits the scene rename operation
        /// </summary>
        private async Task CommitSceneRename(TextBox textBox)
        {
            try
            {
                var tagData = textBox.Tag;
                
                if (tagData != null)
                {
                    var sceneItem = (SceneItem)tagData.GetType().GetProperty("Item").GetValue(tagData);
                    var originalName = (string)tagData.GetType().GetProperty("OriginalName").GetValue(tagData);
                    var newName = textBox.Text.Trim();

                    if (string.IsNullOrWhiteSpace(newName) || newName == originalName)
                    {
                        CancelEdit(textBox);
                        return;
                    }

                    // Use the existing file path from the scene item
                    var originalFilePath = sceneItem.FilePath;
                    
                    if (string.IsNullOrEmpty(originalFilePath) || !File.Exists(originalFilePath))
                    {
                        MessageBox.Show($"Could not locate file for scene '{originalName}'", "Rename Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        CancelEdit(textBox);
                        return;
                    }
                    
                    // Perform the rename operation
                    var newFilePath = await _fileRenamingService.RenameSceneAsync(originalFilePath, newName);
                    
                    if (!string.IsNullOrEmpty(newFilePath))
                    {
                        // Update the scene item
                        sceneItem.Name = Path.GetFileName(newFilePath);
                        sceneItem.DisplayName = newName;
                        sceneItem.FilePath = newFilePath;
                        
                        // Update thumbnail path if it exists
                        var newThumbnailPath = Path.ChangeExtension(newFilePath, ".jpg");
                        if (!File.Exists(newThumbnailPath))
                        {
                            newThumbnailPath = Path.ChangeExtension(newFilePath, ".png");
                        }
                        if (File.Exists(newThumbnailPath))
                        {
                            sceneItem.ThumbnailPath = newThumbnailPath;
                        }
                        
                        // Force UI refresh by triggering collection change notification
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                // Force refresh of the Scenes collection
                                if (Scenes != null)
                                {
                                    var index = Scenes.IndexOf(sceneItem);
                                    if (index >= 0)
                                    {
                                        Scenes.RemoveAt(index);
                                        Scenes.Insert(index, sceneItem);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Silently handle refresh errors
                            }
                        });
                        
                        SetStatus($"Successfully renamed scene to '{newName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename scene: {ex.Message}", "Rename Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CancelEdit(textBox);
            }
        }

        #endregion

        #region Preset Renaming

        /// <summary>
        /// Handles double-clicking on preset name to start editing
        /// </summary>
        private void PresetName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is Border border && border.Parent is Grid grid)
            {
                // Look for the edit border (new structure) or fallback to TextBox (old structure)
                var editBorder = grid.Children.OfType<Border>().FirstOrDefault(b => b.Name == "PresetEditBorder");
                var editTextBox = editBorder?.Child is DockPanel dockPanel 
                    ? dockPanel.Children.OfType<TextBox>().FirstOrDefault() 
                    : grid.Children.OfType<TextBox>().FirstOrDefault();
                
                if (editTextBox != null)
                {
                    // Get the CustomAtomItem from the DataContext (binding context)
                    var presetItem = border.DataContext as CustomAtomItem;
                    
                    if (presetItem != null)
                    {
                        // Store original name for cancellation
                        editTextBox.SetValue(FrameworkElement.TagProperty, new { Item = presetItem, OriginalName = presetItem.DisplayName });
                        
                        // Switch to edit mode
                        border.Visibility = Visibility.Collapsed;
                        if (editBorder != null)
                        {
                            editBorder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            editTextBox.Visibility = Visibility.Visible;
                        }
                        editTextBox.Focus();
                        editTextBox.SelectAll();
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles key presses in preset name edit box
        /// </summary>
        private async void PresetNameEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Enter)
                {
                    await CommitPresetRename(textBox);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CancelEdit(textBox);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles losing focus on preset name edit box - just cancel without saving
        /// </summary>
        private void PresetNameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                CancelEdit(textBox);
            }
        }

        /// <summary>
        /// Commits the preset rename operation
        /// </summary>
        private async Task CommitPresetRename(TextBox textBox)
        {
            try
            {
                var tagData = textBox.Tag;
                if (tagData != null)
                {
                    var presetItem = (CustomAtomItem)tagData.GetType().GetProperty("Item").GetValue(tagData);
                    var originalName = (string)tagData.GetType().GetProperty("OriginalName").GetValue(tagData);
                    var newName = textBox.Text.Trim();
                    
                    if (string.IsNullOrWhiteSpace(newName) || newName == originalName)
                    {
                        CancelEdit(textBox);
                        return;
                    }

                    // Use the existing file path from the preset item
                    var originalFilePath = presetItem.FilePath;
                    if (string.IsNullOrEmpty(originalFilePath) || !File.Exists(originalFilePath))
                    {
                        MessageBox.Show($"Could not locate file for preset '{originalName}'", "Rename Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        CancelEdit(textBox);
                        return;
                    }
                    
                    // Perform the rename operation
                    var newFilePath = await _fileRenamingService.RenamePresetAsync(originalFilePath, newName);
                    
                    if (!string.IsNullOrEmpty(newFilePath))
                    {
                        // Update the preset item
                        presetItem.Name = Path.GetFileName(newFilePath);
                        presetItem.DisplayName = newName;
                        presetItem.FilePath = newFilePath;
                        
                        // Update thumbnail path if it exists
                        var newThumbnailPath = Path.ChangeExtension(newFilePath, ".jpg");
                        if (!File.Exists(newThumbnailPath))
                        {
                            newThumbnailPath = Path.ChangeExtension(newFilePath, ".png");
                        }
                        if (File.Exists(newThumbnailPath))
                        {
                            presetItem.ThumbnailPath = newThumbnailPath;
                        }
                        
                        // Force UI refresh by triggering collection change notification
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                // Force refresh of the CustomAtomItems collection
                                if (CustomAtomItems != null)
                                {
                                    var index = CustomAtomItems.IndexOf(presetItem);
                                    if (index >= 0)
                                    {
                                        CustomAtomItems.RemoveAt(index);
                                        CustomAtomItems.Insert(index, presetItem);
                                    }
                                }
                                
                                // Also update the original list for filtering
                                if (_originalCustomAtomItems != null)
                                {
                                    var origIndex = _originalCustomAtomItems.IndexOf(presetItem);
                                    if (origIndex >= 0)
                                    {
                                        _originalCustomAtomItems.RemoveAt(origIndex);
                                        _originalCustomAtomItems.Insert(origIndex, presetItem);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Silently handle refresh errors
                            }
                        });
                        
                        SetStatus($"Successfully renamed preset to '{newName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to rename preset: {ex.Message}", "Rename Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CancelEdit(textBox);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Handles cancel button click for preset rename
        /// </summary>
        private void PresetCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Find the TextBox in the same DockPanel
                var dockPanel = button.Parent as DockPanel;
                var textBox = dockPanel?.Children.OfType<TextBox>().FirstOrDefault();
                
                if (textBox != null)
                {
                    CancelEdit(textBox);
                }
            }
        }

        /// <summary>
        /// Handles cancel button click for scene rename
        /// </summary>
        private void SceneCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Find the TextBox in the same DockPanel
                var dockPanel = button.Parent as DockPanel;
                var textBox = dockPanel?.Children.OfType<TextBox>().FirstOrDefault();
                
                if (textBox != null)
                {
                    CancelEdit(textBox);
                }
            }
        }

        /// <summary>
        /// Cancels the edit operation and returns to display mode
        /// </summary>
        private void CancelEdit(TextBox textBox)
        {
            // Handle both old structure (Grid) and new structure (DockPanel inside Border)
            DependencyObject parent = textBox.Parent;
            
            // If parent is DockPanel, go up to Border, then to Grid
            if (parent is DockPanel dockPanel)
            {
                parent = VisualTreeHelper.GetParent(dockPanel);
            }
            
            // Now parent should be the Border containing the edit controls
            if (parent is Border editBorder && VisualTreeHelper.GetParent(editBorder) is Grid grid)
            {
                var displayBorder = grid.Children.OfType<Border>().FirstOrDefault(b => b.Name == "PresetDisplayBorder" || b.Name == "SceneDisplayBorder");
                if (displayBorder != null)
                {
                    // Reset the text to original value
                    var tagData = textBox.Tag;
                    if (tagData != null)
                    {
                        var originalName = (string)tagData.GetType().GetProperty("OriginalName")?.GetValue(tagData);
                        if (!string.IsNullOrEmpty(originalName))
                        {
                            textBox.Text = originalName;
                        }
                    }

                    // Switch back to display mode
                    editBorder.Visibility = Visibility.Collapsed;
                    displayBorder.Visibility = Visibility.Visible;
                }
            }
            // Fallback for old structure (direct Grid parent)
            else if (textBox.Parent is Grid grid2)
            {
                var displayBorder = grid2.Children.OfType<Border>().FirstOrDefault(b => b.Name == "PresetDisplayBorder" || b.Name == "SceneDisplayBorder");
                if (displayBorder != null)
                {
                    // Reset the text to original value
                    var tagData = textBox.Tag;
                    if (tagData != null)
                    {
                        var originalName = (string)tagData.GetType().GetProperty("OriginalName")?.GetValue(tagData);
                        if (!string.IsNullOrEmpty(originalName))
                        {
                            textBox.Text = originalName;
                        }
                    }

                    // Switch back to display mode
                    textBox.Visibility = Visibility.Collapsed;
                    displayBorder.Visibility = Visibility.Visible;
                }
            }
        }


        #endregion
    }
}
