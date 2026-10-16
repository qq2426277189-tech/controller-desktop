using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ControllerDesktop.Updater;

public sealed class UpdateInfo
{
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string ReleaseUrl { get; set; } = "";
    public bool IsNewer { get; set; }
}

public sealed class GitHubUpdateChecker
{
    private readonly HttpClient _http = new();
    private readonly string _repository;

    public GitHubUpdateChecker(string repository = "qq2426277189-tech/controller-desktop")
    {
        _repository = repository;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("ControllerDesktop-Updater");
    }

    public async Task<UpdateInfo> CheckLatestAsync(Version currentVersion, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.github.com/repos/{_repository}/releases/latest";
        var json = await _http.GetStringAsync(url, cancellationToken);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var tag = root.TryGetProperty("tag_name", out var tagElement) ? tagElement.GetString() ?? "" : "";
        var html = root.TryGetProperty("html_url", out var htmlElement) ? htmlElement.GetString() ?? "" : "";

        var info = new UpdateInfo
        {
            Version = tag.TrimStart('v'),
            ReleaseUrl = html
        };

        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.TryGetProperty("browser_download_url", out var download))
                {
                    info.DownloadUrl = download.GetString() ?? "";
                    break;
                }
            }
        }

        if (Version.TryParse(info.Version.Split('-')[0], out var latest))
        {
            info.IsNewer = latest > currentVersion;
        }

        return info;
    }
}
