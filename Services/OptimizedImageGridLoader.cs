using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPM.Models;
using VPM.Windows;

namespace VPM.Services
{
    /// <summary>
    /// Optimized image grid loader for packages mode.
    /// Handles efficient loading, caching, and virtualization of package images.
    /// </summary>
    public class OptimizedImageGridLoader
    {
        private readonly ImageManager _imageManager;
        private readonly Func<string, VarMetadata> _getMetadata;
        private readonly Dictionary<string, List<LazyLoadImage>> _packageImageCache = new();
        private readonly object _cacheLock = new object();

        public OptimizedImageGridLoader(ImageManager imageManager, Func<string, VarMetadata> getMetadata)
        {
            _imageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
            _getMetadata = getMetadata ?? throw new ArgumentNullException(nameof(getMetadata));
        }

        /// <summary>
        /// Loads images for a package with caching and returns lazy load image tiles
        /// </summary>
        public async Task<List<LazyLoadImage>> LoadPackageImagesAsync(PackageItem packageItem)
        {
            if (packageItem == null)
                return new List<LazyLoadImage>();

            var packageKey = !string.IsNullOrEmpty(packageItem.MetadataKey) ? packageItem.MetadataKey : packageItem.Name;

            // Check cache first
            lock (_cacheLock)
            {
                if (_packageImageCache.TryGetValue(packageKey, out var cachedImages))
                {
                    return cachedImages;
                }
            }

            try
            {
                var metadata = _getMetadata(packageKey);
                if (metadata == null)
                    return new List<LazyLoadImage>();

                var packageBase = System.IO.Path.GetFileNameWithoutExtension(metadata.Filename);
                var totalImages = _imageManager.GetCachedImageCount(packageBase);

                if (totalImages == 0)
                    return new List<LazyLoadImage>();

                // Queue for preloading
                _imageManager.QueueForPreloading(packageBase);

                // Load all images
                var images = await _imageManager.LoadImagesFromCacheAsync(packageBase, int.MaxValue);

                // Create lazy load image tiles
                var imageTiles = new List<LazyLoadImage>();
                for (int i = 0; i < images.Count; i++)
                {
                    try
                    {
                        var lazyImageTile = new LazyLoadImage
                        {
                            ImageSource = images[i],
                            PackageKey = packageKey,
                            ImageIndex = i,
                            CornerRadius = new CornerRadius(8),
                            Margin = new Thickness(3),
                            ToolTip = $"{packageItem.Name}\nDouble-click to open in image viewer"
                        };

                        imageTiles.Add(lazyImageTile);
                    }
                    catch (Exception)
                    {
                        // Skip problematic images
                    }
                }

                // Cache the result
                if (imageTiles.Count > 0)
                {
                    lock (_cacheLock)
                    {
                        _packageImageCache[packageKey] = imageTiles;
                    }
                }

                return imageTiles;
            }
            catch (Exception)
            {
                return new List<LazyLoadImage>();
            }
        }

        /// <summary>
        /// Clears the image cache
        /// </summary>
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _packageImageCache.Clear();
            }
        }

        /// <summary>
        /// Removes specific packages from cache
        /// </summary>
        public void RemoveFromCache(IEnumerable<string> packageKeys)
        {
            if (packageKeys == null)
                return;

            lock (_cacheLock)
            {
                foreach (var key in packageKeys)
                {
                    _packageImageCache.Remove(key);
                }
            }
        }
    }
}
