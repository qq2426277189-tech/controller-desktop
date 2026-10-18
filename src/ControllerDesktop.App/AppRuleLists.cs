using System;
using System.Collections.Generic;
using System.IO;
using ControllerDesktop.Core;

namespace ControllerDesktop.App
{
    /// <summary>Ported from legacy: per-app rule lists (txt files under config).</summary>
    internal static class AppRuleLists
    {
        public static readonly HashSet<string> PassthroughGames = new(StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> MouseGames = new(StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> DesktopFullscreen = new(StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> DirectInputApps = new(StringComparer.OrdinalIgnoreCase);

        public static void Load(string configDir)
        {
            LoadList(Path.Combine(configDir, "passthrough-games.txt"), PassthroughGames);
            LoadList(Path.Combine(configDir, "mouse-games.txt"), MouseGames);
            LoadList(Path.Combine(configDir, "desktop-fullscreen.txt"), DesktopFullscreen);
            LoadList(Path.Combine(configDir, "direct-input-apps.txt"), DirectInputApps);
        }

        private static void LoadList(string path, HashSet<string> target)
        {
            target.Clear();
            try
            {
                if (!File.Exists(path)) return;
                foreach (var line in File.ReadAllLines(path))
                {
                    var s = line.Trim();
                    if (s.Length == 0 || s.StartsWith("#")) continue;
                    target.Add(s);
                }
            }
            catch { }
        }

        public static void SaveList(string fileName, HashSet<string> source)
        {
            try
            {
                var path = Path.Combine(AppPaths.ConfigDir, fileName);
                var lines = new List<string> { "# 每行一个程序名（不需要 .exe）。保存后立即生效。" };
                lines.AddRange(source);
                File.WriteAllLines(path, lines);
            }
            catch { }
        }
    }
}


