# 变更记录

## 2026-10-19 托盘菜单与诊断页
- 新增 DiagnosticsPage：版本、输入后端、运行时长、手柄实时状态、前台程序与全屏判定、各类目录路径、日志尾部 12 行。
- 诊断页可刷新，并提供「打开日志目录」「打开配置目录」按钮。
- 托盘右键菜单：显示主窗口 / 打开配置文件 / 打开日志目录 / 退出；双击托盘图标恢复主窗口。
- 主界面导航新增「诊断」页（第 8 项）。
- Controller111.png 加入 csproj Resource，确保打包后可加载底图。
- dotnet build 通过，0 警告、0 错误。

## 2026-10-19 映射页重做 + 组合键 + 侧键
- 映射页改为三栏布局：按键列表 / 手柄图示 / 右侧配置面板。
- 底图换为 Assets/Controller111.png（639x550），18 个按键锚点按图片实测标定。
- 新增 KeyCaptureDialog，组合键可捕获主键；支持 Ctrl/Shift/Alt/Win 修饰键与预览。
- 动作类型新增 MouseX1/MouseX2（鼠标侧键），IInputBackend、SendInputBackend、BindingEngine 同步实现 XBUTTON1/2。
- dotnet build 通过，0 警告、0 错误。

## 2026-10-17 v2.0.0-alpha.1
- 建立 WPF 主界面导航框架。
- 建立中英文语言服务。
- 建立配置模型和 ProfileStore。
- 建立 SendInput 输入后端。
- 建立驱动安装接口。
- 建立 GitHub Releases 更新检查模块。
- 每次编译后自动刷新桌面续接卡、源码快照和 ZIP 包。
- 修复续接卡中文编码问题。
- 公开仓库推送初始骨架和项目文档。

## 2026-10-17 v0.5.1-beta
- 生成可用的安装包并发布到 GitHub Releases。

## 2026-10-17 部署 v0.5.1-beta
- 静默部署安装包到 D:\ControllerDesktop。
