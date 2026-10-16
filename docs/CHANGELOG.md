# 变更记录

## 2026-10-17
- 创建 D:\ControllerDesktopDev 开发目录和分类结构。
- 安装 .NET SDK 8.0.426 到 D:\ControllerDesktopDev\app。
- 安装 Inno Setup 6.7.3 到 D:\ControllerDesktopDev\tools\InnoSetup。
- 创建 v2 .NET 8 解决方案：App、Core、Input、Updater、Driver.Abstractions。
- `dotnet build ControllerDesktop.sln -c Debug` 成功，0 警告、0 错误。
- 加入最小 WPF 界面和中英文切换按钮。
- 加入 PerMonitorV2 DPI 清单。
- 创建公开仓库 https://github.com/qq2426277189-tech/controller-desktop 并推送初始骨架。
- GitHub Actions 因 token workflow 权限缺失暂未推送。
