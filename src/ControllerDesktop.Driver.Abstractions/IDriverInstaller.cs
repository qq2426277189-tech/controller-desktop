using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControllerDesktop.Driver.Abstractions;

public sealed class DriverPackageInfo
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public bool RequiresAdministrator { get; set; } = true;
}

public interface IDriverInstaller
{
    bool IsInstalled(string name);
    Task InstallAsync(DriverPackageInfo package, IProgress<string>? progress, CancellationToken cancellationToken = default);
    Task UninstallAsync(string name, CancellationToken cancellationToken = default);
}
