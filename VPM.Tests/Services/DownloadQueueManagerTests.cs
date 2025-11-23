using Xunit;
using VPM.Services;
using VPM.Models;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace VPM.Tests.Services
{
    public class DownloadQueueManagerTests : IDisposable
    {
        private readonly Mock<PackageDownloader> _mockDownloader;
        private readonly DownloadQueueManager _queueManager;

        public DownloadQueueManagerTests()
        {
            _mockDownloader = new Mock<PackageDownloader>(null, null, null);
            _queueManager = new DownloadQueueManager(_mockDownloader.Object, 2);
        }

        public void Dispose()
        {
            _queueManager?.Dispose();
        }

        [Fact]
        public void Constructor_NullDownloader_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DownloadQueueManager(null));
        }

        [Fact]
        public void Constructor_InitializesWithCorrectDefaults()
        {
            Assert.Equal(0, _queueManager.QueuedCount);
            Assert.Equal(0, _queueManager.ActiveCount);
            Assert.Empty(_queueManager.QueuedDownloads);
            Assert.Empty(_queueManager.ActiveDownloads);
        }

        [Fact]
        public void EnqueueDownload_NullPackageName_ReturnsFalse()
        {
            var result = _queueManager.EnqueueDownload(null, new PackageDownloadInfo());

            Assert.False(result);
            Assert.Equal(0, _queueManager.QueuedCount);
        }

        [Fact]
        public void EnqueueDownload_EmptyPackageName_ReturnsFalse()
        {
            var result = _queueManager.EnqueueDownload("", new PackageDownloadInfo());

            Assert.False(result);
            Assert.Equal(0, _queueManager.QueuedCount);
        }

        [Fact]
        public void EnqueueDownload_WhitespacePackageName_ReturnsFalse()
        {
            var result = _queueManager.EnqueueDownload("   ", new PackageDownloadInfo());

            Assert.False(result);
            Assert.Equal(0, _queueManager.QueuedCount);
        }

        [Fact]
        public void EnqueueDownload_ValidPackage_EnqueuesSuccessfully()
        {
            var downloadInfo = new PackageDownloadInfo
            {
                DownloadUrl = "https://example.com/package.var"
            };

            var result = _queueManager.EnqueueDownload("TestPackage.1", downloadInfo);

            Assert.True(result);
            Assert.Equal(1, _queueManager.QueuedCount);
        }

        [Fact]
        public void EnqueueDownload_DuplicatePackage_ReturnsFalse()
        {
            var downloadInfo = new PackageDownloadInfo
            {
                DownloadUrl = "https://example.com/package.var"
            };

            _queueManager.EnqueueDownload("TestPackage.1", downloadInfo);
            var result = _queueManager.EnqueueDownload("TestPackage.1", downloadInfo);

            Assert.False(result);
            Assert.Equal(1, _queueManager.QueuedCount);
        }

        [Fact]
        public void EnqueueDownload_RaisesDownloadQueuedEvent()
        {
            bool eventRaised = false;
            string queuedPackageName = null;

            _queueManager.DownloadQueued += (sender, args) =>
            {
                eventRaised = true;
                queuedPackageName = args.Download.PackageName;
            };

            _queueManager.EnqueueDownload("TestPackage.1", new PackageDownloadInfo());

            Assert.True(eventRaised);
            Assert.Equal("TestPackage.1", queuedPackageName);
        }

        [Fact]
        public void EnqueueDownload_RaisesQueueStatusChangedEvent()
        {
            bool eventRaised = false;
            int queuedCount = 0;

            _queueManager.QueueStatusChanged += (sender, args) =>
            {
                eventRaised = true;
                queuedCount = args.QueuedCount;
            };

            _queueManager.EnqueueDownload("TestPackage.1", new PackageDownloadInfo());

            Assert.True(eventRaised);
            Assert.Equal(1, queuedCount);
        }

        [Fact]
        public void QueuedDownloads_ReturnsAllQueuedPackages()
        {
            _queueManager.EnqueueDownload("Package1.1", new PackageDownloadInfo());
            _queueManager.EnqueueDownload("Package2.1", new PackageDownloadInfo());
            _queueManager.EnqueueDownload("Package3.1", new PackageDownloadInfo());

            var queued = _queueManager.QueuedDownloads.ToList();

            Assert.Equal(3, queued.Count);
            Assert.Contains(queued, d => d.PackageName == "Package1.1");
            Assert.Contains(queued, d => d.PackageName == "Package2.1");
            Assert.Contains(queued, d => d.PackageName == "Package3.1");
        }

        [Fact]
        public void RemoveFromQueue_NonExistentPackage_ReturnsFalse()
        {
            var result = _queueManager.RemoveFromQueue("NonExistent.1");

            Assert.False(result);
        }

        [Fact]
        public void RemoveFromQueue_ExistingPackage_RemovesSuccessfully()
        {
            _queueManager.EnqueueDownload("TestPackage.1", new PackageDownloadInfo());

            var result = _queueManager.RemoveFromQueue("TestPackage.1");

            Assert.True(result);
        }

        [Fact]
        public void RemoveFromQueue_RaisesDownloadRemovedEvent()
        {
            bool eventRaised = false;
            string removedPackageName = null;

            _queueManager.EnqueueDownload("TestPackage.1", new PackageDownloadInfo());

            _queueManager.DownloadRemoved += (sender, args) =>
            {
                eventRaised = true;
                removedPackageName = args.Download.PackageName;
            };

            _queueManager.RemoveFromQueue("TestPackage.1");

            Assert.True(eventRaised);
            Assert.Equal("TestPackage.1", removedPackageName);
        }

        [Fact]
        public void ClearQueue_WithMultiplePackages_RemovesAll()
        {
            _queueManager.EnqueueDownload("Package1.1", new PackageDownloadInfo());
            _queueManager.EnqueueDownload("Package2.1", new PackageDownloadInfo());
            _queueManager.EnqueueDownload("Package3.1", new PackageDownloadInfo());

            _queueManager.ClearQueue();

            Assert.Equal(0, _queueManager.QueuedCount);
        }

        [Fact]
        public void ClearQueue_RaisesQueueStatusChangedEvent()
        {
            bool eventRaised = false;
            int finalQueuedCount = -1;

            _queueManager.EnqueueDownload("Package1.1", new PackageDownloadInfo());
            _queueManager.EnqueueDownload("Package2.1", new PackageDownloadInfo());

            _queueManager.QueueStatusChanged += (sender, args) =>
            {
                eventRaised = true;
                finalQueuedCount = args.QueuedCount;
            };

            _queueManager.ClearQueue();

            Assert.True(eventRaised);
            Assert.Equal(0, finalQueuedCount);
        }

        [Fact]
        public void ActiveDownloads_InitiallyEmpty()
        {
            var active = _queueManager.ActiveDownloads;

            Assert.Empty(active);
        }

        [Fact]
        public void QueuedCount_UpdatesCorrectly()
        {
            Assert.Equal(0, _queueManager.QueuedCount);

            _queueManager.EnqueueDownload("Package1.1", new PackageDownloadInfo());
            Assert.Equal(1, _queueManager.QueuedCount);

            _queueManager.EnqueueDownload("Package2.1", new PackageDownloadInfo());
            Assert.Equal(2, _queueManager.QueuedCount);

            _queueManager.RemoveFromQueue("Package1.1");
        }

        [Fact]
        public void ActiveCount_InitiallyZero()
        {
            Assert.Equal(0, _queueManager.ActiveCount);
        }

        [Fact]
        public void CancelDownload_NonExistentPackage_ReturnsFalse()
        {
            var result = _queueManager.CancelDownload("NonExistent.1");

            Assert.False(result);
        }

        [Fact]
        public void Dispose_CleansUpResources()
        {
            var manager = new DownloadQueueManager(_mockDownloader.Object, 2);
            manager.EnqueueDownload("Package1.1", new PackageDownloadInfo());

            manager.Dispose();
        }

        [Fact]
        public void Constructor_CustomMaxConcurrentDownloads_SetsCorrectly()
        {
            var manager = new DownloadQueueManager(_mockDownloader.Object, 5);

            Assert.NotNull(manager);
            manager.Dispose();
        }

        [Fact]
        public void Constructor_ZeroMaxConcurrentDownloads_ClampsToMinimum()
        {
            var manager = new DownloadQueueManager(_mockDownloader.Object, 0);

            Assert.NotNull(manager);
            manager.Dispose();
        }

        [Fact]
        public void Constructor_NegativeMaxConcurrentDownloads_ClampsToMinimum()
        {
            var manager = new DownloadQueueManager(_mockDownloader.Object, -5);

            Assert.NotNull(manager);
            manager.Dispose();
        }
    }
}
