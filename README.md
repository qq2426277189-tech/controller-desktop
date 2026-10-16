# ControllerDesktop

Windows gamepad-to-desktop / remapping software.

当前状态：v2 初始骨架，免费测试版，未来商业化。

- Windows only
- 中文 / English
- .NET 8 + WPF
- 默认无驱动输入后端
- 可选高级驱动模块：单独下载、确认安装、一键卸载

项目文档（开发目录）：

- AGENTS.md
- docs/STATUS.md
- docs/TODO.md
- docs/DECISIONS.md
- docs/HANDOFF.md
- docs/ARCHITECTURE.md

本地构建：

    $env:PATH = 'D:\ControllerDesktopDev\app;' + $env:PATH
    cd D:\ControllerDesktopDev\src\ControllerDesktop
    dotnet build

发布前请阅读 docs/HANDOFF.md。
