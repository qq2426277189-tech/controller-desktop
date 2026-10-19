# 交接说明

## 新对话第一步
1. 打开 D:\ControllerDesktopDev。
2. 阅读 AGENTS.md。
3. 阅读 docs/STATUS.md、TODO.md、DECISIONS.md、ARCHITECTURE.md、CHANGELOG.md。
4. 在 D:\ControllerDesktopDev\src\ControllerDesktop 运行 `git pull` 和 `dotnet build`。

## 环境
- .NET SDK：D:\ControllerDesktopDevpp\dotnet.exe
- Inno Setup：D:\ControllerDesktopDev	ools\InnoSetup\ISCC.exe
- GitHub：https://github.com/qq2426277189-tech/controller-desktop

## 下一步
- P1：按程序自动配置（桌面/游戏/鼠标模式的可视化规则编辑），然后是配置导入导出、实时按键测试页。
- 诊断页在导航第 8 项，可直接查看手柄、前台程序和日志，排查问题时先用它。
- 映射页底图为 Controller111.png，18 个键位；如需增加 HOME/L3/R3 等独立键，要先有带对应按键的底图再标定。

## 不要做的事
- 不要默认安装驱动或运行未知安装包。
- 不要把开发工具装到 C 盘。
- 不要删除 D:\ControllerDesktop 的现有个人版，除非用户确认。
- 不要把未验证的计划写成已完成。
