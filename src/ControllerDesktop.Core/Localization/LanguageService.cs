using System;
using System.Collections.Generic;

namespace ControllerDesktop.Core.Localization;

public sealed class LanguageService
{
    private static readonly Dictionary<string, string> Zh = new()
    {
        ["AppTitle"] = "ControllerDesktop",
        ["Dashboard"] = "总览",
        ["Mapping"] = "按键映射",
        ["Profiles"] = "配置方案",
        ["Driver"] = "高级驱动",
        ["Settings"] = "设置",
        ["Language"] = "语言",
        ["Status"] = "状态",
        ["CurrentProfile"] = "当前配置",
        ["NoProfile"] = "暂无配置",
        ["NewProfile"] = "新建配置",
        ["Save"] = "保存",
        ["Load"] = "加载",
        ["Delete"] = "删除",
        ["Apply"] = "应用",
        ["Version"] = "版本",
        ["FrameworkReady"] = "v2 程序框架已启动",
        ["MappingPlaceholder"] = "改键编辑器将在下一阶段实现。",
        ["ProfilesPlaceholder"] = "这里将管理不同程序和不同游戏的按键方案。",
        ["DriverPlaceholder"] = "高级驱动模块需要单独下载、确认安装，并可一键卸载。",
        ["SettingsPlaceholder"] = "这里将显示路径、语言、更新和诊断设置。",
        ["BindingsHeader"] = "按键绑定",
        ["BindingsHint"] = "在手柄按键列表中填写按键名，选择动作类型，目标填写键名或方向。",
        ["GamepadButton"] = "手柄按键",
        ["Action"] = "动作",
        ["Target"] = "目标",
        ["StickSettingsHeader"] = "摇杆设置（左摇杆）",
        ["MouseSpeed"] = "鼠标速度",
        ["Deadzone"] = "死区",
        ["Sensitivity"] = "灵敏度",
        ["InvertY"] = "反转 Y 轴",
        ["ResetChanges"] = "还原",
        ["SavedProfile"] = "已保存配置 {0}（{1}）",
        ["GamepadTest"] = "手柄测试",
        ["GamepadConnected"] = "手柄已连接",
        ["Buttons"] = "按下的按键",
        ["TestHint"] = "插上手柄并按几下按键；如果没反应，请把手柄背面的模式开关切到 X（XInput）模式。",
    };

    private static readonly Dictionary<string, string> En = new()
    {
        ["AppTitle"] = "ControllerDesktop",
        ["Dashboard"] = "Dashboard",
        ["Mapping"] = "Mapping",
        ["Profiles"] = "Profiles",
        ["Driver"] = "Advanced Driver",
        ["Settings"] = "Settings",
        ["Language"] = "Language",
        ["Status"] = "Status",
        ["CurrentProfile"] = "Current profile",
        ["NoProfile"] = "No profile",
        ["NewProfile"] = "New profile",
        ["Save"] = "Save",
        ["Load"] = "Load",
        ["Delete"] = "Delete",
        ["Apply"] = "Apply",
        ["Version"] = "Version",
        ["FrameworkReady"] = "v2 application framework started",
        ["MappingPlaceholder"] = "The mapping editor will be implemented in the next phase.",
        ["ProfilesPlaceholder"] = "Manage per-application and per-game profiles here.",
        ["DriverPlaceholder"] = "The advanced driver module is downloaded separately and can be uninstalled with one click.",
        ["SettingsPlaceholder"] = "Paths, language, updates and diagnostics will be configured here.",
        ["BindingsHeader"] = "Button bindings",
        ["BindingsHint"] = "Enter a gamepad button, choose an action type, and set the target key or direction.",
        ["GamepadButton"] = "Gamepad button",
        ["Action"] = "Action",
        ["Target"] = "Target",
        ["StickSettingsHeader"] = "Stick settings (left stick)",
        ["MouseSpeed"] = "Mouse speed",
        ["Deadzone"] = "Deadzone",
        ["Sensitivity"] = "Sensitivity",
        ["InvertY"] = "Invert Y axis",
        ["ResetChanges"] = "Reset",
        ["SavedProfile"] = "Saved profile {0} ({1})",
        ["GamepadTest"] = "Gamepad test",
        ["GamepadConnected"] = "Gamepad connected",
        ["Buttons"] = "Buttons down",
        ["TestHint"] = "Plug in the controller and press buttons; if nothing shows, switch the mode switch on the back to X (XInput) mode.",
    };

    public string CurrentLanguage { get; private set; } = "zh-CN";

    public event EventHandler? LanguageChanged;

    public void SetLanguage(string language)
    {
        CurrentLanguage = string.Equals(language, "en-US", StringComparison.OrdinalIgnoreCase) ? "en-US" : "zh-CN";
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string T(string key)
    {
        var table = CurrentLanguage == "en-US" ? En : Zh;
        return table.TryGetValue(key, out var value) ? value : key;
    }
}

