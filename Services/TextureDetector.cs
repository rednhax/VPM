using System;
using System.Collections.Generic;
using System.Linq;
using SharpCompress.Archives;

namespace VPM.Services
{
    /// <summary>
    /// Fast and reliable texture detection using inverse pairing logic.
    /// 
    /// Key insight: Textures are ORPHANED images (no companion files),
    /// while previews are PAIRED images (have matching .json, .vap, etc.).
    /// 
    /// This is the inverse of PreviewImageValidator:
    /// - Preview: Image HAS a companion file → IS a preview
    /// - Texture: Image HAS NO companion file → IS a texture
    /// 
    /// Benefits:
    /// - Single archive pass (no header reading needed initially)
    /// - Simple pairing check (inverse of preview logic)
    /// - Automatically filters out all companion files
    /// - No meta.json parsing required
    /// - More robust than dimension-based filtering
    /// - ~3x faster than TextureValidator for large packages
    /// </summary>
    public static class TextureDetector
    {

        /// <summary>
        /// Detects textures in an archive by finding orphaned images (no companion files).
        /// Returns a list of texture paths that are NOT paired with other files.
        /// Uses FULL PATHS to handle cases where same filename exists in different directories.
        /// </summary>
        /// <param name="archive">The archive to scan</param>
        /// <returns>List of texture file paths (internal archive paths)</returns>
        public static List<string> DetectTexturesInArchive(IArchive archive, bool enableDebug = false)
        {
            if (archive == null)
                return new List<string>();

            var textures = new List<string>();

            try
            {
                // Step 1: Build a list of ALL files in the archive (using full paths)
                // This allows us to check for companion files in the SAME directory
                var allEntries = SharpCompressHelper.GetAllEntries(archive);
                var allFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in allEntries)
                {
                    if (!entry.Key.EndsWith("/"))
                    {
                        allFilePaths.Add(entry.Key.ToLower());
                    }
                }

                // Step 2: Find all image files
                var imageEntries = new List<IArchiveEntry>();
                foreach (var entry in allEntries)
                {
                    if (entry.Key.EndsWith("/")) continue;

                    var ext = System.IO.Path.GetExtension(entry.Key);
                    if (TextureUtils.IsImageExtension(ext))
                    {
                        imageEntries.Add(entry);
                    }
                }

                if (enableDebug)
                {
                    System.Diagnostics.Debug.WriteLine($"\n=== TEXTURE DETECTION DEBUG ===");
                    System.Diagnostics.Debug.WriteLine($"Total files in archive: {allFilePaths.Count}");
                    System.Diagnostics.Debug.WriteLine($"Total image files: {imageEntries.Count}");
                    
                    // Group files by directory to understand pairing
                    var filesByDir = allFilePaths.GroupBy(p => System.IO.Path.GetDirectoryName(p) ?? "").ToList();
                    foreach (var dirGroup in filesByDir)
                    {
                        var nonImageFiles = dirGroup.Where(p => !TextureUtils.IsImageExtension(System.IO.Path.GetExtension(p))).ToList();
                        var imageFiles = dirGroup.Where(p => TextureUtils.IsImageExtension(System.IO.Path.GetExtension(p))).ToList();
                        
                        if (imageFiles.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"\nDirectory: {dirGroup.Key}");
                            System.Diagnostics.Debug.WriteLine($"  Images ({imageFiles.Count}):");
                            foreach (var img in imageFiles.Take(10)) // Limit output
                            {
                                System.Diagnostics.Debug.WriteLine($"    - {System.IO.Path.GetFileName(img)}");
                            }
                            if (imageFiles.Count > 10) System.Diagnostics.Debug.WriteLine($"    ... and {imageFiles.Count - 10} more");
                            
                            System.Diagnostics.Debug.WriteLine($"  Non-images ({nonImageFiles.Count}):");
                            foreach (var f in nonImageFiles.Take(10))
                            {
                                System.Diagnostics.Debug.WriteLine($"    - {System.IO.Path.GetFileName(f)}");
                            }
                            if (nonImageFiles.Count > 10) System.Diagnostics.Debug.WriteLine($"    ... and {nonImageFiles.Count - 10} more");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("");
                }

                // Step 3: For each image, check if it's ORPHANED (no companion file in SAME directory)
                // This is the INVERSE of preview detection
                foreach (var entry in imageEntries)
                {
                    bool isOrphaned = TextureUtils.IsOrphanedImage(entry.Key, allFilePaths, enableDebug);
                    if (isOrphaned)
                    {
                        textures.Add(entry.Key);
                    }
                    
                    if (enableDebug)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {System.IO.Path.GetFileName(entry.Key)}: {(isOrphaned ? "TEXTURE (orphaned)" : "PREVIEW (paired)")}");
                    }
                }
                
                if (enableDebug)
                {
                    System.Diagnostics.Debug.WriteLine($"Total textures detected: {textures.Count}");
                }
            }
            catch (Exception ex)
            {
                if (enableDebug)
                {
                    System.Diagnostics.Debug.WriteLine($"TEXTURE DETECTION ERROR: {ex.Message}");
                }
            }

