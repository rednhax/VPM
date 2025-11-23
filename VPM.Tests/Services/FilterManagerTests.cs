using System;
using System.Collections.Generic;
using Xunit;
using VPM.Models;
using VPM.Services;

namespace VPM.Tests.Services
{
    public class FilterManagerTests
    {
        [Fact]
        public void ClearAllFilters_ClearsAllFilterProperties()
        {
            var manager = new FilterManager
            {
                SelectedStatus = "Loaded",
                SearchText = "test",
                FilterDuplicates = true,
                SelectedCategory = "Looks",
                SelectedCreator = "Creator1"
            };
            manager.SelectedStatuses.Add("Loaded");
            manager.SelectedCategories.Add("Looks");
            manager.SelectedFileSizeRanges.Add("1-10");

            manager.ClearAllFilters();

            Assert.Null(manager.SelectedStatus);
            Assert.Empty(manager.SelectedStatuses);
            Assert.Empty(manager.SelectedCategories);
            Assert.Null(manager.SelectedCategory);
            Assert.Null(manager.SelectedCreator);
            Assert.Empty(manager.SearchText);
            Assert.False(manager.FilterDuplicates);
            Assert.Empty(manager.SelectedFileSizeRanges);
        }

        [Fact]
        public void ClearCategoryFilter_ClearsCategoryProperties()
        {
            var manager = new FilterManager
            {
                SelectedCategory = "Looks"
            };
            manager.SelectedCategories.Add("Looks");
            manager.SelectedCategories.Add("Hair");

            manager.ClearCategoryFilter();

            Assert.Null(manager.SelectedCategory);
            Assert.Empty(manager.SelectedCategories);
        }

        [Fact]
        public void ClearCreatorFilter_ClearsCreatorProperties()
        {
            var manager = new FilterManager
            {
                SelectedCreator = "Creator1"
            };
            manager.SelectedCreators.Add("Creator1");
            manager.SelectedCreators.Add("Creator2");

            manager.ClearCreatorFilter();

            Assert.Null(manager.SelectedCreator);
            Assert.Empty(manager.SelectedCreators);
        }

        [Fact]
        public void ClearStatusFilter_ClearsStatusPropertiesAndDuplicateFilter()
        {
            var manager = new FilterManager
            {
                SelectedStatus = "Loaded",
                FilterDuplicates = true
            };
            manager.SelectedStatuses.Add("Loaded");

            manager.ClearStatusFilter();

            Assert.Null(manager.SelectedStatus);
            Assert.Empty(manager.SelectedStatuses);
            Assert.False(manager.FilterDuplicates);
        }

        [Fact]
        public void ClearDateFilter_ResetsDateFilter()
        {
            var manager = new FilterManager();
            manager.DateFilter = new DateFilter { FilterType = DateFilterType.PastWeek };

            manager.ClearDateFilter();

            Assert.NotNull(manager.DateFilter);
            Assert.Equal(DateFilterType.AllTime, manager.DateFilter.FilterType);
        }

        [Fact]
        public void ClearFileSizeFilter_ClearsFileSizeRanges()
        {
            var manager = new FilterManager();
            manager.SelectedFileSizeRanges.Add("1-10");
            manager.SelectedFileSizeRanges.Add("10-100");

            manager.ClearFileSizeFilter();

            Assert.Empty(manager.SelectedFileSizeRanges);
        }

        [Fact]
        public void ClearSubfoldersFilter_ClearsSubfolders()
        {
            var manager = new FilterManager();
            manager.SelectedSubfolders.Add("Folder1");
            manager.SelectedSubfolders.Add("Folder2");

            manager.ClearSubfoldersFilter();

            Assert.Empty(manager.SelectedSubfolders);
        }

        [Fact]
        public void SetSearchText_PreparesAndSetsSearchText()
        {
            var manager = new FilterManager();

            manager.SetSearchText("  Test Package  ");

            Assert.Equal("Test Package", manager.SearchText);
        }

        [Fact]
        public void SetSearchText_HandlesNullInput()
        {
            var manager = new FilterManager();

            manager.SetSearchText(null);

            Assert.Equal("", manager.SearchText);
        }

        [Fact]
        public void MatchesSearch_EmptySearchText_ReturnsTrue()
        {
            var manager = new FilterManager();

            var result = manager.MatchesSearch("AnyPackage");

            Assert.True(result);
        }

        [Fact]
        public void MatchesSearch_MatchingPackageName_ReturnsTrue()
        {
            var manager = new FilterManager();
            manager.SetSearchText("Test");

            var result = manager.MatchesSearch("TestPackage");

            Assert.True(result);
        }

