using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VPM.Services
{
    /// <summary>
    /// Manages scene favorite markers (.fav files)
    /// </summary>
    public class SceneFavoritesManager
    {
        private readonly string _scenesDirectory;
        private HashSet<string> _favoriteScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public SceneFavoritesManager(string scenesDirectory)
        {
            _scenesDirectory = scenesDirectory;
        }

        /// <summary>
        /// Loads all favorite markers from the scenes directory
        /// </summary>
        public void LoadFavorites()
        {
            _favoriteScenePaths.Clear();

            try
            {
                if (!Directory.Exists(_scenesDirectory))
                    return;

                // Find all .fav files in the scenes directory
                var favFiles = Directory.GetFiles(_scenesDirectory, "*.fav", SearchOption.AllDirectories);
                foreach (var favFile in favFiles)
                {
                    // The .fav file is a marker for the scene file (e.g., scene1.json.fav marks scene1.json as favorite)
                    // Store the full path of the corresponding scene file
                    string sceneFilePath = favFile.Substring(0, favFile.Length - 4); // Remove .fav extension
                    _favoriteScenePaths.Add(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Checks if a scene is marked as favorite
        /// </summary>
        public bool IsFavorite(string sceneFilePath)
        {
            return _favoriteScenePaths.Contains(sceneFilePath);
        }

        /// <summary>
        /// Marks a scene as favorite by creating a .fav file
        /// </summary>
        public void AddFavorite(string sceneFilePath)
        {
            try
            {
                if (!File.Exists(sceneFilePath))
                    return;

                string favFilePath = sceneFilePath + ".fav";
                
                // Create empty .fav file if it doesn't exist
                if (!File.Exists(favFilePath))
                {
                    File.Create(favFilePath).Dispose();
                    _favoriteScenePaths.Add(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Removes favorite marker from a scene by deleting the .fav file
        /// </summary>
        public void RemoveFavorite(string sceneFilePath)
        {
            try
            {
                string favFilePath = sceneFilePath + ".fav";
                
                if (File.Exists(favFilePath))
                {
                    File.Delete(favFilePath);
                    _favoriteScenePaths.Remove(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Adds multiple scenes as favorites
        /// </summary>
        public void AddFavoriteBatch(IEnumerable<string> sceneFilePaths)
        {
            foreach (var sceneFilePath in sceneFilePaths)
            {
                AddFavorite(sceneFilePath);
            }
        }

        /// <summary>
        /// Removes multiple scenes from favorites
        /// </summary>
        public void RemoveFavoriteBatch(IEnumerable<string> sceneFilePaths)
        {
            foreach (var sceneFilePath in sceneFilePaths)
            {
                RemoveFavorite(sceneFilePath);
            }
        }

        /// <summary>
        /// Gets all favorite scene file paths
        /// </summary>
        public HashSet<string> GetAllFavorites()
        {
            return new HashSet<string>(_favoriteScenePaths, StringComparer.OrdinalIgnoreCase);
        }
    }
}
