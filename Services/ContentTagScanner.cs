using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SharpCompress.Archives;
using VPM.Models;

namespace VPM.Services
{
    /// <summary>
    /// Scans VAR packages to extract clothing and hair tags from .vam files.
    /// Tags are stored in the "tags" field of .vam JSON files.
    /// </summary>
    public class ContentTagScanner
    {
        /// <summary>
        /// Result of scanning a VAR for content tags
        /// </summary>
        public class TagScanResult
        {
            /// <summary>
            /// All unique clothing tags found in the package (lowercase, comma-separated per item combined)
            /// </summary>
            public HashSet<string> ClothingTags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            
            /// <summary>
            /// All unique hair tags found in the package (lowercase, comma-separated per item combined)
            /// </summary>
            public HashSet<string> HairTags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            
            /// <summary>
            /// Number of clothing items scanned
            /// </summary>
            public int ClothingItemsScanned { get; set; }
            
            /// <summary>
            /// Number of hair items scanned
            /// </summary>
            public int HairItemsScanned { get; set; }
            
            /// <summary>
            /// Number of items with tags
            /// </summary>
            public int ItemsWithTags { get; set; }
        }

        /// <summary>
        /// Scans a VAR archive for clothing and hair tags
        /// </summary>
        /// <param name="archive">Open archive to scan</param>
        /// <param name="entries">List of file entries to check</param>
        /// <returns>Tag scan result with all found tags</returns>
        public TagScanResult ScanForTags(IArchive archive, IEnumerable<string> entries)
        {
            var result = new TagScanResult();
            
            foreach (var entryPath in entries)
            {
                if (string.IsNullOrEmpty(entryPath))
                    continue;
                
                // Check if this is a clothing or hair .vam file
                bool isClothing = entryPath.Contains("/Custom/Clothing/", StringComparison.OrdinalIgnoreCase) &&
                                  entryPath.EndsWith(".vam", StringComparison.OrdinalIgnoreCase);
                bool isHair = entryPath.Contains("/Custom/Hair/", StringComparison.OrdinalIgnoreCase) &&
                              entryPath.EndsWith(".vam", StringComparison.OrdinalIgnoreCase);
                
                if (!isClothing && !isHair)
                    continue;
                
                if (isClothing) result.ClothingItemsScanned++;
                if (isHair) result.HairItemsScanned++;
                
                try
                {
                    var entry = SharpCompressHelper.FindEntryByPath(archive, entryPath);
                    if (entry == null)
                        continue;
                    
                    var jsonContent = SharpCompressHelper.ReadEntryAsString(archive, entry);
                    if (string.IsNullOrEmpty(jsonContent))
                        continue;
                    
                    var tags = ExtractTagsFromVamJson(jsonContent);
                    if (tags.Count > 0)
                    {
                        result.ItemsWithTags++;
                        
                        foreach (var tag in tags)
                        {
                            if (isClothing)
                                result.ClothingTags.Add(tag);
                            else if (isHair)
                                result.HairTags.Add(tag);
                        }
                    }
                }
                catch
                {
                    // Skip files that can't be read
                }
            }
            
            return result;
        }