        [Fact]
        public void MatchesSearch_NonMatchingPackageName_ReturnsFalse()
        {
            var manager = new FilterManager();
            manager.SetSearchText("xyz");

            var result = manager.MatchesSearch("TestPackage");

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_NullMetadata_ReturnsFalse()
        {
            var manager = new FilterManager();

            var result = manager.MatchesFilters(null);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_HideArchivedPackages_FiltersArchivedStatus()
        {
            var manager = new FilterManager { HideArchivedPackages = true };
            var metadata = new VarMetadata { Status = "Archived" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_HideArchivedPackages_FiltersArchivedVariantRole()
        {
            var manager = new FilterManager { HideArchivedPackages = true };
            var metadata = new VarMetadata { VariantRole = "Archived" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_HideArchivedPackages_FiltersArchivedFilename()
        {
            var manager = new FilterManager { HideArchivedPackages = true };
            var metadata = new VarMetadata { Filename = "Package#archived" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_HideArchivedDisabled_ShowsArchivedPackages()
        {
            var manager = new FilterManager { HideArchivedPackages = false };
            var metadata = new VarMetadata { Status = "Archived" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_StatusFilter_MatchingStatus_ReturnsTrue()
        {
            var manager = new FilterManager { SelectedStatus = "Loaded" };
            var metadata = new VarMetadata { Status = "Loaded" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_StatusFilter_NonMatchingStatus_ReturnsFalse()
        {
            var manager = new FilterManager { SelectedStatus = "Loaded" };
            var metadata = new VarMetadata { Status = "Available" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_MultipleStatusFilter_MatchingStatus_ReturnsTrue()
        {
            var manager = new FilterManager();
            manager.SelectedStatuses.Add("Loaded");
            manager.SelectedStatuses.Add("Available");
            var metadata = new VarMetadata { Status = "Loaded" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_MultipleStatusFilter_NonMatchingStatus_ReturnsFalse()
        {
            var manager = new FilterManager();
            manager.SelectedStatuses.Add("Loaded");
            manager.SelectedStatuses.Add("Available");
            var metadata = new VarMetadata { Status = "Missing" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_OptimizedFilter_MatchesOptimizedPackage()
        {
            var manager = new FilterManager();
            manager.SelectedOptimizationStatuses.Add("Optimized");
            var metadata = new VarMetadata { IsOptimized = true };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_UnoptimizedFilter_MatchesUnoptimizedPackage()
        {
            var manager = new FilterManager();
            manager.SelectedOptimizationStatuses.Add("Unoptimized");
            var metadata = new VarMetadata { IsOptimized = false };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_OptimizationFilter_ExcludesArchivedPackages()
        {
            var manager = new FilterManager();
            manager.SelectedOptimizationStatuses.Add("Optimized");
            var metadata = new VarMetadata { Status = "Archived", IsOptimized = true };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_VersionFilter_MatchesLatestVersion()
        {
            var manager = new FilterManager();
            manager.SelectedVersionStatuses.Add("Latest");
            var metadata = new VarMetadata { IsOldVersion = false };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_VersionFilter_MatchesOldVersion()
        {
            var manager = new FilterManager();
            manager.SelectedVersionStatuses.Add("Old");
            var metadata = new VarMetadata { IsOldVersion = true };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_CreatorFilter_MatchingCreator_ReturnsTrue()
        {
            var manager = new FilterManager();
            manager.SelectedCreators.Add("TestCreator");
            var metadata = new VarMetadata { CreatorName = "TestCreator" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_CreatorFilter_CaseInsensitive()
        {
            var manager = new FilterManager();
            manager.SelectedCreators.Add("testcreator");
            var metadata = new VarMetadata { CreatorName = "TestCreator" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_CategoryFilter_MatchingCategory_ReturnsTrue()
        {
            var manager = new FilterManager();
            manager.SelectedCategories.Add("Looks");
            var metadata = new VarMetadata { Categories = new HashSet<string> { "Looks", "Hair" } };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_CategoryFilter_NoMatchingCategory_ReturnsFalse()
        {
            var manager = new FilterManager();
            manager.SelectedCategories.Add("Clothing");
            var metadata = new VarMetadata { Categories = new HashSet<string> { "Looks", "Hair" } };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_LicenseFilter_MatchingLicense_ReturnsTrue()
        {
            var manager = new FilterManager();
            manager.SelectedLicenseTypes.Add("FC");
            var metadata = new VarMetadata { LicenseType = "FC" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void MatchesFilters_LicenseFilter_NonMatchingLicense_ReturnsFalse()
        {
            var manager = new FilterManager();
            manager.SelectedLicenseTypes.Add("FC");
            var metadata = new VarMetadata { LicenseType = "CC BY" };

            var result = manager.MatchesFilters(metadata);

            Assert.False(result);
        }

        [Fact]
        public void MatchesFilters_NoFiltersApplied_ReturnsTrue()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { Status = "Loaded" };

            var result = manager.MatchesFilters(metadata);

            Assert.True(result);
        }

        [Fact]
        public void PassesPackageFilter_NullPackage_ReturnsFalse()
        {
            var manager = new FilterManager();
            var filters = new Dictionary<string, object>();

            var result = manager.PassesPackageFilter(null, "", filters);

            Assert.False(result);
        }

        [Fact]
        public void PassesPackageFilter_SearchTextMatch_ReturnsTrue()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "TestPackage" };
            var filters = new Dictionary<string, object>();

            var result = manager.PassesPackageFilter(package, "Test", filters);

            Assert.True(result);
        }

        [Fact]
        public void PassesPackageFilter_SearchTextNoMatch_ReturnsFalse()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "TestPackage" };
            var filters = new Dictionary<string, object>();

            var result = manager.PassesPackageFilter(package, "xyz", filters);

            Assert.False(result);
        }

        [Fact]
        public void PassesPackageFilter_StatusFilter_Match_ReturnsTrue()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", Status = "Loaded" };
            var filters = new Dictionary<string, object>
            {
                ["Status"] = new List<string> { "Loaded", "Available" }
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.True(result);
        }

        [Fact]
        public void PassesPackageFilter_StatusFilter_NoMatch_ReturnsFalse()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", Status = "Missing" };
            var filters = new Dictionary<string, object>
            {
                ["Status"] = new List<string> { "Loaded", "Available" }
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.False(result);
        }

        [Fact]
        public void PassesPackageFilter_DuplicateFilter_NonDuplicate_ReturnsFalse()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", IsDuplicate = false };
            var filters = new Dictionary<string, object>
            {
                ["Duplicate"] = true
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.False(result);
        }

        [Fact]
        public void PassesPackageFilter_DuplicateFilter_IsDuplicate_ReturnsTrue()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", IsDuplicate = true };
            var filters = new Dictionary<string, object>
            {
                ["Duplicate"] = true
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.True(result);
        }

        [Fact]
        public void PassesPackageFilter_CreatorFilter_Match_ReturnsTrue()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", Creator = "TestCreator" };
            var filters = new Dictionary<string, object>
            {
                ["Creator"] = new List<string> { "TestCreator" }
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.True(result);
        }

        [Fact]
        public void PassesPackageFilter_CreatorFilter_CaseInsensitive()
        {
            var manager = new FilterManager();
            var package = new PackageItem { Name = "Test", Creator = "TestCreator" };
            var filters = new Dictionary<string, object>
            {
                ["Creator"] = new List<string> { "testcreator" }
            };

            var result = manager.PassesPackageFilter(package, "", filters);

            Assert.True(result);
        }

        [Fact]
        public void GetPackageSubfolder_NoAddonPackagesPath_ReturnsNull()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { FilePath = @"C:\SomeFolder\Package.var" };

            var result = manager.GetPackageSubfolder(metadata);

            Assert.Null(result);
        }

        [Fact]
        public void GetPackageSubfolder_DirectlyInAddonPackages_ReturnsNull()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { FilePath = @"C:\VAM\AddonPackages\Package.var" };

            var result = manager.GetPackageSubfolder(metadata);

            Assert.Null(result);
        }

        [Fact]
        public void GetPackageSubfolder_InSubfolder_ReturnsSubfolderPath()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { FilePath = @"C:\VAM\AddonPackages\Creator\Package.var" };

            var result = manager.GetPackageSubfolder(metadata);

            Assert.Equal("Creator", result);
        }

        [Fact]
        public void GetPackageSubfolder_InNestedSubfolders_ReturnsFullPath()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { FilePath = @"C:\VAM\AddonPackages\Creator\Looks\Package.var" };

            var result = manager.GetPackageSubfolder(metadata);

            Assert.Equal("Creator/Looks", result);
        }

        [Fact]
        public void GetPackageSubfolder_AllPackagesFolder_ReturnsSubfolderPath()
        {
            var manager = new FilterManager();
            var metadata = new VarMetadata { FilePath = @"C:\VAM\AllPackages\Creator\Package.var" };

            var result = manager.GetPackageSubfolder(metadata);

            Assert.Equal("Creator", result);
        }
    }
}
