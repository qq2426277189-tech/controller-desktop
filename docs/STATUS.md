# 当前状态

更新时间：2026-10-19

## 已发布可用版本
- v0.5.1-beta：https://github.com/qq2426277189-tech/controller-desktop/releases/tag/v0.5.1-beta
- 安装包 SHA-256：D32FBCD841813497DB64E6197FD7AB736502112C825786B29B9A25EA84FFA5FA
- 默认安装到 D:\ControllerDesktop，不含驱动

## 已完成
- 开发目录 D:\ControllerDesktopDev：app/tools/src/build/docs/data
- .NET SDK 8.0.426：D:\ControllerDesktopDevpp
- Inno Setup 6.7.3：D:\ControllerDesktopDev	ools\InnoSetup
- v2 解决方案和模块：App、Core、Input、Updater、Driver.Abstractions
- v2.0.0-alpha.1 编译通过，0 警告、0 错误
- WPF 主界面导航框架、中英文语言服务、配置模型和 ProfileStore
- SendInput 输入后端（含鼠标左/右/中键、侧键1/2、滚轮、键盘组合键）
- 驱动安装接口、GitHub 更新检查模块、Per-Monitor V2 DPI 清单
- 公开 GitHub 仓库：https://github.com/qq2426277189-tech/controller-desktop
- 映射页重做：左侧按键列表 + 中间手柄图示 + 右侧配置面板
- 映射页底图换为 Assets/Controller111.png（639x550），按图片实测坐标标定 18 个按键
- 新增 KeyCaptureDialog：组合键主键捕获（Esc 取消，仅捕获主键）
- 组合键编辑：Ctrl/Shift/Alt/Win 修饰键 + 主键 + 实时预览
- 动作类型扩展：鼠标侧键1/2（MouseX1/MouseX2），输入层同步实现 XBUTTON1/2
- 本次 dotnet build 通过，0 警告、0 错误
- 新增诊断页（导航第 8 项）：运行状态、手柄实时状态、前台程序、路径、最近日志，可一键打开配置/日志目录
- 托盘菜单补齐：显示主窗口、打开配置文件、打开日志目录、退出；双击托盘图标回到主窗口
- 右键菜单与网站图标等界面文本仍为中文，中英文资源待补全

## 未完成
- 按程序自动配置（目前是硬编码判定，未做可视化规则编辑器）
- 配置导入导出、实时按键测试页
- 安装包、自动更新发布
- 抖音高级驱动模块
- GitHub Actions CI（token 缺 workflow 权限）
- 代码签名（商业化前再补）

## 当前优先目标
按 TODO 的 P1 项继续：按程序自动配置（可视化规则编辑）。
