using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VPM.Services
{
    /// <summary>
    /// Manages scene hide markers (.hide files)
    /// </summary>
    public class SceneHideManager
    {
        private readonly string _scenesDirectory;
        private HashSet<string> _hiddenScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public SceneHideManager(string scenesDirectory)
        {
            _scenesDirectory = scenesDirectory;
        }

        /// <summary>
        /// Loads all hide markers from the scenes directory
        /// </summary>
        public void LoadHidden()
        {
            _hiddenScenePaths.Clear();

            try
            {
                if (!Directory.Exists(_scenesDirectory))
                    return;

                // Find all .hide files in the scenes directory
                var hideFiles = Directory.GetFiles(_scenesDirectory, "*.hide", SearchOption.AllDirectories);
                foreach (var hideFile in hideFiles)
                {
                    // The .hide file is a marker for the scene file (e.g., scene1.json.hide marks scene1.json as hidden)
                    // Store the full path of the corresponding scene file
                    string sceneFilePath = hideFile.Substring(0, hideFile.Length - 5); // Remove .hide extension
                    _hiddenScenePaths.Add(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Checks if a scene is marked as hidden
        /// </summary>
        public bool IsHidden(string sceneFilePath)
        {
            return _hiddenScenePaths.Contains(sceneFilePath);
        }

        /// <summary>
        /// Marks a scene as hidden by creating a .hide file
        /// </summary>
        public void AddHidden(string sceneFilePath)
        {
            try
            {
                if (!File.Exists(sceneFilePath))
                    return;

                string hideFilePath = sceneFilePath + ".hide";
                
                // Create empty .hide file if it doesn't exist
                if (!File.Exists(hideFilePath))
                {
                    File.Create(hideFilePath).Dispose();
                    _hiddenScenePaths.Add(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Removes hidden marker from a scene by deleting the .hide file
        /// </summary>
        public void RemoveHidden(string sceneFilePath)
        {
            try
            {
                string hideFilePath = sceneFilePath + ".hide";
                
                if (File.Exists(hideFilePath))
                {
                    File.Delete(hideFilePath);
                    _hiddenScenePaths.Remove(sceneFilePath);
                }
            }
            catch (Exception)
            {
                // Silently handle errors
            }
        }

        /// <summary>
        /// Adds multiple scenes as hidden
        /// </summary>
        public void AddHiddenBatch(IEnumerable<string> sceneFilePaths)
        {
            foreach (var sceneFilePath in sceneFilePaths)
            {
                AddHidden(sceneFilePath);
            }
        }

        /// <summary>
        /// Removes multiple scenes from hidden
        /// </summary>
        public void RemoveHiddenBatch(IEnumerable<string> sceneFilePaths)
        {
            foreach (var sceneFilePath in sceneFilePaths)
            {
                RemoveHidden(sceneFilePath);
            }
        }

        /// <summary>
        /// Gets all hidden scene file paths
        /// </summary>
        public HashSet<string> GetAllHidden()
        {
            return new HashSet<string>(_hiddenScenePaths, StringComparer.OrdinalIgnoreCase);
        }
    }
}
