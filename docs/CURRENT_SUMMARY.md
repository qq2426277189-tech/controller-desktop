# ControllerDesktop 新对话续接卡

## 目标
把手柄桌面控制工具升级为可分享、可自动更新、未来可商业化的 Windows 软件。优先解决抖音，默认无驱动模式，驱动作为可选高级模块。

## 当前阶段
- 已发布可用测试版：v0.5.1-beta
- 同时开发 v2.0.0-alpha.1 正式版框架
- 安装包：https://github.com/qq2426277189-tech/controller-desktop/releases/tag/v0.5.1-beta
- 默认安装到 D:\ControllerDesktop，不含驱动

## 部署状态
- v0.5.1-beta 已部署到 D:\ControllerDesktop
- 桌面快捷方式“手柄桌面控制”已指向软件本体图标
- 当前进程运行正常，controller-error.log 无错误
- 旧临时工作区已清理，旧个人版源码归档到 D:\ControllerDesktopDev\legacy

## 本批次已完成
- 当前可用版本 v0.5.1-beta 已打包并发布到 GitHub Releases
- v2.0.0-alpha.1：WPF 主界面框架、中英文语言服务、配置模型、ProfileStore、SendInput 后端、驱动接口、更新检查模块
- 每次编译后自动刷新桌面续接卡、源码快照和 ZIP 包
- 续接卡中文编码已修复

## 下一步
1. 图形化改键编辑器
2. 按程序自动配置
3. 托盘常驻和诊断页
4. Inno Setup 安装包（v2）
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
- 可用版本安装包：https://github.com/qq2426277189-tech/controller-desktop/releases/tag/v0.5.1-beta
- GitHub：https://github.com/qq2426277189-tech/controller-desktop
- 旧个人版：D:\ControllerDesktop（不要删除或覆盖，除非用户确认）

