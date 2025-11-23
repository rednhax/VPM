using Xunit;
using Moq;
using VPM.Services;
using VPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPM.Tests.Services
{
    public class PackageUpdateCheckerTests
    {
        private Mock<PackageDownloader> CreateMockPackageDownloader(List<string> onlinePackages = null)
        {
            var mock = new Mock<PackageDownloader>();
            mock.Setup(m => m.GetAllPackageNames())
                .Returns(onlinePackages ?? new List<string>());
            return mock;
        }

        [Fact]
        public void Constructor_NullPackageDownloader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PackageUpdateChecker(null));
        }

        [Fact]
        public void Constructor_ValidPackageDownloader_InitializesSuccessfully()
        {
            var mockDownloader = CreateMockPackageDownloader();

            var checker = new PackageUpdateChecker(mockDownloader.Object);

            Assert.NotNull(checker);
        }

        [Fact]
        public void ExtractVersion_ValidPackageName_ReturnsVersion()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("Creator.Package.5");

            Assert.Equal(5, version);
        }

        [Fact]
        public void ExtractVersion_WithVarExtension_ReturnsVersion()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("Creator.Package.10.var");

            Assert.Equal(10, version);
        }

        [Fact]
        public void ExtractVersion_NoVersion_ReturnsMinusOne()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("Creator.Package");

            Assert.Equal(-1, version);
        }

        [Fact]
        public void ExtractVersion_InvalidVersion_ReturnsMinusOne()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("Creator.Package.abc");

            Assert.Equal(-1, version);
        }

        [Fact]
        public void ExtractVersion_SpecialCharactersInName_ExtractsCorrectly()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("Creator.[Special]_Name.42");

            Assert.Equal(42, version);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_NoLocalPackages_ReturnsEmpty()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            var updates = await checker.CheckForUpdatesAsync(new List<PackageItem>());

            Assert.Empty(updates);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_NoOnlinePackages_ReturnsEmpty()
        {
            var mockDownloader = CreateMockPackageDownloader(new List<string>());
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Empty(updates);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_UpdateAvailable_ReturnsUpdate()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.1",
                "Creator.Package.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal("Creator.Package", updates[0].BaseName);
            Assert.Equal(1, updates[0].LocalVersion);
            Assert.Equal(2, updates[0].OnlineVersion);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_LocalVersionIsLatest_ReturnsEmpty()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.1",
                "Creator.Package.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.2" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Empty(updates);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_LocalVersionNewer_ReturnsEmpty()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.1"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.2" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Empty(updates);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_MultiplePackages_ReturnsMultipleUpdates()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package1.2",
                "Creator.Package2.3",
                "Creator.Package3.1"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package1.1" },
                new PackageItem { Name = "Creator.Package2.2" },
                new PackageItem { Name = "Creator.Package3.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Equal(2, updates.Count);
            Assert.Contains(updates, u => u.BaseName == "Creator.Package1");
            Assert.Contains(updates, u => u.BaseName == "Creator.Package2");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_MultipleLocalVersionsSamePackage_UsesHighest()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.5"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1" },
                new PackageItem { Name = "Creator.Package.3" },
                new PackageItem { Name = "Creator.Package.2" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal(3, updates[0].LocalVersion);
            Assert.Equal(5, updates[0].OnlineVersion);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_InvalidVersionNumbers_SkipsInvalid()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.invalid" },
                new PackageItem { Name = "Creator.Package.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal(1, updates[0].LocalVersion);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_SpecialCharactersInPackageName_HandlesCorrectly()
        {
            var onlinePackages = new List<string>
            {
                "Creator.[Special]_Name.5"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.[Special]_Name.3" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal("Creator.[Special]_Name", updates[0].BaseName);
        }

        [Fact]
        public async Task GetUpdateCount_NoUpdates_ReturnsZero()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int count = checker.GetUpdateCount();

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetUpdateCount_AfterCheck_ReturnsCorrectCount()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package1.2",
                "Creator.Package2.3"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package1.1" },
                new PackageItem { Name = "Creator.Package2.2" }
            };

            await checker.CheckForUpdatesAsync(localPackages);
            int count = checker.GetUpdateCount();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetUpdatePackageNames_NoUpdates_ReturnsEmptyList()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            var names = checker.GetUpdatePackageNames();

            Assert.Empty(names);
        }

        [Fact]
        public async Task GetUpdatePackageNames_AfterCheck_ReturnsPackageNames()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1" }
            };

            await checker.CheckForUpdatesAsync(localPackages);
            var names = checker.GetUpdatePackageNames();

            Assert.Single(names);
            Assert.Equal("Creator.Package.1", names[0]);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_CalledTwice_ClearsPreviousResults()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package1.2",
                "Creator.Package2.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages1 = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package1.1" },
                new PackageItem { Name = "Creator.Package2.1" }
            };
            var localPackages2 = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package1.1" }
            };

            await checker.CheckForUpdatesAsync(localPackages1);
            var firstCount = checker.GetUpdateCount();
            await checker.CheckForUpdatesAsync(localPackages2);
            var secondCount = checker.GetUpdateCount();

            Assert.Equal(2, firstCount);
            Assert.Equal(1, secondCount);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_CaseInsensitive_MatchesPackages()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.2"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "creator.package.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_MultipleOnlineVersions_UsesHighest()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.2",
                "Creator.Package.5",
                "Creator.Package.3"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal(5, updates[0].OnlineVersion);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_PackageNameWithVarExtension_HandlesCorrectly()
        {
            var onlinePackages = new List<string>
            {
                "Creator.Package.2.var"
            };
            var mockDownloader = CreateMockPackageDownloader(onlinePackages);
            var checker = new PackageUpdateChecker(mockDownloader.Object);
            var localPackages = new List<PackageItem>
            {
                new PackageItem { Name = "Creator.Package.1.var" }
            };

            var updates = await checker.CheckForUpdatesAsync(localPackages);

            Assert.Single(updates);
            Assert.Equal("Creator.Package", updates[0].BaseName);
        }

        [Fact]
        public void ExtractVersion_EmptyString_ReturnsMinusOne()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion("");

            Assert.Equal(-1, version);
        }

        [Fact]
        public void ExtractVersion_OnlyExtension_ReturnsMinusOne()
        {
            var mockDownloader = CreateMockPackageDownloader();
            var checker = new PackageUpdateChecker(mockDownloader.Object);

            int version = checker.ExtractVersion(".var");

            Assert.Equal(-1, version);
        }
    }
}
