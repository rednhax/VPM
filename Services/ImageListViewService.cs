using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using VPM.Models;
using VPM.Windows;

namespace VPM.Services
{
    /// <summary>
    /// Service to manage ImageListView control integration with ImagePreviewItem data.
    /// Provides image management, caching, filtering, sorting, and selection capabilities.
    /// </summary>
    public class ImageListViewService
    {
        private readonly Dictionary<ImagePreviewItem, object> _itemCache = 
            new Dictionary<ImagePreviewItem, object>();
        
        private readonly HashSet<object> _selectedItems = new HashSet<object>();
        private ImageListView _currentListView;

        /// <summary>
        /// Occurs when the selection changes
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Configures an ImageListView control with optimal settings
        /// </summary>
        public void ConfigureImageListView(ImageListView imageListView)
        {
            if (imageListView == null)
                return;

            _currentListView = imageListView;

            // Set default view and thumbnail size
            imageListView.ViewMode = ImageListViewMode.Thumbnails;
            imageListView.ThumbnailSize = new System.Windows.Size(120, 120);
            imageListView.ShowFileIcons = false;

            // Enable grouping if not already set
            var collectionView = CollectionViewSource.GetDefaultView(imageListView.ItemsSource);
            if (collectionView != null && collectionView.GroupDescriptions.Count == 0)
            {
                // Grouping should be configured in XAML via CollectionViewSource
            }
        }

        /// <summary>
        /// Converts a collection of ImagePreviewItem to display items for the ImageListView
        /// </summary>
        public ObservableCollection<ImagePreviewItem> PrepareItemsForDisplay(IEnumerable<ImagePreviewItem> sourceItems)
        {
            var result = new ObservableCollection<ImagePreviewItem>();
            
            if (sourceItems == null)
                return result;

            foreach (var item in sourceItems)
            {
                result.Add(item);
                CacheItem(item);
            }

            return result;
        }

        /// <summary>
        /// Caches an item for fast lookup
        /// </summary>
        private void CacheItem(ImagePreviewItem sourceItem)
        {
            if (sourceItem != null && !_itemCache.ContainsKey(sourceItem))
            {
                _itemCache[sourceItem] = sourceItem;
            }
        }

        /// <summary>
        /// Gets the original ImagePreviewItem from cache
        /// </summary>
        public ImagePreviewItem GetSourceItem(object cachedItem)
        {
            if (cachedItem is ImagePreviewItem item)
                return item;

            if (_itemCache.TryGetValue(cachedItem as ImagePreviewItem, out var cached))
                return cached as ImagePreviewItem;

            return null;
        }

        /// <summary>
        /// Filters items by package
        /// </summary>
        public ObservableCollection<ImagePreviewItem> FilterByPackage(
            IEnumerable<ImagePreviewItem> items, 
            PackageItem package)
        {
            var result = new ObservableCollection<ImagePreviewItem>();
            
            if (items == null || package == null)
                return result;

            foreach (var item in items.Where(i => i.PackageItem == package))
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Filters items by extraction status
        /// </summary>
        public ObservableCollection<ImagePreviewItem> FilterByExtractionStatus(
            IEnumerable<ImagePreviewItem> items, 
            bool isExtracted)
        {
            var result = new ObservableCollection<ImagePreviewItem>();
            
            if (items == null)
                return result;

            foreach (var item in items.Where(i => i.IsExtracted == isExtracted))
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Sorts items by package name
        /// </summary>
        public ObservableCollection<ImagePreviewItem> SortByPackageName(
            IEnumerable<ImagePreviewItem> items)
        {
            var result = new ObservableCollection<ImagePreviewItem>(
                items?.OrderBy(i => i.PackageName) ?? Enumerable.Empty<ImagePreviewItem>());
            
            return result;
        }

        /// <summary>
        /// Sorts items by internal path
        /// </summary>
        public ObservableCollection<ImagePreviewItem> SortByPath(
            IEnumerable<ImagePreviewItem> items)
        {
            var result = new ObservableCollection<ImagePreviewItem>(
                items?.OrderBy(i => i.InternalPath) ?? Enumerable.Empty<ImagePreviewItem>());
            
            return result;
        }

        /// <summary>
        /// Adds an item to the selection
        /// </summary>
        public void SelectItem(ImagePreviewItem item)
        {
            if (item != null)
            {
                _selectedItems.Add(item);
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Removes an item from the selection
        /// </summary>
        public void DeselectItem(ImagePreviewItem item)
        {
            if (item != null)
            {
                _selectedItems.Remove(item);
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Clears the selection
        /// </summary>
        public void ClearSelection()
        {
            if (_selectedItems.Count > 0)
            {
                _selectedItems.Clear();
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Gets the current selection
        /// </summary>
        public IReadOnlyCollection<ImagePreviewItem> GetSelectedItems()
        {
            return _selectedItems.Cast<ImagePreviewItem>().ToList().AsReadOnly();
        }

        /// <summary>
        /// Checks if an item is selected
        /// </summary>
        public bool IsItemSelected(ImagePreviewItem item)
        {
            return item != null && _selectedItems.Contains(item);
        }

        /// <summary>
        /// Selects all items in a collection
        /// </summary>
        public void SelectAll(IEnumerable<ImagePreviewItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                _selectedItems.Add(item);
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Gets statistics about the current items
        /// </summary>
        public ImageListViewStatistics GetStatistics(IEnumerable<ImagePreviewItem> items)
        {
            if (items == null)
                return new ImageListViewStatistics();

            var itemList = items.ToList();
            return new ImageListViewStatistics
            {
                TotalItems = itemList.Count,
                ExtractedItems = itemList.Count(i => i.IsExtracted),
                UnextractedItems = itemList.Count(i => !i.IsExtracted),
                UniquePackages = itemList.Select(i => i.PackageItem).Distinct().Count(),
                TotalSize = itemList.Sum(i => i.PackageItem?.FileSize ?? 0)
            };
        }

        /// <summary>
        /// Clears all caches
        /// </summary>
        public void ClearCache()
        {
            _itemCache.Clear();
            _selectedItems.Clear();
        }

        /// <summary>
        /// Removes a specific item from the cache
        /// </summary>
        public void RemoveFromCache(ImagePreviewItem sourceItem)
        {
            if (sourceItem != null)
            {
                _itemCache.Remove(sourceItem);
                _selectedItems.Remove(sourceItem);
            }
        }

        /// <summary>
        /// Raises the SelectionChanged event
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Statistics about ImageListView items
    /// </summary>
    public class ImageListViewStatistics
    {
        public int TotalItems { get; set; }
        public int ExtractedItems { get; set; }
        public int UnextractedItems { get; set; }
        public int UniquePackages { get; set; }
        public long TotalSize { get; set; }
    }
}
