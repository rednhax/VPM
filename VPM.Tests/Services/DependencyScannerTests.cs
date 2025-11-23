using Xunit;
using VPM.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace VPM.Tests.Services
{
    public class DependencyScannerTests
    {
        [Fact]
        public void ScanPackageDependencies_EmptyPath_ReturnsErrorResult()
        {
            var scanner = new DependencyScanner();

            var result = scanner.ScanPackageDependencies("");

            Assert.False(result.Success);
            Assert.Equal("Package path is empty", result.ErrorMessage);
            Assert.Empty(result.Dependencies);
        }

        [Fact]
        public void ScanPackageDependencies_NullPath_ReturnsErrorResult()
        {
            var scanner = new DependencyScanner();

            var result = scanner.ScanPackageDependencies(null);

            Assert.False(result.Success);
            Assert.Equal("Package path is empty", result.ErrorMessage);
        }

        [Fact]
        public void ScanPackageDependencies_NonExistentPath_ReturnsErrorResult()
        {
            var scanner = new DependencyScanner();

            var result = scanner.ScanPackageDependencies(@"z:\NonExistent\Package.var");

            Assert.False(result.Success);
            Assert.Equal("Package path does not exist", result.ErrorMessage);
        }

        [Fact]
        public void DependencyScanResult_SuccessProperty_TrueWhenNoError()
        {
            var result = new DependencyScanner.DependencyScanResult
            {
                ErrorMessage = ""
            };

            Assert.True(result.Success);
        }

        [Fact]
        public void DependencyScanResult_SuccessProperty_FalseWhenError()
        {
            var result = new DependencyScanner.DependencyScanResult
            {
                ErrorMessage = "Test error"
            };

            Assert.False(result.Success);
        }

        [Fact]
        public void DependencyScanResult_DefaultState_HasEmptyDependencies()
        {
            var result = new DependencyScanner.DependencyScanResult();

            Assert.NotNull(result.Dependencies);
            Assert.Empty(result.Dependencies);
            Assert.Equal("", result.ErrorMessage);
            Assert.True(result.Success);
        }
    }
}
