using Xunit;
using VPM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VPM.Tests.Models
{
    public class AppSettingsTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            var settings = new AppSettings();

            Assert.True(settings.IsFirstLaunch);
            Assert.Equal("System", settings.Theme);
            Assert.Equal(3, settings.ImageColumns);
            Assert.True(settings.CascadeFiltering);
            Assert.True(settings.HideArchivedPackages);
        }

        [Fact]
        public void CreateDefault_ReturnsSettingsWithExpectedDefaults()
        {
            var settings = AppSettings.CreateDefault();

            Assert.NotNull(settings);
            Assert.True(settings.IsFirstLaunch);
            Assert.Equal("Dark", settings.Theme);
            Assert.Equal(3, settings.ImageColumns);
            Assert.Equal(1200, settings.WindowWidth);
            Assert.Equal(800, settings.WindowHeight);
            Assert.True(settings.ShowPackageInfo);
            Assert.True(settings.ShowDependencies);
            Assert.True(settings.GroupImagesByPackage);
        }

        [Fact]
        public void ImageColumns_SetValue_ClampsBetween1And12()
        {
            var settings = new AppSettings();

            settings.ImageColumns = 15;
            Assert.Equal(12, settings.ImageColumns);

            settings.ImageColumns = -5;
            Assert.Equal(1, settings.ImageColumns);

            settings.ImageColumns = 6;
            Assert.Equal(6, settings.ImageColumns);
        }

        [Fact]
        public void WindowWidth_SetValue_ClampsToMinimum800()
        {
            var settings = new AppSettings();

            settings.WindowWidth = 600;
            Assert.Equal(800, settings.WindowWidth);

            settings.WindowWidth = 1920;
            Assert.Equal(1920, settings.WindowWidth);
        }

        [Fact]
        public void WindowHeight_SetValue_ClampsToMinimum600()
        {
            var settings = new AppSettings();

            settings.WindowHeight = 400;
            Assert.Equal(600, settings.WindowHeight);

            settings.WindowHeight = 1080;
            Assert.Equal(1080, settings.WindowHeight);
        }

        [Fact]
        public void LeftPanelWidth_SetValue_ClampsToMinimum()
        {
            var settings = new AppSettings();

            settings.LeftPanelWidth = 0.05;
            Assert.Equal(0.1, settings.LeftPanelWidth);

            settings.LeftPanelWidth = 2.5;
            Assert.Equal(2.5, settings.LeftPanelWidth);
        }

        [Fact]
        public void ImageCacheSize_SetValue_ClampsToMinimum50()
        {
            var settings = new AppSettings();

            settings.ImageCacheSize = 10;
            Assert.Equal(50, settings.ImageCacheSize);

            settings.ImageCacheSize = 1000;
            Assert.Equal(1000, settings.ImageCacheSize);
        }

        [Fact]
        public void MaxSafeSelection_SetValue_ClampsBetween50And5000()
        {
            var settings = new AppSettings();

            settings.MaxSafeSelection = 10;
            Assert.Equal(50, settings.MaxSafeSelection);

            settings.MaxSafeSelection = 10000;
            Assert.Equal(5000, settings.MaxSafeSelection);

            settings.MaxSafeSelection = 500;
            Assert.Equal(500, settings.MaxSafeSelection);
        }

        [Fact]
        public void SelectedFolder_SetNull_SetsEmptyString()
        {
            var settings = new AppSettings();

            settings.SelectedFolder = null;

            Assert.Equal("", settings.SelectedFolder);
        }

        [Fact]
        public void CacheFolder_SetNull_SetsEmptyString()
        {
            var settings = new AppSettings();

            settings.CacheFolder = null;

            Assert.Equal("", settings.CacheFolder);
        }

        [Fact]
        public void PropertyChanged_SettingProperty_RaisesEvent()
        {
            var settings = new AppSettings();
            bool eventRaised = false;
            string changedProperty = null;

            settings.PropertyChanged += (sender, args) =>
            {
                eventRaised = true;
                changedProperty = args.PropertyName;
            };

            settings.Theme = "Dark";

            Assert.True(eventRaised);
            Assert.Equal(nameof(settings.Theme), changedProperty);
        }

        [Fact]
        public void PropertyChanged_SettingSameValue_DoesNotRaiseEvent()
        {
            var settings = new AppSettings();
            settings.Theme = "Dark";

            bool eventRaised = false;
            settings.PropertyChanged += (sender, args) => eventRaised = true;

            settings.Theme = "Dark";

            Assert.False(eventRaised);
        }

        [Fact]
        public void PackageFilterOrder_SetNull_SetsDefaultOrder()
        {
            var settings = new AppSettings();

            settings.PackageFilterOrder = null;

            Assert.NotNull(settings.PackageFilterOrder);
            Assert.Contains("DateFilter", settings.PackageFilterOrder);
            Assert.Contains("StatusFilter", settings.PackageFilterOrder);
        }

        [Fact]
        public void SceneFilterOrder_SetNull_SetsDefaultOrder()
        {
            var settings = new AppSettings();

            settings.SceneFilterOrder = null;

            Assert.NotNull(settings.SceneFilterOrder);
            Assert.Contains("SceneTypeFilter", settings.SceneFilterOrder);
        }

        [Fact]
        public void PresetFilterOrder_SetNull_SetsDefaultOrder()
        {
            var settings = new AppSettings();

            settings.PresetFilterOrder = null;

            Assert.NotNull(settings.PresetFilterOrder);
            Assert.Contains("PresetCategoryFilter", settings.PresetFilterOrder);
        }

        [Fact]
        public void SortingStates_SetNull_SetsEmptyDictionary()
        {
            var settings = new AppSettings();

            settings.SortingStates = null;

            Assert.NotNull(settings.SortingStates);
            Assert.Empty(settings.SortingStates);
        }

        [Fact]
        public void BooleanSettings_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.IsFirstLaunch = false;
            Assert.False(settings.IsFirstLaunch);

            settings.CascadeFiltering = false;
            Assert.False(settings.CascadeFiltering);

            settings.WindowMaximized = true;
            Assert.True(settings.WindowMaximized);

            settings.ShowPackageInfo = false;
            Assert.False(settings.ShowPackageInfo);

            settings.ShowDependencies = false;
            Assert.False(settings.ShowDependencies);

            settings.EnableVirtualization = false;
            Assert.False(settings.EnableVirtualization);

            settings.EnableImageCaching = false;
            Assert.False(settings.EnableImageCaching);

            settings.UseThoroughTextureScan = true;
            Assert.True(settings.UseThoroughTextureScan);

            settings.EnableAutoDownload = true;
            Assert.True(settings.EnableAutoDownload);

            settings.ForceLatestDependencies = false;
            Assert.False(settings.ForceLatestDependencies);

            settings.DisableMorphPreload = false;
            Assert.False(settings.DisableMorphPreload);

            settings.MinifyJsonFiles = false;
            Assert.False(settings.MinifyJsonFiles);
        }

        [Fact]
        public void FilterHeightProperties_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.DateFilterHeight = 150;
            Assert.Equal(150, settings.DateFilterHeight);

            settings.StatusFilterHeight = 200;
            Assert.Equal(200, settings.StatusFilterHeight);

            settings.ContentTypesFilterHeight = 250;
            Assert.Equal(250, settings.ContentTypesFilterHeight);
        }

        [Fact]
        public void FilterVisibilityProperties_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.DateFilterVisible = false;
            Assert.False(settings.DateFilterVisible);

            settings.StatusFilterVisible = false;
            Assert.False(settings.StatusFilterVisible);

            settings.ContentTypesFilterVisible = false;
            Assert.False(settings.ContentTypesFilterVisible);
        }

        [Fact]
        public void FileSizeThresholds_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.FileSizeTinyMax = 2;
            Assert.Equal(2, settings.FileSizeTinyMax);

            settings.FileSizeSmallMax = 20;
            Assert.Equal(20, settings.FileSizeSmallMax);

            settings.FileSizeMediumMax = 200;
            Assert.Equal(200, settings.FileSizeMediumMax);
        }

        [Fact]
        public void NetworkPermissionSettings_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.NetworkPermissionGranted = true;
            Assert.True(settings.NetworkPermissionGranted);

            settings.NetworkPermissionAsked = true;
            Assert.True(settings.NetworkPermissionAsked);

            settings.NeverShowNetworkPermissionDialog = true;
            Assert.True(settings.NeverShowNetworkPermissionDialog);
        }

        [Fact]
        public void PanelWidthProperties_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.CenterPanelWidth = 2.0;
            Assert.Equal(2.0, settings.CenterPanelWidth);

            settings.RightPanelWidth = 1.5;
            Assert.Equal(1.5, settings.RightPanelWidth);

            settings.ImagesPanelWidth = 2.5;
            Assert.Equal(2.5, settings.ImagesPanelWidth);

            settings.DepsInfoSplitterHeight = 2.0;
            Assert.Equal(2.0, settings.DepsInfoSplitterHeight);
        }

        [Fact]
        public void FilterListProperties_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            var statusFilters = new List<string> { "Installed", "Updated" };
            settings.SelectedStatusFilters = statusFilters;
            Assert.Equal(statusFilters, settings.SelectedStatusFilters);

            var optimizationFilters = new List<string> { "Optimized" };
            settings.SelectedOptimizationFilters = optimizationFilters;
            Assert.Equal(optimizationFilters, settings.SelectedOptimizationFilters);
        }

        [Fact]
        public void SearchTextProperties_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.PackageSearchText = "test search";
            Assert.Equal("test search", settings.PackageSearchText);

            settings.DependencySearchText = "dependency search";
            Assert.Equal("dependency search", settings.DependencySearchText);

            settings.CreatorFilterText = "creator filter";
            Assert.Equal("creator filter", settings.CreatorFilterText);
        }

        [Fact]
        public void PerformanceSettings_CanBeSetAndRetrieved()
        {
            var settings = new AppSettings();

            settings.CacheLength = 5;
            Assert.Equal(5, settings.CacheLength);

            settings.EnableVirtualization = false;
            Assert.False(settings.EnableVirtualization);
        }
    }
}
