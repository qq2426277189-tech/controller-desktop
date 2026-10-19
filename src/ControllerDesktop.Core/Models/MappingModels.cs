using System.Collections.Generic;

namespace ControllerDesktop.Core.Models;

public enum MappingMode
{
    Desktop,
    GamePassThrough,
    Mouse
}

public enum ActionType
{
    None,
    MouseMove,
    MouseLeft,
    MouseRight,
    MouseMiddle,
    MouseX1,
    MouseX2,
    Wheel,
    Key,
    MediaKey
}

public sealed class ButtonBinding
{
    public string Button { get; set; } = "";
    public ActionType Action { get; set; } = ActionType.None;
    public string Value { get; set; } = "";
}

public sealed class StickSettings
{
    public int MouseSpeed { get; set; } = 8;
    public int Deadzone { get; set; } = 9000;
    public double Sensitivity { get; set; } = 1.0;
    public bool InvertY { get; set; }
}

public sealed class AppRule
{
    public string ProcessName { get; set; } = "";
    public MappingMode Mode { get; set; } = MappingMode.Desktop;
    public string ProfileName { get; set; } = "";
}

public sealed class MappingProfile
{
    public string Name { get; set; } = "Default";
    public StickSettings LeftStick { get; set; } = new();
    public StickSettings RightStick { get; set; } = new();
    public List<ButtonBinding> Bindings { get; set; } = new();
    public List<AppRule> AppRules { get; set; } = new();
}

public sealed class AppConfiguration
{
    public string CurrentProfile { get; set; } = "Default";
    public string Language { get; set; } = "zh-CN";
    public List<MappingProfile> Profiles { get; set; } = new();
}
