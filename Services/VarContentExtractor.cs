using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpCompress.Archives;

namespace VPM.Services
{
    /// <summary>
    /// Extracts content from .VAR archives to the game folder, maintaining directory structure.
    /// Supports extracting all files with the same base name but different extensions.
    /// </summary>
    public class VarContentExtractor
    {
        /// <summary>
        /// Extracts all files with the same base name as the image to the game folder.
        /// Maintains the directory structure from the archive.
        /// </summary>
        /// <param name="varFilePath">Full path to the .var archive</param>
        /// <param name="internalImagePath">Internal path to the image within the archive (e.g., "\Custom\Hair\Female\...\image.jpg")</param>
        /// <param name="gameFolder">Target game folder path</param>
        /// <returns>Number of files extracted</returns>
        public static async Task<int> ExtractRelatedFilesAsync(string varFilePath, string internalImagePath, string gameFolder)
        {
            return await Task.Run(() => ExtractRelatedFiles(varFilePath, internalImagePath, gameFolder));
        }

        /// <summary>
        /// Synchronous version of ExtractRelatedFilesAsync
        /// </summary>
        public static int ExtractRelatedFiles(string varFilePath, string internalImagePath, string gameFolder)
        {
            if (!File.Exists(varFilePath))
                throw new FileNotFoundException($"VAR file not found: {varFilePath}");

            if (string.IsNullOrWhiteSpace(internalImagePath))
                throw new ArgumentException("Internal image path cannot be empty");

            if (string.IsNullOrWhiteSpace(gameFolder))
                throw new ArgumentException("Game folder cannot be empty");

            int extractedCount = 0;

            try
            {
                using var archive = SharpCompressHelper.OpenForRead(varFilePath);

                // Get the base name without extension (e.g., "Scalp mid base (by REN)")
                var baseName = Path.GetFileNameWithoutExtension(internalImagePath);
                var directoryPath = Path.GetDirectoryName(internalImagePath);

                // Normalize paths to use forward slashes for consistency
                directoryPath = directoryPath?.Replace('\\', '/') ?? "";

                // Find all files in the archive with the same base name
                var relatedEntries = archive.Entries
                    .Where(e => !e.Key.EndsWith("/"))
                    .Where(e =>
                    {
                        var entryBaseName = Path.GetFileNameWithoutExtension(e.Key);
                        var entryDir = Path.GetDirectoryName(e.Key)?.Replace('\\', '/') ?? "";

                        // Match if same base name and same directory
                        return entryBaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase) &&
                               entryDir.Equals(directoryPath, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                if (relatedEntries.Count == 0)
                    return 0;

                // Extract each related file
                foreach (var entry in relatedEntries)
                {
                    try
                    {
                        // Construct the target path in the game folder
                        var targetPath = Path.Combine(gameFolder, entry.Key.Replace('/', Path.DirectorySeparatorChar));

                        // Ensure target directory exists
                        var targetDirectory = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }

                        // Extract the file
                        using (var entryStream = entry.OpenEntryStream())
                        using (var fileStream = File.Create(targetPath))
                        {
                            entryStream.CopyTo(fileStream);
                        }

                        extractedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to extract {entry.Key}: {ex.Message}");
                    }
                }

                return extractedCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during extraction: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if all related files already exist in the game folder
        /// </summary>
        /// <param name="varFilePath">Full path to the .var archive</param>
        /// <param name="internalImagePath">Internal path to the image within the archive</param>
        /// <param name="gameFolder">Target game folder path</param>
        /// <returns>True if all related files exist, false otherwise</returns>
        public static async Task<bool> AreRelatedFilesExtractedAsync(string varFilePath, string internalImagePath, string gameFolder)
        {
            return await Task.Run(() => AreRelatedFilesExtracted(varFilePath, internalImagePath, gameFolder));
        }

        /// <summary>
        /// Synchronous version of AreRelatedFilesExtractedAsync
        /// </summary>
        public static bool AreRelatedFilesExtracted(string varFilePath, string internalImagePath, string gameFolder)
        {
            if (!File.Exists(varFilePath))
                return false;

            if (string.IsNullOrWhiteSpace(internalImagePath) || string.IsNullOrWhiteSpace(gameFolder))
                return false;

            try
            {
                using var archive = SharpCompressHelper.OpenForRead(varFilePath);

                // Get the base name without extension
                var baseName = Path.GetFileNameWithoutExtension(internalImagePath);
                var directoryPath = Path.GetDirectoryName(internalImagePath);
                directoryPath = directoryPath?.Replace('\\', '/') ?? "";

                // Find all files in the archive with the same base name
                var relatedEntries = archive.Entries
                    .Where(e => !e.Key.EndsWith("/"))
                    .Where(e =>
                    {
                        var entryBaseName = Path.GetFileNameWithoutExtension(e.Key);
                        var entryDir = Path.GetDirectoryName(e.Key)?.Replace('\\', '/') ?? "";

                        return entryBaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase) &&
                               entryDir.Equals(directoryPath, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                if (relatedEntries.Count == 0)
                    return false;

                // Check if all related files exist in the game folder
                foreach (var entry in relatedEntries)
                {
                    var targetPath = Path.Combine(gameFolder, entry.Key.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(targetPath))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking extracted files: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the category from the internal path (e.g., "Hair" from "\Custom\Hair\Female\...")
        /// </summary>
        public static string GetCategoryFromPath(string internalPath)
        {
            if (string.IsNullOrWhiteSpace(internalPath))
                return "";

            var parts = internalPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Expected structure: Custom/[Category]/[Subcategory]/...
            // or Scene/[Category]/...
            if (parts.Length >= 2)
            {
                // Return the second part (category) if first part is Custom or Scene
                if ((parts[0].Equals("Custom", StringComparison.OrdinalIgnoreCase) ||
                     parts[0].Equals("Scene", StringComparison.OrdinalIgnoreCase)) &&
                    parts.Length > 1)
                {
                    return parts[1];
                }
            }

            return "";
        }

        /// <summary>
        /// Gets a single character representation of the category for display
        /// </summary>
        public static string GetCategoryLetter(string internalPath)
        {
            var category = GetCategoryFromPath(internalPath);
            if (string.IsNullOrWhiteSpace(category))
                return "?";

            return category.Substring(0, 1).ToUpper();
        }
    }
}
