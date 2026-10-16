$ErrorActionPreference = 'Stop'
$env:PATH = 'D:\ControllerDesktopDev\app;' + $env:PATH
Set-Location 'D:\ControllerDesktopDev\src\ControllerDesktop'
dotnet run --project src\ControllerDesktop.App\ControllerDesktop.App.csproj
