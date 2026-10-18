using System;
using System.IO;
using System.Text.Json;
using ControllerDesktop.Core.Models;

namespace ControllerDesktop.Core.Profiles;

public sealed class ProfileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _path;

    public ProfileStore(string? path = null)
    {
        _path = path ?? Path.Combine(AppPaths.ConfigDir, "profiles.json");
    }

    public string StorePath => _path;

    public AppConfiguration Load()
    {
        try
        {
            if (!File.Exists(_path)) return CreateDefault();
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<AppConfiguration>(json, Options);
            return data ?? CreateDefault();
        }
        catch
        {
            return CreateDefault();
        }
    }

    public void Save(AppConfiguration configuration)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_path)!);
        var json = JsonSerializer.Serialize(configuration, Options);
        File.WriteAllText(_path, json);
    }

    public static void EnsureDefaults(AppConfiguration configuration)
    {
        foreach (var profile in configuration.Profiles)
        {
            if (profile.Bindings.Count > 0) continue;
            profile.Bindings.AddRange(new[]
            {
                new ButtonBinding { Button = "A", Action = ActionType.MouseLeft },
                new ButtonBinding { Button = "X", Action = ActionType.MouseRight },
                new ButtonBinding { Button = "B", Action = ActionType.Key, Value = "ESC" },
                new ButtonBinding { Button = "Y", Action = ActionType.None },
                new ButtonBinding { Button = "START", Action = ActionType.Key, Value = "LWIN" },
                new ButtonBinding { Button = "BACK", Action = ActionType.Key, Value = "ALT+LEFT" },
                new ButtonBinding { Button = "LB", Action = ActionType.Key, Value = "MEDIA_PREV" },
                new ButtonBinding { Button = "RB", Action = ActionType.Key, Value = "MEDIA_NEXT" },
                new ButtonBinding { Button = "DPADUP", Action = ActionType.Key, Value = "UP" },
                new ButtonBinding { Button = "DPADDOWN", Action = ActionType.Key, Value = "DOWN" },
                new ButtonBinding { Button = "DPADLEFT", Action = ActionType.Key, Value = "LEFT" },
                new ButtonBinding { Button = "DPADRIGHT", Action = ActionType.Key, Value = "RIGHT" },
                new ButtonBinding { Button = "LT", Action = ActionType.Key, Value = "VOLUME_DOWN" },
                new ButtonBinding { Button = "RT", Action = ActionType.Key, Value = "VOLUME_UP" }
            });
        }
    }

    private static AppConfiguration CreateDefault()
    {
        var profile = new MappingProfile { Name = "Default" };
        return new AppConfiguration
        {
            CurrentProfile = profile.Name,
            Language = "zh-CN",
            Profiles = new() { profile }
        };
    }
}



