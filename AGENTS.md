# ControllerDesktop Repository Instructions

本仓库是 ControllerDesktop v2 正式版。任何新 Codex 对话、任何协作者开始前先读：

1. docs/STATUS.md
2. docs/TODO.md
3. docs/DECISIONS.md
4. docs/HANDOFF.md
5. docs/ARCHITECTURE.md
6. docs/PROJECT.md

工作结束后必须更新 STATUS、TODO、DECISIONS（如有）、CHANGELOG、HANDOFF。

## 本地环境（本机）
- 开发根目录：D:\ControllerDesktopDev
- .NET SDK：D:\ControllerDesktopDev\app\dotnet.exe
- Inno Setup：D:\ControllerDesktopDev\tools\InnoSetup\ISCC.exe
- 旧个人版：D:\ControllerDesktop（不要删除或覆盖，除非用户确认）

## 构建
    $env:PATH = 'D:\ControllerDesktopDev\app;' + $env:PATH
    cd D:\ControllerDesktopDev\src\ControllerDesktop
    dotnet build ControllerDesktop.sln -c Debug

## 长期决定
- Windows only
- 中文 + English
- .NET 8 + WPF
- 当前免费分享，未来商业化
- 主安装包不包含内核驱动
- 驱动高级模块单独下载、单独确认安装、一键卸载
- 当前没有代码签名证书，免费测试版接受 SmartScreen 警告
- 自动更新计划使用 GitHub Releases + SHA-256 清单

## 下一阶段
按 docs/TODO.md 的 P0 项推进：中英文资源体系、主界面导航、配置模型和映射引擎。
