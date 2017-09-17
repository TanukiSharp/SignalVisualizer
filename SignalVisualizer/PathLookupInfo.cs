using System;
using System.Linq;

namespace SignalVisualizer
{
    public struct PathLookupInfo
    {
        public string Path { get; }
        public string[] SearchPatterns { get; }
        public bool IncludeSubDirectories { get; }

        public PathLookupInfo(string path)
            : this(path, false)
        {
        }

        public PathLookupInfo(string path, params string[] searchPatterns)
            : this(path, false, searchPatterns)
        {
        }

        public PathLookupInfo(string path, bool includeSubDirectories)
            : this(path, includeSubDirectories, new string[0])
        {
        }

        public PathLookupInfo(string path, bool includeSubDirectories, params string[] searchPatterns)
            : this()
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"Invalid '{nameof(path)}' argument.", nameof(path));

            Path = path;
            SearchPatterns = searchPatterns.DefaultIfEmpty("*.*").ToArray();
            IncludeSubDirectories = includeSubDirectories;
        }
    }
}
