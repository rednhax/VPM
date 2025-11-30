namespace VPM.Services
{
    /// <summary>
    /// Shared formatting utilities to eliminate duplicate code across the codebase.
    /// </summary>
    public static class FormatHelper
    {
        private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>
        /// Formats bytes to human-readable string (e.g., "1.5 MB")
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            double size = bytes;
            int order = 0;
            while (size >= 1024 && order < SizeSuffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {SizeSuffixes[order]}";
        }

        /// <summary>
        /// Formats bytes with more decimal precision for detailed views
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
