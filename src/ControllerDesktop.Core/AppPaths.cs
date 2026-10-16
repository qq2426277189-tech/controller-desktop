using System;
using System.IO;

namespace ControllerDesktop.Core;

public static class AppPaths
{
    public static string Root
    {
        get
        {
            var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            var parent = Directory.GetParent(exeDir);
            if (parent != null && string.Equals(Path.GetFileName(exeDir), "app", StringComparison.OrdinalIgnoreCase))
            {
                return parent.FullName;
            }

            return exeDir;
        }
    }

    public static string ConfigDir => Ensure("config");
    public static string LogsDir => Ensure("logs");
    public static string DataDir => Ensure("data");

    private static string Ensure(string name)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(path);
        return path;
    }
}
