using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VPM.Models;

namespace VPM.Services
{
    /// <summary>
    /// Binary serialization cache for playlists
    /// Provides fast loading and persistence of user-created playlists
    /// </summary>
    public class PlaylistCache : IDisposable
    {
        private const int CACHE_VERSION = 1;
        private readonly string _cacheFilePath;
        private readonly string _cacheDirectory;
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private bool _disposed = false;

        public PlaylistCache(string cacheFolder = null)
        {
            if (string.IsNullOrEmpty(cacheFolder))
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                _cacheDirectory = Path.Combine(appDataPath, "VPM", "Cache");
            }
            else
            {
                _cacheDirectory = cacheFolder;
            }

            _cacheFilePath = Path.Combine(_cacheDirectory, "Playlists.cache");

            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Loads playlists from the binary cache
        /// </summary>
        public List<Playlist> LoadPlaylists()
        {
            if (!File.Exists(_cacheFilePath))
            {
                return new List<Playlist>();
            }

            try
            {
                using var stream = new FileStream(_cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new BinaryReader(stream);

                var version = reader.ReadInt32();
                if (version != CACHE_VERSION)
                {
                    return new List<Playlist>();
                }

                var count = reader.ReadInt32();
                if (count < 0 || count > 10000)
                {
                    return new List<Playlist>();
                }

                var playlists = new List<Playlist>();

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var playlist = ReadPlaylist(reader);
                        if (playlist != null)
                        {
                            playlists.Add(playlist);
                        }
                    }
                    catch
                    {
                    }
                }

                return playlists;
            }
            catch (Exception)
            {
                return new List<Playlist>();
            }
        }

        /// <summary>
        /// Saves playlists to the binary cache
        /// </summary>
        public bool SavePlaylists(List<Playlist> playlists)
        {
            if (playlists == null)
                return false;

            try
            {
                _cacheLock.EnterWriteLock();
                try
                {
                    Directory.CreateDirectory(_cacheDirectory);

                    using var stream = new FileStream(_cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var writer = new BinaryWriter(stream);

                    writer.Write(CACHE_VERSION);
                    writer.Write(playlists.Count);

                    foreach (var playlist in playlists)
                    {
                        try
                        {
                            WritePlaylist(writer, playlist);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    writer.Flush();
                    return true;
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Playlist ReadPlaylist(BinaryReader reader)
        {
            var playlist = new Playlist
            {
                Id = reader.ReadString(),
                Name = reader.ReadString(),
                Description = reader.ReadString(),
                IsEnabled = reader.ReadBoolean(),
                SortOrder = reader.ReadInt32(),
                UnloadOtherPackages = reader.ReadBoolean(),
                CreatedAt = new DateTime(reader.ReadInt64()),
                LastModifiedAt = new DateTime(reader.ReadInt64())
            };

            var packageCount = reader.ReadInt32();
            playlist.PackageKeys = new List<string>(packageCount);

            for (int i = 0; i < packageCount; i++)
            {
                playlist.PackageKeys.Add(reader.ReadString());
            }

            return playlist;
        }

        private void WritePlaylist(BinaryWriter writer, Playlist playlist)
        {
            writer.Write(playlist.Id ?? "");
            writer.Write(playlist.Name ?? "");
            writer.Write(playlist.Description ?? "");
            writer.Write(playlist.IsEnabled);
            writer.Write(playlist.SortOrder);
            writer.Write(playlist.UnloadOtherPackages);
            writer.Write(playlist.CreatedAt.Ticks);
            writer.Write(playlist.LastModifiedAt.Ticks);

            writer.Write(playlist.PackageKeys.Count);
            foreach (var packageKey in playlist.PackageKeys)
            {
                writer.Write(packageKey ?? "");
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _cacheLock?.Dispose();
            }
            catch
            {
            }

            _disposed = true;
        }
    }
}
