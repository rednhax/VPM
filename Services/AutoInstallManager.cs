using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VPM.Models;

namespace VPM.Services
{
    /// <summary>
    /// Manages auto-install package list with file persistence and shadow file support.
    /// Inherits common functionality from NameSetManager.
    /// </summary>
    public class AutoInstallManager : NameSetManager
    {
        public event EventHandler AutoInstallChanged
        {
            add => Changed += value;
            remove => Changed -= value;
        }

        public AutoInstallManager(string vamFolderPath)
            : base(
                Path.Combine(vamFolderPath, "Custom", "PluginData", "sfishere", "AutoInstall.txt"),
                Path.Combine(vamFolderPath, "Custom", "PluginData", "sfishere", "AutoInstall.shadow.txt"))
        {
        }

        // Compatibility methods that delegate to base class
        public bool IsAutoInstall(string packageName) => Contains(packageName);
        public void AddAutoInstall(string packageName) => Add(packageName);
        public void RemoveAutoInstall(string packageName) => Remove(packageName);
        public void AddAutoInstallBatch(IEnumerable<string> packageNames) => AddBatch(packageNames);
        public void RemoveAutoInstallBatch(IEnumerable<string> packageNames) => RemoveBatch(packageNames);
        public void ToggleAutoInstall(string packageName) => Toggle(packageName);
        public HashSet<string> GetAllAutoInstall() => GetAll();
        public void LoadAutoInstall() => Load();
        public void SaveAutoInstall() => Save();
        public void ReloadAutoInstall() => Reload();

        protected override List<string> DeserializeMainFile(string json)
        {
            var data = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.AutoInstallData);
            return data?.Names;
        }

        protected override (List<string> additions, List<string> removals) DeserializeShadowFile(string json)
        {
            var data = JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ShadowAutoInstallData);
            return (data?.Additions, data?.Removals);
        }

        protected override string SerializeMainFile(List<string> names)
        {
            var data = new AutoInstallData { Names = names };
            return JsonSerializer.Serialize(data, JsonSourceGenerationContext.Default.AutoInstallData);
        }

        protected override string SerializeShadowFile(List<string> additions, List<string> removals)
        {
            var data = new ShadowAutoInstallData { Additions = additions, Removals = removals };
            return JsonSerializer.Serialize(data, JsonSourceGenerationContext.Default.ShadowAutoInstallData);
        }
    }
}

