$ErrorActionPreference = 'Stop'
$env:PATH = 'D:\ControllerDesktopDev\app;' + $env:PATH
Set-Location 'D:\ControllerDesktopDev\src\ControllerDesktop'
dotnet build ControllerDesktop.sln -c Debug
