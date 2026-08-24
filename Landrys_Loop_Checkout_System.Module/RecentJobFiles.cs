using System;
using System.Collections.Generic;
using System.IO;

namespace Landrys_Loop_Checkout_System.Module
{
    public static class RecentJobFiles
    {
        public const int MaxCount = 10;
        public const string ClearCommand = "__clear__";

        private static readonly object Sync = new object();

        private static string StorePath
        {
            get { return Path.Combine(JobDatabase.AppDataFolder, "recent-jobs.txt"); }
        }

        public static IReadOnlyList<string> GetAll()
        {
            lock (Sync)
            {
                return LoadCore();
            }
        }

        public static IReadOnlyList<string> GetExisting()
        {
            List<string> existing = new List<string>();
            foreach (string path in GetAll())
            {
                if (File.Exists(path))
                {
                    existing.Add(path);
                }
            }
            return existing;
        }

        public static string FindLastExisting()
        {
            IReadOnlyList<string> existing = GetExisting();
            return existing.Count > 0 ? existing[0] : null;
        }

        public static string GetInitialDirectory(string currentPath)
        {
            string directory = GetDirectoryIfExists(currentPath);
            if (directory != null)
            {
                return directory;
            }

            foreach (string path in GetExisting())
            {
                directory = GetDirectoryIfExists(path);
                if (directory != null)
                {
                    return directory;
                }
            }

            return null;
        }

        public static void Add(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string fullPath = Path.GetFullPath(filePath);
            lock (Sync)
            {
                List<string> paths = LoadCore();
                paths.RemoveAll(path => PathsEqual(path, fullPath));
                paths.Insert(0, fullPath);
                if (paths.Count > MaxCount)
                {
                    paths.RemoveRange(MaxCount, paths.Count - MaxCount);
                }
                SaveCore(paths);
            }
        }

        public static void Remove(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string fullPath = Path.GetFullPath(filePath);
            lock (Sync)
            {
                List<string> paths = LoadCore();
                paths.RemoveAll(path => PathsEqual(path, fullPath));
                SaveCore(paths);
            }
        }

        public static void Clear()
        {
            lock (Sync)
            {
                SaveCore(new List<string>());
            }
        }

        private static List<string> LoadCore()
        {
            List<string> paths = new List<string>();
            string storePath = StorePath;
            if (!File.Exists(storePath))
            {
                return paths;
            }

            foreach (string line in File.ReadAllLines(storePath))
            {
                string path = line.Trim();
                if (path.Length == 0)
                {
                    continue;
                }
                try
                {
                    path = Path.GetFullPath(path);
                }
                catch
                {
                    continue;
                }
                if (!paths.Exists(existing => PathsEqual(existing, path)))
                {
                    paths.Add(path);
                }
            }
            return paths;
        }

        private static void SaveCore(List<string> paths)
        {
            Directory.CreateDirectory(JobDatabase.AppDataFolder);
            File.WriteAllLines(StorePath, paths);
        }

        private static string GetDirectoryIfExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                string directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
                return !string.IsNullOrEmpty(directory) && Directory.Exists(directory) ? directory : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}
