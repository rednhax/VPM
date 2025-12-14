using System;
using System.Collections.Generic;
using System.IO;

namespace VPM.Services
{
    public static class SafeFileEnumerator
    {
        public static IEnumerable<string> EnumerateFiles(string rootPath, string searchPattern, bool recursive)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(searchPattern))
            {
                yield break;
            }

            var pending = new Stack<string>();
            pending.Push(rootPath);

            while (pending.Count > 0)
            {
                var current = pending.Pop();

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(current, searchPattern, SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    yield return file;
                }

                if (!recursive)
                {
                    continue;
                }

                IEnumerable<string> directories;
                try
                {
                    directories = Directory.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var dir in directories)
                {
                    pending.Push(dir);
                }
            }
        }
    }
}
