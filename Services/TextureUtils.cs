using System;
using System.IO;

namespace VPM.Services
{
    /// <summary>
    /// Shared utility methods for texture processing.
    /// Consolidates duplicated logic from TextureDetector, TextureValidator, and TextureConverter.
    /// </summary>
    public static class TextureUtils
    {
        /// <summary>
        /// Image extensions considered as textures
        /// </summary>
        public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png" };

        /// <summary>
        /// Checks if a file extension is an image type
        /// </summary>
        public static bool IsImageExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;
            var ext = extension.ToLowerInvariant();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
        }

        /// <summary>
        /// Gets a resolution label (8K, 4K, 2K, 1K, or Npx) from max dimension
        /// </summary>
        public static string GetResolutionLabel(int maxDimension) => maxDimension switch
        {
            >= 7680 => "8K",
            >= 4096 => "4K",
            >= 2048 => "2K",
            >= 1024 => "1K",
            _ => $"{maxDimension}px"
        };

        /// <summary>
        /// Gets a resolution label from width and height
        /// </summary>
        public static string GetResolutionLabel(int width, int height)
            => GetResolutionLabel(Math.Max(width, height));

        /// <summary>
        /// Gets the texture type based on filename suffix (_D, _S, _G, _N, _A)
        /// </summary>
        public static string GetTextureType(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return "Texture";
            
            var name = Path.GetFileNameWithoutExtension(filename);
            if (string.IsNullOrEmpty(name) || name.Length < 2) return "Texture";

            // Check for standard texture suffixes
            if (name.EndsWith("_D", StringComparison.OrdinalIgnoreCase)) return "Diffuse";
            if (name.EndsWith("_S", StringComparison.OrdinalIgnoreCase)) return "Specular";
            if (name.EndsWith("_G", StringComparison.OrdinalIgnoreCase)) return "Gloss";
            if (name.EndsWith("_N", StringComparison.OrdinalIgnoreCase)) return "Normal";
            if (name.EndsWith("_A", StringComparison.OrdinalIgnoreCase)) return "Alpha";
            
            return "Texture";
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable MB string
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "-";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }

        /// <summary>
        /// Formats dimensions as "WxH" string
        /// </summary>
        public static string FormatDimensions(int width, int height)
        {
            if (width <= 0 || height <= 0) return "-";
            return $"{width}×{height}";
        }

        /// <summary>
        /// Gets the target dimension for a resolution label
        /// </summary>
        public static int GetTargetDimension(string resolution) => resolution switch
        {
            "8K" => 7680,
            "4K" => 4096,
            "2K" => 2048,
            "1K" => 1024,
            _ => 0
        };

        /// <summary>
        /// Checks if an image path is orphaned (no companion files with same stem in same directory).
        /// An orphaned image is considered a texture, while a paired image is a preview.
        /// </summary>
        /// <param name="imagePath">Full path to the image file</param>
        /// <param name="allFilePaths">All file paths in the archive/folder (lowercase)</param>
        /// <param name="enableDebug">Enable debug output</param>
        /// <returns>True if orphaned (texture), false if paired (preview)</returns>
        public static bool IsOrphanedImage(string imagePath, System.Collections.Generic.IEnumerable<string> allFilePaths, bool enableDebug = false)
        {
            if (string.IsNullOrEmpty(imagePath) || allFilePaths == null)
                return false;

            var imagePathLower = imagePath.ToLowerInvariant();
            var filename = Path.GetFileName(imagePathLower);
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            var directory = Path.GetDirectoryName(imagePathLower);

            // Must be an image file
            if (!IsImageExtension(ext))
                return false;

            var stem = Path.GetFileNameWithoutExtension(filename).ToLowerInvariant();
            if (string.IsNullOrEmpty(stem))
                return false;

            // Check for companion files (same stem, different non-image extension, same directory)
            foreach (var filePath in allFilePaths)
            {
                if (string.IsNullOrEmpty(filePath) || filePath.Equals(imagePathLower, StringComparison.OrdinalIgnoreCase))
                    continue;

                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!string.Equals(fileDirectory, directory, StringComparison.OrdinalIgnoreCase))
                    continue;

                var fileFilename = Path.GetFileName(filePath);
                var fileStem = Path.GetFileNameWithoutExtension(fileFilename).ToLowerInvariant();
                var fileExt = Path.GetExtension(fileFilename).ToLowerInvariant();

                // If we find a file with same stem but different extension (and it's NOT an image)
                // then this image is PAIRED (preview), not orphaned (texture)
                if (fileStem == stem && fileExt != ext && !IsImageExtension(fileExt))
                {
                    if (enableDebug)
                    {
                        System.Diagnostics.Debug.WriteLine($"    PAIRED: {filename} matches {fileFilename} (stem={stem})");
                    }
                    return false; // NOT orphaned, it's a preview
                }
            }

            // No companion file found in same directory → it's orphaned (TEXTURE)
            return true;
        }
    }
}
