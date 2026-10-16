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
        ["SettingsPlaceholder"] = "这里将显示路径、语言、更新和诊断设置。"
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
        ["SettingsPlaceholder"] = "Paths, language, updates and diagnostics will be configured here."
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
