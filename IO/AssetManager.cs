using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDrain.IO
{
    internal static class AssetManager
    {
        static Dictionary<string, string> strings = [];

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

        public static void LoadLanguage(string language)
        {
            var languages = Directory.GetFileSystemEntries(GetPath("Languages"), "", SearchOption.TopDirectoryOnly);
            if (languages.Any(p => p.EndsWith(language)))
            {
                var files = ListFiles($"Languages/{language}");
                strings = files
                    .Where(f => f.EndsWith(".strings")) // kv files, rest is dialogue or something
                    .Select(f => (realm: Path.GetFileNameWithoutExtension(f), lines: File.ReadAllLines(f)))
                    .Select(f => f.lines.Select(l => l.Split("===")).Select(l => KeyValuePair.Create($"{f.realm}.{l[0]}", l[1])))
                    .SelectMany(e => e)
                    .ToDictionary();
            }
        }

        public static string T(string key)
        {
            var success = strings.TryGetValue(key, out string? translation);
            return success ? translation! : $"<Untranslated: {key}>";
        }
    }
}