        /// <summary>
        /// Scans a VAR file path for clothing and hair tags
        /// </summary>
        /// <param name="varPath">Path to the VAR file</param>
        /// <returns>Tag scan result with all found tags</returns>
        public TagScanResult ScanVarForTags(string varPath)
        {
            var result = new TagScanResult();
            
            try
            {
                using var archive = SharpCompressHelper.OpenForRead(varPath);
                var entries = SharpCompressHelper.GetAllEntries(archive.Archive);
                
                foreach (var entry in entries)
                {
                    if (entry.IsDirectory)
                        continue;
                    
                    var entryPath = entry.Key;
                    
                    // Check if this is a clothing or hair .vam file
                    bool isClothing = entryPath.Contains("/Custom/Clothing/", StringComparison.OrdinalIgnoreCase) &&
                                      entryPath.EndsWith(".vam", StringComparison.OrdinalIgnoreCase);
                    bool isHair = entryPath.Contains("/Custom/Hair/", StringComparison.OrdinalIgnoreCase) &&
                                  entryPath.EndsWith(".vam", StringComparison.OrdinalIgnoreCase);
                    
                    if (!isClothing && !isHair)
                        continue;
                    
                    if (isClothing) result.ClothingItemsScanned++;
                    if (isHair) result.HairItemsScanned++;
                    
                    try
                    {
                        var jsonContent = SharpCompressHelper.ReadEntryAsString(archive.Archive, entry);
                        if (string.IsNullOrEmpty(jsonContent))
                            continue;
                        
                        var tags = ExtractTagsFromVamJson(jsonContent);
                        if (tags.Count > 0)
                        {
                            result.ItemsWithTags++;
                            
                            foreach (var tag in tags)
                            {
                                if (isClothing)
                                    result.ClothingTags.Add(tag);
                                else if (isHair)
                                    result.HairTags.Add(tag);
                            }
                        }
                    }
                    catch
                    {
                        // Skip files that can't be read
                    }
                }
            }
            catch
            {
                // Return empty result on error
            }
            
            return result;
        }

        /// <summary>
        /// Extracts tags from a .vam JSON file content
        /// </summary>
        /// <param name="jsonContent">JSON content of the .vam file</param>
        /// <returns>List of tags found (lowercase, trimmed)</returns>
        public List<string> ExtractTagsFromVamJson(string jsonContent)
        {
            var tags = new List<string>();
            
            if (string.IsNullOrEmpty(jsonContent))
                return tags;
            
            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("tags", out var tagsElement))
                {
                    string tagString = null;
                    
                    if (tagsElement.ValueKind == JsonValueKind.String)
                    {
                        tagString = tagsElement.GetString();
                    }
                    else if (tagsElement.ValueKind == JsonValueKind.Array)
                    {
                        // Some .vam files might have tags as an array
                        var tagList = new List<string>();
                        foreach (var item in tagsElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                var t = item.GetString()?.Trim().ToLowerInvariant();
                                if (!string.IsNullOrEmpty(t))
                                    tagList.Add(t);
                            }
                        }
                        return tagList;
                    }
                    
                    if (!string.IsNullOrEmpty(tagString))
                    {
                        // Tags are comma-separated in VaM
                        var splits = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var split in splits)
                        {
                            var tag = split.Trim().ToLowerInvariant();
                            if (!string.IsNullOrEmpty(tag))
                            {
                                tags.Add(tag);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on parse error
            }
            
            return tags;
        }

        /// <summary>
        /// Checks if a set of tags matches a filter (any tag in filter matches any tag in item)
        /// </summary>
        /// <param name="itemTags">Tags on the item</param>
        /// <param name="filterTags">Tags to filter by</param>
        /// <returns>True if any filter tag matches any item tag</returns>
        public static bool MatchesTagFilter(IEnumerable<string> itemTags, IEnumerable<string> filterTags)
        {
            if (itemTags == null || filterTags == null)
                return false;
            
            var itemTagSet = new HashSet<string>(itemTags, StringComparer.OrdinalIgnoreCase);
            
            foreach (var filterTag in filterTags)
            {
                if (itemTagSet.Contains(filterTag))
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a set of tags matches ALL filter tags (AND logic)
        /// </summary>
        /// <param name="itemTags">Tags on the item</param>
        /// <param name="filterTags">Tags to filter by</param>
        /// <returns>True if all filter tags are present in item tags</returns>
        public static bool MatchesAllTagFilters(IEnumerable<string> itemTags, IEnumerable<string> filterTags)
        {
            if (itemTags == null)
                return false;
            if (filterTags == null)
                return true;
            
            var itemTagSet = new HashSet<string>(itemTags, StringComparer.OrdinalIgnoreCase);
            
            foreach (var filterTag in filterTags)
            {
                if (!itemTagSet.Contains(filterTag))
                    return false;
            }
            
            return true;
        }
    }
}
