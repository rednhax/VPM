using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

                // Now check for .vaj files and extract their dependencies
                var vajEntry = relatedEntries.FirstOrDefault(e => e.Key.EndsWith(".vaj", StringComparison.OrdinalIgnoreCase));
                if (vajEntry != null)
                {
                    extractedCount += ExtractVajDependencies(archive, vajEntry, gameFolder, directoryPath);
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
        /// Extracts texture dependencies found in a .vaj file
        /// </summary>
        /// <param name="archive">The open archive</param>
        /// <param name="vajEntry">The .vaj file entry</param>
        /// <param name="gameFolder">Target game folder path</param>
        /// <param name="directoryPath">Directory path where the .vaj file is located</param>
        /// <returns>Number of dependency files extracted</returns>
        private static int ExtractVajDependencies(IArchive archive, IArchiveEntry vajEntry, string gameFolder, string directoryPath)
        {
            int extractedCount = 0;

            try
            {
                // Read the .vaj file content
                string vajContent;
                using (var stream = vajEntry.OpenEntryStream())
                using (var reader = new StreamReader(stream))
                {
                    vajContent = reader.ReadToEnd();
                }

                // Parse the JSON to find texture dependencies
                var dependencies = ExtractTextureDependenciesFromVaj(vajContent);

                if (dependencies.Count == 0)
                    return 0;

                // Extract each dependency file from the same directory
                foreach (var dependency in dependencies)
                {
                    try
                    {
                        // Construct the full path within the archive
                        var dependencyPath = string.IsNullOrEmpty(directoryPath) 
                            ? dependency 
                            : $"{directoryPath}/{dependency}";

                        // Find the dependency file in the archive
                        var depEntry = archive.Entries
                            .FirstOrDefault(e => e.Key.Equals(dependencyPath, StringComparison.OrdinalIgnoreCase) ||
                                                 e.Key.Equals(dependencyPath.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase));

                        if (depEntry != null)
                        {
                            var targetPath = Path.Combine(gameFolder, depEntry.Key.Replace('/', Path.DirectorySeparatorChar));

                            // Skip if file already exists
                            if (File.Exists(targetPath))
                                continue;

                            // Ensure target directory exists
                            var targetDirectory = Path.GetDirectoryName(targetPath);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }

                            // Extract the dependency file
                            using (var entryStream = depEntry.OpenEntryStream())
                            using (var fileStream = File.Create(targetPath))
                            {
                                entryStream.CopyTo(fileStream);
                            }

                            extractedCount++;
                            System.Diagnostics.Debug.WriteLine($"Extracted dependency: {dependency}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Dependency not found in archive: {dependency}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to extract dependency {dependency}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting .vaj dependencies: {ex.Message}");
            }

            return extractedCount;
        }

        /// <summary>
        /// Extracts texture file names from .vaj JSON content
        /// </summary>
        /// <param name="vajContent">JSON content of the .vaj file</param>
        /// <returns>List of texture file names</returns>
        private static List<string> ExtractTextureDependenciesFromVaj(string vajContent)
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var document = JsonDocument.Parse(vajContent);
                var root = document.RootElement;

                // Look for "storables" array
                if (root.TryGetProperty("storables", out var storables) && storables.ValueKind == JsonValueKind.Array)
                {
                    foreach (var storable in storables.EnumerateArray())
                    {
                        // Look for customTexture properties
                        foreach (var property in storable.EnumerateObject())
                        {
                            if (property.Name.StartsWith("customTexture_", StringComparison.OrdinalIgnoreCase) &&
                                property.Value.ValueKind == JsonValueKind.String)
                            {
                                var textureFile = property.Value.GetString();
                                if (!string.IsNullOrWhiteSpace(textureFile))
                                {
                                    // Support common texture file formats
                                    var ext = Path.GetExtension(textureFile).ToLowerInvariant();
                                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".tga" || ext == ".dds" || ext == ".bmp")
                                    {
                                        dependencies.Add(textureFile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing .vaj file: {ex.Message}");
            }

            return dependencies.ToList();
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

                // Also check for .vaj dependencies
                var vajEntry = relatedEntries.FirstOrDefault(e => e.Key.EndsWith(".vaj", StringComparison.OrdinalIgnoreCase));
                if (vajEntry != null)
                {
                    // Check if dependency files exist
                    if (!AreDependenciesExtracted(archive, vajEntry, gameFolder, directoryPath))
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
        /// Checks if all dependency files from a .vaj file exist in the game folder
        /// </summary>
        /// <param name="archive">The open archive</param>
        /// <param name="vajEntry">The .vaj file entry</param>
        /// <param name="gameFolder">Target game folder path</param>
        /// <param name="directoryPath">Directory path where the .vaj file is located</param>
        /// <returns>True if all dependencies exist, false otherwise</returns>
        private static bool AreDependenciesExtracted(IArchive archive, IArchiveEntry vajEntry, string gameFolder, string directoryPath)
        {
            try
            {
                // Read the .vaj file content
                string vajContent;
                using (var stream = vajEntry.OpenEntryStream())
                using (var reader = new StreamReader(stream))
                {
                    vajContent = reader.ReadToEnd();
                }

                // Parse the JSON to find texture dependencies
                var dependencies = ExtractTextureDependenciesFromVaj(vajContent);

                if (dependencies.Count == 0)
                    return true; // No dependencies, so they're all "extracted"

                // Check each dependency file
                foreach (var dependency in dependencies)
                {
                    // Construct the full path within the archive
                    var dependencyPath = string.IsNullOrEmpty(directoryPath)
                        ? dependency
                        : $"{directoryPath}/{dependency}";

                    // Find the dependency file in the archive
                    var depEntry = archive.Entries
                        .FirstOrDefault(e => e.Key.Equals(dependencyPath, StringComparison.OrdinalIgnoreCase) ||
                                             e.Key.Equals(dependencyPath.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase));

                    if (depEntry != null)
                    {
                        var targetPath = Path.Combine(gameFolder, depEntry.Key.Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(targetPath))
                            return false; // Dependency file doesn't exist
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking .vaj dependencies: {ex.Message}");
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
                // Special-case: VaM previews under Custom/Atom/Person/<Category>
                // Handle common categories like Skin, Clothing, Hair, Appearance so labels are specific and not just "Atom"
                // Examples:
                //   "\\Custom\\Atom\\Person\\Skin\\..."     -> "Skin"
                //   "\\Custom\\Atom\\Person\\Clothing\\..." -> "Clothing"
                //   "\\Custom\\Atom\\Person\\Hair\\..."     -> "Hair"
                //   "\\Custom\\Atom\\Person\\Appearance\\..." -> "Appearance"
                if (parts.Length >= 4 &&
                    parts[0].Equals("Custom", StringComparison.OrdinalIgnoreCase) &&
                    parts[1].Equals("Atom", StringComparison.OrdinalIgnoreCase) &&
                    parts[2].Equals("Person", StringComparison.OrdinalIgnoreCase))
                {
                    var subCategory = parts[3];
                    if (subCategory.Equals("Skin", StringComparison.OrdinalIgnoreCase) ||
                        subCategory.Equals("Clothing", StringComparison.OrdinalIgnoreCase) ||
                        subCategory.Equals("Hair", StringComparison.OrdinalIgnoreCase) ||
                        subCategory.Equals("Appearance", StringComparison.OrdinalIgnoreCase))
                    {
                        return subCategory;
                    }
                }

                // Return the second part (category) if first part is Custom or Scene
                if ((parts[0].Equals("Custom", StringComparison.OrdinalIgnoreCase) ||
                     parts[0].Equals("Scene", StringComparison.OrdinalIgnoreCase)) &&
                    parts.Length > 1)
                {
                    return parts[1];
                }

                // Handle VaM typical scene preview path: Saves/scene/... -> label should be "Scene"
                if (parts[0].Equals("Saves", StringComparison.OrdinalIgnoreCase) && parts.Length > 1 &&
                    parts[1].Equals("scene", StringComparison.OrdinalIgnoreCase))
                {
                    return "Scene";
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
