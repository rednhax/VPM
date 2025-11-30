using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VPM.Models;

namespace VPM.Services
{
    /// <summary>
    /// Manages package favorites with file persistence and shadow file support.
    /// Inherits common functionality from NameSetManager.
    /// </summary>
    public class FavoritesManager : NameSetManager
    {
        public event EventHandler FavoritesChanged
        {
            add => Changed += value;
            remove => Changed -= value;
        }

        public FavoritesManager(string vamFolderPath)
            : base(
                Path.Combine(vamFolderPath, "Custom", "PluginData", "sfishere", "Favorites.txt"),
                Path.Combine(vamFolderPath, "Custom", "PluginData", "sfishere", "Favorites.shadow.txt"))
        {
        }

        // Compatibility methods that delegate to base class
        public bool IsFavorite(string packageName) => Contains(packageName);
        public void AddFavorite(string packageName) => Add(packageName);
        public void RemoveFavorite(string packageName) => Remove(packageName);
        public void AddFavoriteBatch(IEnumerable<string> packageNames) => AddBatch(packageNames);
        public void RemoveFavoriteBatch(IEnumerable<string> packageNames) => RemoveBatch(packageNames);
        public void ToggleFavorite(string packageName) => Toggle(packageName);
        public HashSet<string> GetAllFavorites() => GetAll();
        public void LoadFavorites() => Load();
        public void SaveFavorites() => Save();
        public void ReloadFavorites() => Reload();

        protected override List<string> DeserializeMainFile(string json)
        {
            var data = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.FavoritesData);
            return data?.FavoriteNames;
        }

        protected override (List<string> additions, List<string> removals) DeserializeShadowFile(string json)
        {
            var data = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ShadowFavoritesData);
            return (data?.Additions, data?.Removals);
        }

        protected override string SerializeMainFile(List<string> names)
        {
            var data = new FavoritesData { FavoriteNames = names };
            return JsonSerializer.Serialize(data, JsonSourceGenerationContext.Default.FavoritesData);
        }

        protected override string SerializeShadowFile(List<string> additions, List<string> removals)
        {
            var data = new ShadowFavoritesData { Additions = additions, Removals = removals };
            return JsonSerializer.Serialize(data, JsonSourceGenerationContext.Default.ShadowFavoritesData);
        }
    }
}

