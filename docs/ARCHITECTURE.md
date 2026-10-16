# 架构草案

## 结构
- ControllerDesktop.App：WPF 图形界面、托盘、中英文资源
- ControllerDesktop.Core：配置、映射模型、模式切换、日志
- ControllerDesktop.Input：输入后端接口与 SendInput 实现
- ControllerDesktop.Updater：GitHub Releases 检查和更新
- ControllerDesktop.Driver.Abstractions：驱动安装接口，不含驱动本体

## 插件接口
IInputBackend
- Name
- Initialize
- Poll / SendAction
- Shutdown

IDriverInstaller
- IsInstalled
- InstallWithConsent
- Uninstall
- GetDownloadInfo

## 默认行为
- 无驱动 SendInput 后端。
- 游戏和反作弊程序默认透传。
- 驱动后端必须手动启用。
