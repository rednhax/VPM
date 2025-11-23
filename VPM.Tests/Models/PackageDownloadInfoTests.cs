using Xunit;
using VPM.Models;
using System.Collections.Generic;

namespace VPM.Tests.Models
{
    public class PackageDownloadInfoTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaults()
        {
            var info = new PackageDownloadInfo();

            Assert.Equal("", info.PackageName);
            Assert.Equal("", info.DownloadUrl);
            Assert.NotNull(info.HubUrls);
            Assert.Empty(info.HubUrls);
            Assert.NotNull(info.PdrUrls);
            Assert.Empty(info.PdrUrls);
        }

        [Fact]
        public void PackageName_CanBeSet()
        {
            var info = new PackageDownloadInfo();

            info.PackageName = "Creator.Package.1";

            Assert.Equal("Creator.Package.1", info.PackageName);
        }

        [Fact]
        public void DownloadUrl_CanBeSet()
        {
            var info = new PackageDownloadInfo();

            info.DownloadUrl = "https://example.com/package.var";

            Assert.Equal("https://example.com/package.var", info.DownloadUrl);
        }

        [Fact]
        public void HubUrls_CanBeSet()
        {
            var info = new PackageDownloadInfo();
            var urls = new List<string> { "https://hub1.com", "https://hub2.com" };

            info.HubUrls = urls;

            Assert.Equal(urls, info.HubUrls);
            Assert.Equal(2, info.HubUrls.Count);
        }

        [Fact]
        public void PdrUrls_CanBeSet()
        {
            var info = new PackageDownloadInfo();
            var urls = new List<string> { "https://pdr1.com", "https://pdr2.com" };

            info.PdrUrls = urls;

            Assert.Equal(urls, info.PdrUrls);
            Assert.Equal(2, info.PdrUrls.Count);
        }

        [Fact]
        public void HubUrls_CanAddUrls()
        {
            var info = new PackageDownloadInfo();

            info.HubUrls.Add("https://hub1.com");
            info.HubUrls.Add("https://hub2.com");

            Assert.Equal(2, info.HubUrls.Count);
            Assert.Contains("https://hub1.com", info.HubUrls);
            Assert.Contains("https://hub2.com", info.HubUrls);
        }

        [Fact]
        public void PdrUrls_CanAddUrls()
        {
            var info = new PackageDownloadInfo();

            info.PdrUrls.Add("https://pdr1.com");
            info.PdrUrls.Add("https://pdr2.com");

            Assert.Equal(2, info.PdrUrls.Count);
            Assert.Contains("https://pdr1.com", info.PdrUrls);
            Assert.Contains("https://pdr2.com", info.PdrUrls);
        }

        [Fact]
        public void AllProperties_CanBeSetTogether()
        {
            var info = new PackageDownloadInfo
            {
                PackageName = "Creator.Package.1",
                DownloadUrl = "https://example.com/package.var",
                HubUrls = new List<string> { "https://hub.com" },
                PdrUrls = new List<string> { "https://pdr.com" }
            };

            Assert.Equal("Creator.Package.1", info.PackageName);
            Assert.Equal("https://example.com/package.var", info.DownloadUrl);
            Assert.Single(info.HubUrls);
            Assert.Single(info.PdrUrls);
        }

        [Fact]
        public void HubUrls_EmptyList_IsValid()
        {
            var info = new PackageDownloadInfo
            {
                PackageName = "Creator.Package.1",
                DownloadUrl = "https://example.com/package.var"
            };

            Assert.Empty(info.HubUrls);
            Assert.NotNull(info.HubUrls);
        }

        [Fact]
        public void PdrUrls_EmptyList_IsValid()
        {
            var info = new PackageDownloadInfo
            {
                PackageName = "Creator.Package.1",
                DownloadUrl = "https://example.com/package.var"
            };

            Assert.Empty(info.PdrUrls);
            Assert.NotNull(info.PdrUrls);
        }

