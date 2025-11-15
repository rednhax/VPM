using System;
using System.Collections.Generic;
using System.Linq;

namespace VPM.Services
{
    /// <summary>
    /// Manages incremental image grid loading to support appending new packages
    /// without redrawing the entire grid. Tracks which packages are already displayed
    /// to enable efficient incremental updates.
    /// </summary>
    public class IncrementalImageGridManager
    {
        private readonly HashSet<string> _displayedPackageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _displayedPackagesLock = new object();

        /// <summary>
        /// Gets the count of currently displayed packages
        /// </summary>
        public int DisplayedPackageCount
        {
            get
            {
                lock (_displayedPackagesLock)
                {
                    return _displayedPackageNames.Count;
                }
            }
        }

        /// <summary>
        /// Clears all tracked displayed packages (call when doing a full redraw)
        /// </summary>
        public void Clear()
        {
            lock (_displayedPackagesLock)
            {
                _displayedPackageNames.Clear();
            }
        }

        /// <summary>
        /// Checks if a package is already displayed
        /// </summary>
        public bool IsPackageDisplayed(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return false;

            lock (_displayedPackagesLock)
            {
                return _displayedPackageNames.Contains(packageName);
            }
        }

        /// <summary>
        /// Marks a package as displayed
        /// </summary>
        public void MarkPackageAsDisplayed(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return;

            lock (_displayedPackagesLock)
            {
                _displayedPackageNames.Add(packageName);
            }
        }

        /// <summary>
        /// Marks multiple packages as displayed
        /// </summary>
        public void MarkPackagesAsDisplayed(IEnumerable<string> packageNames)
        {
            if (packageNames == null)
                return;

            lock (_displayedPackagesLock)
            {
                foreach (var name in packageNames)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        _displayedPackageNames.Add(name);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of packages that need to be displayed (not already shown)
        /// </summary>
        public List<(string PackageName, int OriginalIndex)> GetPackagesToDisplay(List<string> requestedPackageNames)
        {
            if (requestedPackageNames == null || requestedPackageNames.Count == 0)
                return new List<(string, int)>();

            lock (_displayedPackagesLock)
            {
                var result = new List<(string, int)>();
                for (int i = 0; i < requestedPackageNames.Count; i++)
                {
                    var name = requestedPackageNames[i];
                    if (!string.IsNullOrEmpty(name) && !_displayedPackageNames.Contains(name))
                    {
                        result.Add((name, i));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Determines if we should do a full redraw or incremental append.
        /// Returns true if we should do a full redraw, false for incremental append.
        /// </summary>
        public bool ShouldFullRedraw(List<string> requestedPackageNames)
        {
            if (requestedPackageNames == null || requestedPackageNames.Count == 0)
                return true;

            lock (_displayedPackagesLock)
            {
                // If no packages are currently displayed, do a full redraw
                if (_displayedPackageNames.Count == 0)
                    return true;

                // If the requested set is completely different from displayed, do a full redraw
                var requestedSet = new HashSet<string>(requestedPackageNames, StringComparer.OrdinalIgnoreCase);
                
                // If all requested packages are already displayed, no redraw needed
                if (requestedSet.All(p => _displayedPackageNames.Contains(p)))
                    return false;

                // If there are packages in the displayed set that aren't in the requested set,
                // we need a full redraw (user deselected something)
                if (_displayedPackageNames.Any(p => !requestedSet.Contains(p)))
                    return true;

                // Otherwise, we can do an incremental append
                return false;
            }
        }

        /// <summary>
        /// Gets the list of packages that should be removed (were displayed but are no longer requested)
        /// </summary>
        public List<string> GetPackagesToRemove(List<string> requestedPackageNames)
        {
            if (requestedPackageNames == null)
                requestedPackageNames = new List<string>();

            lock (_displayedPackagesLock)
            {
                var requestedSet = new HashSet<string>(requestedPackageNames, StringComparer.OrdinalIgnoreCase);
                var toRemove = _displayedPackageNames
                    .Where(p => !requestedSet.Contains(p))
                    .ToList();
                
                return toRemove;
            }
        }

        /// <summary>
        /// Removes specific packages from the displayed set
        /// </summary>
        public void RemovePackages(IEnumerable<string> packageNames)
        {
            if (packageNames == null)
                return;

            lock (_displayedPackagesLock)
            {
                foreach (var name in packageNames)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        _displayedPackageNames.Remove(name);
                    }
                }
            }
        }
    }
}
