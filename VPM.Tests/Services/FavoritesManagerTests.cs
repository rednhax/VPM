using Xunit;
using VPM.Services;
using System;
using System.IO;
using System.Linq;

namespace VPM.Tests.Services
{
    public class FavoritesManagerTests : IDisposable
    {
        private readonly string _testFolder;

        public FavoritesManagerTests()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), "VPM_FavoritesTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testFolder);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testFolder))
            {
                try
                {
                    Directory.Delete(_testFolder, true);
                }
                catch { }
            }
        }

        [Fact]
        public void IsFavorite_NewManager_ReturnsFalse()
        {
            var manager = new FavoritesManager(_testFolder);

            bool result = manager.IsFavorite("TestPackage.1");

            Assert.False(result);
        }

        [Fact]
        public void AddFavorite_NewPackage_AddsSuccessfully()
        {
            var manager = new FavoritesManager(_testFolder);

            manager.AddFavorite("TestPackage.1");

            Assert.True(manager.IsFavorite("TestPackage.1"));
        }

        [Fact]
        public void AddFavorite_DuplicatePackage_DoesNotAddTwice()
        {
            var manager = new FavoritesManager(_testFolder);

            manager.AddFavorite("TestPackage.1");
            manager.AddFavorite("TestPackage.1");

            var favorites = manager.GetAllFavorites();
            Assert.Single(favorites);
            Assert.Contains("TestPackage.1", favorites);
        }

        [Fact]
        public void RemoveFavorite_ExistingPackage_RemovesSuccessfully()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("TestPackage.1");

            manager.RemoveFavorite("TestPackage.1");

            Assert.False(manager.IsFavorite("TestPackage.1"));
        }

        [Fact]
        public void RemoveFavorite_NonExistentPackage_DoesNothing()
        {
            var manager = new FavoritesManager(_testFolder);

            manager.RemoveFavorite("NonExistent.1");

            Assert.False(manager.IsFavorite("NonExistent.1"));
        }

        [Fact]
        public void ToggleFavorite_NewPackage_AddsFavorite()
        {
            var manager = new FavoritesManager(_testFolder);

            manager.ToggleFavorite("TestPackage.1");

            Assert.True(manager.IsFavorite("TestPackage.1"));
        }

        [Fact]
        public void ToggleFavorite_ExistingPackage_RemovesFavorite()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("TestPackage.1");

            manager.ToggleFavorite("TestPackage.1");

            Assert.False(manager.IsFavorite("TestPackage.1"));
        }

        [Fact]
        public void AddFavoriteBatch_MultiplePackages_AddsAll()
        {
            var manager = new FavoritesManager(_testFolder);
            var packages = new[] { "Package1.1", "Package2.1", "Package3.1" };

            manager.AddFavoriteBatch(packages);

            Assert.True(manager.IsFavorite("Package1.1"));
            Assert.True(manager.IsFavorite("Package2.1"));
            Assert.True(manager.IsFavorite("Package3.1"));
        }

        [Fact]
        public void AddFavoriteBatch_WithDuplicates_OnlyAddsUnique()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("Package1.1");
            var packages = new[] { "Package1.1", "Package2.1", "Package3.1" };

            manager.AddFavoriteBatch(packages);

            var favorites = manager.GetAllFavorites();
            Assert.Equal(3, favorites.Count);
        }

        [Fact]
        public void RemoveFavoriteBatch_MultiplePackages_RemovesAll()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavoriteBatch(new[] { "Package1.1", "Package2.1", "Package3.1" });

            manager.RemoveFavoriteBatch(new[] { "Package1.1", "Package2.1" });

            Assert.False(manager.IsFavorite("Package1.1"));
            Assert.False(manager.IsFavorite("Package2.1"));
            Assert.True(manager.IsFavorite("Package3.1"));
        }

        [Fact]
        public void GetAllFavorites_EmptyManager_ReturnsEmptySet()
        {
            var manager = new FavoritesManager(_testFolder);

            var favorites = manager.GetAllFavorites();

            Assert.NotNull(favorites);
            Assert.Empty(favorites);
        }

        [Fact]
        public void GetAllFavorites_WithFavorites_ReturnsAllFavorites()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavoriteBatch(new[] { "Package1.1", "Package2.1", "Package3.1" });

            var favorites = manager.GetAllFavorites();

            Assert.Equal(3, favorites.Count);
            Assert.Contains("Package1.1", favorites);
            Assert.Contains("Package2.1", favorites);
            Assert.Contains("Package3.1", favorites);
        }

        [Fact]
        public void GetAllFavorites_ReturnsCopy_NotOriginalSet()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("Package1.1");

            var favorites1 = manager.GetAllFavorites();
            var favorites2 = manager.GetAllFavorites();

            Assert.NotSame(favorites1, favorites2);
        }

        [Fact]
        public void IsFavorite_CaseInsensitive_MatchesCorrectly()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("TestPackage.1");

            Assert.True(manager.IsFavorite("testpackage.1"));
            Assert.True(manager.IsFavorite("TESTPACKAGE.1"));
            Assert.True(manager.IsFavorite("TestPackage.1"));
        }

        [Fact]
        public void ReloadFavorites_AfterModification_ReloadsCorrectly()
        {
            var manager = new FavoritesManager(_testFolder);
            manager.AddFavorite("Package1.1");

            manager.ReloadFavorites();

            Assert.True(manager.IsFavorite("Package1.1"));
        }

        [Fact]
        public void LoadFavorites_NonExistentFile_InitializesEmpty()
        {
            var manager = new FavoritesManager(_testFolder);

            manager.LoadFavorites();

            var favorites = manager.GetAllFavorites();
            Assert.Empty(favorites);
        }
    }
}
