using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace VPM.Services
{
    /// <summary>
    /// Helper class to provide a consistent interface for SharpZipLib ZIP operations.
    /// Simplifies migration from System.IO.Compression.ZipArchive.
    /// </summary>
    public static class SharpZipLibHelper
    {
        /// <summary>
        /// Opens a ZIP file for reading
        /// </summary>
        public static ZipFile OpenForRead(string filePath)
        {
            return new ZipFile(filePath);
        }

        /// <summary>
        /// Opens a ZIP file stream for reading
        /// </summary>
        public static ZipFile OpenStreamForRead(Stream stream)
        {
            return new ZipFile(stream);
        }

        /// <summary>
        /// Creates a new ZIP file at the specified path
        /// </summary>
        public static ZipOutputStream CreateZipFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var zipStream = new ZipOutputStream(fileStream);
            zipStream.SetLevel(9); // Maximum compression
            return zipStream;
        }

        /// <summary>
        /// Creates a ZIP stream for writing to a stream
        /// </summary>
        public static ZipOutputStream CreateZipStream(Stream stream)
        {
            var zipStream = new ZipOutputStream(stream);
            zipStream.SetLevel(9); // Maximum compression
            return zipStream;
        }

        /// <summary>
        /// Opens a ZIP file for updating (reading and writing)
        /// </summary>
        public static ZipFile OpenForUpdate(string filePath)
        {
            return new ZipFile(filePath);
        }

        /// <summary>
        /// Gets all entries from a ZIP file
        /// </summary>
        public static List<ZipEntry> GetAllEntries(ZipFile zipFile)
        {
            var entries = new List<ZipEntry>();
            foreach (ZipEntry entry in zipFile)
            {
                entries.Add(entry);
            }
            return entries;
        }

        /// <summary>
        /// Finds an entry by name (case-insensitive)
        /// </summary>
        public static ZipEntry FindEntry(ZipFile zipFile, string entryName)
        {
            foreach (ZipEntry entry in zipFile)
            {
                if (entry.Name.Equals(entryName, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return null;
        }

        /// <summary>
        /// Finds an entry by full path (case-insensitive)
        /// </summary>
        public static ZipEntry FindEntryByPath(ZipFile zipFile, string fullPath)
        {
            foreach (ZipEntry entry in zipFile)
            {
                if (entry.Name.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return null;
        }

        /// <summary>
        /// Reads the content of a ZIP entry as a string
        /// </summary>
        public static string ReadEntryAsString(ZipFile zipFile, ZipEntry entry)
        {
            using (var stream = zipFile.GetInputStream(entry))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads the content of a ZIP entry as bytes
        /// </summary>
        public static byte[] ReadEntryAsBytes(ZipFile zipFile, ZipEntry entry)
        {
            using (var stream = zipFile.GetInputStream(entry))
            {
                var buffer = new byte[entry.Size];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Reads a ZIP entry into a provided buffer
        /// </summary>
        public static int ReadEntryIntoBuffer(ZipFile zipFile, ZipEntry entry, byte[] buffer, int offset, int count)
        {
            using (var stream = zipFile.GetInputStream(entry))
            {
                return stream.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Writes a string entry to a ZIP output stream
        /// </summary>
        public static void WriteStringEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
            
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                CompressionMethod = CompressionMethod.Deflated,
                Size = data.Length
            };
            
            // Calculate CRC for the data
            var crc = new ICSharpCode.SharpZipLib.Checksum.Crc32();
            crc.Update(data);
            entry.Crc = crc.Value;
            
            zipStream.PutNextEntry(entry);
            zipStream.Write(data, 0, data.Length);
            zipStream.CloseEntry();
        }

        /// <summary>
        /// Writes a byte array entry to a ZIP output stream
        /// </summary>
        public static void WriteByteEntry(ZipOutputStream zipStream, string entryName, byte[] data)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                CompressionMethod = CompressionMethod.Deflated,
                Size = data.Length
            };
            
            // Calculate CRC for the data
            var crc = new ICSharpCode.SharpZipLib.Checksum.Crc32();
            crc.Update(data);
            entry.Crc = crc.Value;
            
            zipStream.PutNextEntry(entry);
            zipStream.Write(data, 0, data.Length);
            zipStream.CloseEntry();
        }

        /// <summary>
        /// Writes a file entry to a ZIP output stream
        /// </summary>
        public static void WriteFileEntry(ZipOutputStream zipStream, string entryName, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var entry = new ZipEntry(entryName)
            {
                DateTime = fileInfo.LastWriteTime,
                CompressionMethod = CompressionMethod.Deflated,
                Size = fileInfo.Length
            };
            zipStream.PutNextEntry(entry);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(zipStream);
            }

            zipStream.CloseEntry();
        }

        /// <summary>
        /// Writes a stream entry to a ZIP output stream
        /// </summary>
        public static void WriteStreamEntry(ZipOutputStream zipStream, string entryName, Stream sourceStream, DateTime? lastWriteTime = null)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = lastWriteTime ?? DateTime.Now,
                CompressionMethod = CompressionMethod.Deflated
            };
            zipStream.PutNextEntry(entry);
            sourceStream.CopyTo(zipStream);
            zipStream.CloseEntry();
        }

        /// <summary>
        /// Filters entries by extension
        /// </summary>
        public static List<ZipEntry> FilterByExtension(List<ZipEntry> entries, params string[] extensions)
        {
            var extensionSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return entries
                .Where(e => !e.IsDirectory && extensionSet.Contains(Path.GetExtension(e.Name)))
                .ToList();
        }

        /// <summary>
        /// Filters entries by path prefix
        /// </summary>
        public static List<ZipEntry> FilterByPath(List<ZipEntry> entries, string pathPrefix)
        {
            return entries
                .Where(e => e.Name.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase) && !e.IsDirectory)
                .ToList();
        }

        /// <summary>
        /// Gets the uncompressed size of an entry
        /// </summary>
        public static long GetEntrySize(ZipEntry entry)
        {
            return entry.Size;
        }

        /// <summary>
        /// Gets the compressed size of an entry
        /// </summary>
        public static long GetEntryCompressedSize(ZipEntry entry)
        {
            return entry.CompressedSize;
        }
    }
}
