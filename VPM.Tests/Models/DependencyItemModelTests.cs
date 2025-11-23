using Xunit;
using VPM.Models;
using System.ComponentModel;

namespace VPM.Tests.Models
{
    public class DependencyItemModelTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaults()
        {
            var model = new DependencyItemModel();

            Assert.Equal("", model.Name);
            Assert.Equal("", model.LicenseType);
            Assert.True(model.IsEnabled);
            Assert.False(model.HasSubDependencies);
            Assert.Equal(0, model.Depth);
            Assert.Equal(0, model.SubDependencyCount);
            Assert.False(model.ForceLatest);
            Assert.Equal("", model.ParentName);
            Assert.False(model.IsDisabledByUser);
            Assert.Equal("", model.PackageName);
        }

        [Fact]
        public void Name_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.Name = "TestPackage";

            Assert.Equal("TestPackage", model.Name);
        }

        [Fact]
        public void Name_SetValue_RaisesPropertyChanged()
        {
            var model = new DependencyItemModel();
            string changedProperty = null;
            model.PropertyChanged += (sender, e) => changedProperty = e.PropertyName;

            model.Name = "TestPackage";

            Assert.Equal(nameof(model.Name), changedProperty);
        }

        [Fact]
        public void LicenseType_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.LicenseType = "FC";

            Assert.Equal("FC", model.LicenseType);
        }

        [Fact]
        public void IsEnabled_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.IsEnabled = false;

            Assert.False(model.IsEnabled);
        }

        [Fact]
        public void HasSubDependencies_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.HasSubDependencies = true;

            Assert.True(model.HasSubDependencies);
        }

        [Fact]
        public void Depth_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.Depth = 3;

            Assert.Equal(3, model.Depth);
        }

        [Fact]
        public void SubDependencyCount_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.SubDependencyCount = 5;

            Assert.Equal(5, model.SubDependencyCount);
        }

        [Fact]
        public void ForceLatest_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.ForceLatest = true;

            Assert.True(model.ForceLatest);
        }

        [Fact]
        public void ForceLatest_SetValue_RaisesDisplayNamePropertyChanged()
        {
            var model = new DependencyItemModel();
            var changedProperties = new System.Collections.Generic.List<string>();
            model.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

            model.ForceLatest = true;

            Assert.Contains(nameof(model.ForceLatest), changedProperties);
            Assert.Contains(nameof(model.DisplayName), changedProperties);
        }

        [Fact]
        public void ParentName_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.ParentName = "ParentPackage";

            Assert.Equal("ParentPackage", model.ParentName);
        }

        [Fact]
        public void IsDisabledByUser_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.IsDisabledByUser = true;

            Assert.True(model.IsDisabledByUser);
        }

        [Fact]
        public void IsDisabledByUser_SetValue_RaisesMultiplePropertyChanged()
        {
            var model = new DependencyItemModel();
            var changedProperties = new System.Collections.Generic.List<string>();
            model.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

            model.IsDisabledByUser = true;

            Assert.Contains(nameof(model.IsDisabledByUser), changedProperties);
            Assert.Contains(nameof(model.DisplayName), changedProperties);
            Assert.Contains(nameof(model.IndentedName), changedProperties);
        }

        [Fact]
        public void PackageName_SetValue_UpdatesProperty()
        {
            var model = new DependencyItemModel();

            model.PackageName = "TestPackage.var";

            Assert.Equal("TestPackage.var", model.PackageName);
        }

        [Fact]
        public void WillBeConvertedToLatest_NameWithoutLatest_ReturnsTrue()
        {
            var model = new DependencyItemModel
            {
                Name = "Package.1"
            };

            Assert.True(model.WillBeConvertedToLatest);
        }

        [Fact]
        public void WillBeConvertedToLatest_NameWithLatest_ReturnsFalse()
        {
            var model = new DependencyItemModel
            {
                Name = "Package.latest"
            };

            Assert.False(model.WillBeConvertedToLatest);
        }

        [Fact]
        public void WillBeConvertedToLatest_NameWithLatestMixedCase_ReturnsFalse()
        {
            var model = new DependencyItemModel
            {
                Name = "Package.LATEST"
            };

            Assert.False(model.WillBeConvertedToLatest);
        }

        [Fact]
        public void DisplayName_NoModifiers_ReturnsName()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage"
            };

            Assert.Equal("TestPackage", model.DisplayName);
        }

        [Fact]
        public void DisplayName_IsDisabledByUser_IncludesDisabledPrefix()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage",
                IsDisabledByUser = true
            };

            Assert.Contains("🔴", model.DisplayName);
            Assert.Contains("[DISABLED - Can Re-enable]", model.DisplayName);
        }

        [Fact]
        public void DisplayName_ForceLatestEnabled_IncludesLatestSuffix()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage.1",
                ForceLatest = true
            };

            Assert.Contains("→ .latest", model.DisplayName);
        }

        [Fact]
        public void DisplayName_ForceLatestWithLatestName_NoSuffix()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage.latest",
                ForceLatest = true
            };

            Assert.DoesNotContain("→ .latest", model.DisplayName);
        }

        [Fact]
        public void DisplayName_DisabledAndForceLatest_IncludesBoth()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage.1",
                IsDisabledByUser = true,
                ForceLatest = true
            };

            Assert.Contains("🔴", model.DisplayName);
            Assert.Contains("[DISABLED - Can Re-enable]", model.DisplayName);
            Assert.Contains("→ .latest", model.DisplayName);
        }

        [Fact]
        public void IndentedName_NoDepth_ReturnsDisplayName()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage",
                Depth = 0
            };

            Assert.Equal("TestPackage", model.IndentedName);
        }

        [Fact]
        public void IndentedName_WithDepth_IncludesIndentationAndPrefix()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage",
                Depth = 1
            };

            Assert.Contains("    └─ ", model.IndentedName);
            Assert.Contains("TestPackage", model.IndentedName);
        }

        [Fact]
        public void IndentedName_MultipleDepthLevels_IncludesCorrectIndentation()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage",
                Depth = 3
            };

            var indentation = new string(' ', 3 * 4);
            Assert.Contains(indentation, model.IndentedName);
        }

        [Fact]
        public void SubDependencyCountDisplay_ZeroCount_ReturnsEmpty()
        {
            var model = new DependencyItemModel
            {
                SubDependencyCount = 0
            };

            Assert.Equal("", model.SubDependencyCountDisplay);
        }

        [Fact]
        public void SubDependencyCountDisplay_NonZeroCount_ReturnsCountString()
        {
            var model = new DependencyItemModel
            {
                SubDependencyCount = 5
            };

            Assert.Equal("5", model.SubDependencyCountDisplay);
        }

        [Fact]
        public void PropertyChanged_SetSameValue_DoesNotRaiseEvent()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage"
            };
            int eventCount = 0;
            model.PropertyChanged += (sender, e) => eventCount++;

            model.Name = "TestPackage";

            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void PropertyChanged_SetDifferentValue_RaisesEvent()
        {
            var model = new DependencyItemModel
            {
                Name = "TestPackage"
            };
            int eventCount = 0;
            model.PropertyChanged += (sender, e) => eventCount++;

            model.Name = "NewPackage";

            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void MultiplePropertyChanges_AllRaiseEvents()
        {
            var model = new DependencyItemModel();
            var changedProperties = new System.Collections.Generic.List<string>();
            model.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

            model.Name = "Test";
            model.Depth = 2;
            model.IsEnabled = false;
            model.SubDependencyCount = 3;

            Assert.Equal(4, changedProperties.Count);
        }

        [Fact]
        public void DisplayName_ComplexScenario_FormatsCorrectly()
        {
            var model = new DependencyItemModel
            {
                Name = "Creator.Package.1",
                IsDisabledByUser = true,
                ForceLatest = true,
                Depth = 2
            };

            var display = model.DisplayName;
            Assert.Contains("🔴", display);
            Assert.Contains("Creator.Package.1", display);
            Assert.Contains("[DISABLED - Can Re-enable]", display);
            Assert.Contains("→ .latest", display);
        }
    }
}