        [Fact]
        public void HubUrls_MultipleUrls_MaintainsOrder()
        {
            var info = new PackageDownloadInfo();
            var urls = new List<string>
            {
                "https://hub1.com",
                "https://hub2.com",
                "https://hub3.com"
            };

            info.HubUrls = urls;

            Assert.Equal("https://hub1.com", info.HubUrls[0]);
            Assert.Equal("https://hub2.com", info.HubUrls[1]);
            Assert.Equal("https://hub3.com", info.HubUrls[2]);
        }

        [Fact]
        public void PdrUrls_MultipleUrls_MaintainsOrder()
        {
            var info = new PackageDownloadInfo();
            var urls = new List<string>
            {
                "https://pdr1.com",
                "https://pdr2.com",
                "https://pdr3.com"
            };

            info.PdrUrls = urls;

            Assert.Equal("https://pdr1.com", info.PdrUrls[0]);
            Assert.Equal("https://pdr2.com", info.PdrUrls[1]);
            Assert.Equal("https://pdr3.com", info.PdrUrls[2]);
        }

        [Fact]
        public void PackageName_EmptyString_IsValid()
        {
            var info = new PackageDownloadInfo
            {
                PackageName = ""
            };

            Assert.Equal("", info.PackageName);
        }

        [Fact]
        public void DownloadUrl_EmptyString_IsValid()
        {
            var info = new PackageDownloadInfo
            {
                DownloadUrl = ""
            };

            Assert.Equal("", info.DownloadUrl);
        }

        [Fact]
        public void CompleteDownloadInfo_AllUrlTypes_StoresCorrectly()
        {
            var info = new PackageDownloadInfo
            {
                PackageName = "Creator.CompletePackage.5",
                DownloadUrl = "https://primary.com/package.var",
                HubUrls = new List<string>
                {
                    "https://hub1.com/package.var",
                    "https://hub2.com/package.var"
                },
                PdrUrls = new List<string>
                {
                    "https://pixeldrain1.com/package.var",
                    "https://pixeldrain2.com/package.var"
                }
            };

            Assert.Equal("Creator.CompletePackage.5", info.PackageName);
            Assert.Equal("https://primary.com/package.var", info.DownloadUrl);
            Assert.Equal(2, info.HubUrls.Count);
            Assert.Equal(2, info.PdrUrls.Count);
        }

        [Fact]
        public void HubUrls_RemoveUrl_Works()
        {
            var info = new PackageDownloadInfo();
            info.HubUrls.Add("https://hub1.com");
            info.HubUrls.Add("https://hub2.com");

            info.HubUrls.Remove("https://hub1.com");

            Assert.Single(info.HubUrls);
            Assert.Equal("https://hub2.com", info.HubUrls[0]);
        }

        [Fact]
        public void PdrUrls_RemoveUrl_Works()
        {
            var info = new PackageDownloadInfo();
            info.PdrUrls.Add("https://pdr1.com");
            info.PdrUrls.Add("https://pdr2.com");

            info.PdrUrls.Remove("https://pdr1.com");

            Assert.Single(info.PdrUrls);
            Assert.Equal("https://pdr2.com", info.PdrUrls[0]);
        }

        [Fact]
        public void HubUrls_Clear_RemovesAllUrls()
        {
            var info = new PackageDownloadInfo();
            info.HubUrls.Add("https://hub1.com");
            info.HubUrls.Add("https://hub2.com");

            info.HubUrls.Clear();

            Assert.Empty(info.HubUrls);
        }

        [Fact]
        public void PdrUrls_Clear_RemovesAllUrls()
        {
            var info = new PackageDownloadInfo();
            info.PdrUrls.Add("https://pdr1.com");
            info.PdrUrls.Add("https://pdr2.com");

            info.PdrUrls.Clear();

            Assert.Empty(info.PdrUrls);
        }
    }
}
