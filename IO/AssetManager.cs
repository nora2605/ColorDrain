using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDrain.IO
{
    internal static class AssetManager
    {
        public static string GetPath(string asset)
        {
            string path = Path.Combine(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets"), asset);
            if (!Directory.Exists(Path.GetDirectoryName(path)!))
                throw new DirectoryNotFoundException($"Your operating system doesn't like this path ----> {Path.GetDirectoryName(path)}");
            return path;
        }

        public static string[] ListFiles(string directory, bool recursive=true)
        {
            return Directory.GetFiles(GetPath(directory), "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
    }
}
