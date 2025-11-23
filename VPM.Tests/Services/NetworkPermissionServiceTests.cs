using Xunit;
using Moq;
using VPM.Services;
using VPM.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VPM.Tests.Services
{
    public class NetworkPermissionServiceTests
    {
        private Mock<ISettingsManager> CreateMockSettingsManager()
        {
            var mockSettings = new Mock<ISettingsManager>();
            mockSettings.Setup(m => m.Settings).Returns(new AppSettings());
            mockSettings.Setup(m => m.SaveSettingsImmediate()).Verifiable();
            return mockSettings;
        }

        [Fact]
        public void Constructor_NullSettingsManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new NetworkPermissionService(null));
        }

        [Fact]
        public void Constructor_ValidSettingsManager_InitializesSuccessfully()
        {
            var mockSettings = CreateMockSettingsManager();

            var service = new NetworkPermissionService(mockSettings.Object);

            Assert.NotNull(service);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_AlwaysReturnsTrue()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            bool result = await service.RequestNetworkAccessAsync();

            Assert.True(result);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_SetsPermissionGranted()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            await service.RequestNetworkAccessAsync();

            Assert.True(mockSettings.Object.Settings.NetworkPermissionGranted);
            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Once);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_SetsNeverShowDialog()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            await service.RequestNetworkAccessAsync();

            Assert.True(mockSettings.Object.Settings.NeverShowNetworkPermissionDialog);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_WithForceShowDialog_StillGrantsAccess()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            bool result = await service.RequestNetworkAccessAsync(forceShowDialog: true);

            Assert.True(result);
            Assert.True(mockSettings.Object.Settings.NetworkPermissionGranted);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_NoOptions_ReturnsGrantedTrue()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var result = await service.RequestNetworkAccessWithOptionsAsync();

            Assert.True(result.granted);
            Assert.False(result.updateDatabase);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_OfferDatabaseUpdate_ReturnsUpdateTrue()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var result = await service.RequestNetworkAccessWithOptionsAsync(offerDatabaseUpdate: true);

            Assert.True(result.granted);
            Assert.True(result.updateDatabase);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_OfferDatabaseUpdateTwice_OnlyUpdatesOnce()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var result1 = await service.RequestNetworkAccessWithOptionsAsync(offerDatabaseUpdate: true);
            var result2 = await service.RequestNetworkAccessWithOptionsAsync(offerDatabaseUpdate: true);

            Assert.True(result1.updateDatabase);
            Assert.False(result2.updateDatabase);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_SetsPermissionSettings()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            await service.RequestNetworkAccessWithOptionsAsync();

            Assert.True(mockSettings.Object.Settings.NetworkPermissionGranted);
            Assert.True(mockSettings.Object.Settings.NeverShowNetworkPermissionDialog);
            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Once);
        }

        [Fact]
        public void IsNetworkAccessAllowed_PermissionGranted_ReturnsTrue()
        {
            var mockSettings = CreateMockSettingsManager();
            mockSettings.Object.Settings.NetworkPermissionGranted = true;
            var service = new NetworkPermissionService(mockSettings.Object);

            bool result = service.IsNetworkAccessAllowed();

            Assert.True(result);
        }

        [Fact]
        public void IsNetworkAccessAllowed_PermissionNotGranted_ReturnsFalse()
        {
            var mockSettings = CreateMockSettingsManager();
            mockSettings.Object.Settings.NetworkPermissionGranted = false;
            var service = new NetworkPermissionService(mockSettings.Object);

            bool result = service.IsNetworkAccessAllowed();

            Assert.False(result);
        }

        [Fact]
        public void ResetNetworkPermission_ResetsAllFlags()
        {
            var mockSettings = CreateMockSettingsManager();
            mockSettings.Object.Settings.NetworkPermissionGranted = true;
            mockSettings.Object.Settings.NeverShowNetworkPermissionDialog = true;
            var service = new NetworkPermissionService(mockSettings.Object);

            service.ResetNetworkPermission();

            Assert.False(mockSettings.Object.Settings.NetworkPermissionGranted);
            Assert.False(mockSettings.Object.Settings.NeverShowNetworkPermissionDialog);
            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Once);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_CalledMultipleTimes_SavesSettingsEachTime()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            await service.RequestNetworkAccessAsync();
            await service.RequestNetworkAccessAsync();
            await service.RequestNetworkAccessAsync();

            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Exactly(3));
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_WithForceShowDialog_IgnoresFlag()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var result = await service.RequestNetworkAccessWithOptionsAsync(forceShowDialog: true);

            Assert.True(result.granted);
        }

        [Fact]
        public void IsNetworkAccessAllowed_DoesNotModifySettings()
        {
            var mockSettings = CreateMockSettingsManager();
            mockSettings.Object.Settings.NetworkPermissionGranted = true;
            var service = new NetworkPermissionService(mockSettings.Object);

            service.IsNetworkAccessAllowed();

            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Never);
        }

        [Fact]
        public async Task RequestNetworkAccessAsync_AlwaysCompletesAsync()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var task = service.RequestNetworkAccessAsync();

            Assert.True(task.IsCompleted);
            bool result = await task;
            Assert.True(result);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_AlwaysCompletesAsync()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var task = service.RequestNetworkAccessWithOptionsAsync();

            Assert.True(task.IsCompleted);
            var result = await task;
            Assert.True(result.granted);
        }

        [Fact]
        public void ResetNetworkPermission_CalledMultipleTimes_WorksCorrectly()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            service.ResetNetworkPermission();
            service.ResetNetworkPermission();
            service.ResetNetworkPermission();

            Assert.False(mockSettings.Object.Settings.NetworkPermissionGranted);
            Assert.False(mockSettings.Object.Settings.NeverShowNetworkPermissionDialog);
            mockSettings.Verify(m => m.SaveSettingsImmediate(), Times.Exactly(3));
        }

        [Fact]
        public async Task PermissionWorkflow_RequestThenCheckThenReset_WorksCorrectly()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            await service.RequestNetworkAccessAsync();
            bool allowed = service.IsNetworkAccessAllowed();
            service.ResetNetworkPermission();
            bool afterReset = service.IsNetworkAccessAllowed();

            Assert.True(allowed);
            Assert.False(afterReset);
        }

        [Fact]
        public async Task RequestNetworkAccessWithOptionsAsync_BothParameters_HandlesCorrectly()
        {
            var mockSettings = CreateMockSettingsManager();
            var service = new NetworkPermissionService(mockSettings.Object);

            var result = await service.RequestNetworkAccessWithOptionsAsync(
                offerDatabaseUpdate: true,
                forceShowDialog: true);

            Assert.True(result.granted);
            Assert.True(result.updateDatabase);
        }
    }
}
