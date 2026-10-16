# ControllerDesktop 新对话续接卡

## 目标
把手柄桌面控制工具升级为可分享、可自动更新、未来可商业化的 Windows 软件。优先解决抖音，默认无驱动模式，驱动作为可选高级模块。

## 当前阶段
v2.0.0-alpha.1。WPF 主界面框架、中英文语言服务、配置模型、配置读写、SendInput 输入后端、驱动安装接口、GitHub 更新检查模块已经建立并编译通过。

## 本批次已完成
- v2 解决方案：App、Core、Input、Updater、Driver.Abstractions
- 主窗口导航框架：总览、按键映射、配置方案、高级驱动、设置
- 中英文切换和语言服务
- 配置模型：MappingProfile、ButtonBinding、StickSettings、AppRule
- 配置文件读写：ProfileStore
- 无驱动 SendInput 输入后端接口和实现
- 驱动安装抽象接口：IDriverInstaller、DriverPackageInfo
- GitHub Releases 更新检查模块
- Per-Monitor V2 DPI 清单
- 每次编译后自动刷新桌面续接卡和项目快照

## 下一步
1. 图形化改键编辑器
2. 按程序自动配置
3. 托盘常驻和诊断页
4. Inno Setup 安装包
5. 抖音高级驱动模块调研

## 重要决定
- Windows only
- 当前免费分享，未来商业化
- 主安装包不包含内核驱动
- 驱动高级模块单独下载、单独确认、可一键卸载
- 暂无代码签名证书，免费测试版接受 SmartScreen 警告

## 关键路径
- 开发目录：D:\ControllerDesktopDev
- 源码：D:\ControllerDesktopDev\src\ControllerDesktop
- GitHub：https://github.com/qq2426277189-tech/controller-desktop
- 旧个人版：D:\ControllerDesktop（不要删除或覆盖，除非用户确认）
