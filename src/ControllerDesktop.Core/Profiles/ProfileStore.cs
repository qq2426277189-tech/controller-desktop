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

