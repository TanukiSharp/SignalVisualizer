using System;
using System.IO;

namespace SignalVisualizer
{
    public static class Utility
    {
        public static string UsePath(string relativePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath = Path.GetFullPath(Path.Combine(basePath, relativePath));

            if (Directory.Exists(absolutePath) == false)
                Directory.CreateDirectory(absolutePath);

            return absolutePath;
        }
    }
}