            return textures;
        }

        /// <summary>
        /// Determines if an image is a TEXTURE (orphaned, no companion file).
        /// Delegates to TextureUtils.IsOrphanedImage for shared implementation.
        /// </summary>
        public static bool IsOrphanedImage(IArchiveEntry imageEntry, IEnumerable<string> allFilePaths, bool enableDebug = false)
            => imageEntry != null && TextureUtils.IsOrphanedImage(imageEntry.Key, allFilePaths, enableDebug);

        /// <summary>
        /// String-based overload for checking if an image path is orphaned.
        /// Delegates to TextureUtils.IsOrphanedImage for shared implementation.
        /// </summary>
        public static bool IsOrphanedImagePath(string imagePath, IEnumerable<string> allFilePaths)
            => TextureUtils.IsOrphanedImage(imagePath, allFilePaths, false);

        /// <summary>
        /// Overload: Detects textures from a VAR file path.
        /// Opens the archive, detects textures, and closes it.
        /// </summary>
        public static List<string> DetectTexturesInVarFile(string varPath, bool enableDebug = false)
        {
            if (string.IsNullOrEmpty(varPath) || !System.IO.File.Exists(varPath))
                return new List<string>();

            try
            {
                if (enableDebug)
                {
                    System.Diagnostics.Debug.WriteLine($"\n=== DETECTING TEXTURES IN: {System.IO.Path.GetFileName(varPath)} ===");
                }
                
                using (var archive = SharpCompressHelper.OpenForRead(varPath))
                {
                    return DetectTexturesInArchive(archive.Archive, enableDebug);
                }
            }
            catch (Exception ex)
            {
                if (enableDebug)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR detecting textures: {ex.Message}");
                }
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets detailed texture information for a list of detected textures.
        /// Includes resolution, file size, and dimensions.
        /// </summary>
        public class TextureInfo
        {
            public string InternalPath { get; set; }
            public long FileSize { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Resolution { get; set; }
            public string TextureType { get; set; }

            public string FileSizeFormatted => TextureUtils.FormatFileSize(FileSize);

            public string DimensionsFormatted => TextureUtils.FormatDimensions(Width, Height);
        }

        /// <summary>
        /// Enriches detected textures with dimension and type information.
        /// Uses header-only reading for 95-99% memory reduction.
        /// </summary>
        public static List<TextureInfo> GetTextureInfoList(IArchive archive, List<string> texturePaths)
        {
            var textureInfos = new List<TextureInfo>();

            if (archive == null || texturePaths == null || texturePaths.Count == 0)
                return textureInfos;

            try
            {
                foreach (var texturePath in texturePaths)
                {
                    try
                    {
                        var entry = SharpCompressHelper.FindEntryByPath(archive, texturePath);
                        if (entry == null) continue;

                        // Get dimensions using header-only read (95-99% memory reduction)
                        var (width, height) = SharpCompressHelper.GetImageDimensionsFromEntry(archive, entry);

                        // Skip if we couldn't read dimensions
                        if (width <= 0 || height <= 0)
                            continue;

                        // Determine resolution classification
                        string resolution = TextureUtils.GetResolutionLabel(width, height);

                        // Determine texture type from suffix
                        string textureType = TextureUtils.GetTextureType(texturePath);

                        textureInfos.Add(new TextureInfo
                        {
                            InternalPath = texturePath,
                            FileSize = entry.Size,
                            Width = width,
                            Height = height,
                            Resolution = resolution,
                            TextureType = textureType
                        });
                    }
                    catch
                    {
                        // Skip textures that fail to process
                    }
                }
            }
            catch
            {
                // Silently fail
            }

            return textureInfos;
        }

        /// <summary>
        /// Generates a summary of detected textures.
        /// </summary>
        public class TextureSummary
        {
            public int TotalTexturesFound { get; set; }
            public long TotalTextureSize { get; set; }
            public Dictionary<string, int> TexturesByResolution { get; set; } = new();
            public Dictionary<string, int> TexturesByType { get; set; } = new();

            public string TotalSizeFormatted => TotalTextureSize > 0
                ? TextureUtils.FormatFileSize(TotalTextureSize)
                : "0 MB";
        }

        /// <summary>
        /// Generates a summary of texture information.
        /// </summary>
        public static TextureSummary GetTextureSummary(List<TextureInfo> textureInfos)
        {
            var summary = new TextureSummary
            {
                TotalTexturesFound = textureInfos.Count,
                TotalTextureSize = textureInfos.Sum(t => t.FileSize)
            };

            foreach (var texture in textureInfos)
            {
                // Count by resolution
                if (!summary.TexturesByResolution.ContainsKey(texture.Resolution))
                    summary.TexturesByResolution[texture.Resolution] = 0;
                summary.TexturesByResolution[texture.Resolution]++;

                // Count by type
                if (!summary.TexturesByType.ContainsKey(texture.TextureType))
                    summary.TexturesByType[texture.TextureType] = 0;
                summary.TexturesByType[texture.TextureType]++;
            }

            return summary;
        }
    }
}
