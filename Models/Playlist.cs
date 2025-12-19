using System;
using System.Collections.Generic;
using System.Linq;

namespace VPM.Models
{
    /// <summary>
    /// Represents a playlist of packages that can be activated to load specific packages and their dependencies
    /// </summary>
    public class Playlist
    {
        /// <summary>
        /// Unique identifier for the playlist (used for identification)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name for the playlist (shown in context menu)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description for the playlist
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of package identifiers (MetadataKey format: Creator.PackageName.Version) in this playlist
        /// </summary>
        public List<string> PackageKeys { get; set; } = new List<string>();

        /// <summary>
        /// Whether this playlist is enabled and should appear in the menu
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Order in which this playlist appears in the menu (lower = higher)
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Whether to unload packages not in the playlist when this playlist is activated
        /// </summary>
        public bool UnloadOtherPackages { get; set; } = true;

        /// <summary>
        /// Timestamp when the playlist was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Timestamp when the playlist was last modified
        /// </summary>
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Creates a new empty Playlist
        /// </summary>
        public Playlist()
        {
        }

        /// <summary>
        /// Creates a new Playlist with the specified name
        /// </summary>
        public Playlist(string name)
        {
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Validates that the playlist has required fields
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        /// <summary>
        /// Adds a package to this playlist if not already present
        /// </summary>
        public bool AddPackage(string packageKey)
        {
            if (string.IsNullOrWhiteSpace(packageKey))
                return false;

            if (!PackageKeys.Any(pk => string.Equals(pk, packageKey, StringComparison.OrdinalIgnoreCase)))
            {
                PackageKeys.Add(packageKey);
                LastModifiedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a package from this playlist
        /// </summary>
        public bool RemovePackage(string packageKey)
        {
            if (string.IsNullOrWhiteSpace(packageKey))
                return false;

            var index = PackageKeys.FindIndex(pk => string.Equals(pk, packageKey, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                PackageKeys.RemoveAt(index);
                LastModifiedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        public bool ContainsPackage(string packageKey)
        {
            return PackageKeys.Any(pk => string.Equals(pk, packageKey, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Clears all packages from this playlist
        /// </summary>
        public void Clear()
        {
            PackageKeys.Clear();
            LastModifiedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Name} ({PackageKeys.Count} packages)";
        }
    }
}
