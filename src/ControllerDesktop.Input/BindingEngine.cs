using ControllerDesktop.Core.Models;

namespace ControllerDesktop.Input;

public sealed class BindingEngine
{
    private readonly IInputBackend _backend;

    public BindingEngine(IInputBackend backend)
    {
        _backend = backend;
    }

    public void Apply(MappingProfile profile, GamepadState previous, GamepadState current)
    {
        foreach (var binding in profile.Bindings)
        {
            if (string.IsNullOrWhiteSpace(binding.Button)) continue;
            if (!TryGetButton(binding.Button.Trim(), out var button)) continue;

            var wasDown = previous.Has(button);
            var isDown = current.Has(button);
            if (wasDown == isDown) continue;

            switch (binding.Action)
            {
                case ActionType.MouseLeft:
                    _backend.MouseLeft(isDown);
                    break;
                case ActionType.MouseRight:
                    _backend.MouseRight(isDown);
                    break;
                case ActionType.Wheel:
                    if (isDown && int.TryParse(binding.Value, out var delta))
                        _backend.Wheel(delta);
                    break;
                case ActionType.Key:
                    if (isDown) _backend.KeyTap(binding.Value);
                    break;
            }
        }

        ApplyStick(profile.RightStick, current.RightX, current.RightY);
    }

    public void ApplyLeftStick(MappingProfile profile, GamepadState current)
    {
        ApplyStick(profile.LeftStick, current.LeftX, current.LeftY);
    }

    private void ApplyStick(StickSettings stick, short x, short y)
    {
        var nx = Math.Abs(x) <= stick.Deadzone ? 0 : x / 32768.0;
        var ny = Math.Abs(y) <= stick.Deadzone ? 0 : y / 32768.0;
        if (nx == 0 && ny == 0) return;
        var dy = stick.InvertY ? -ny : ny;
        _backend.MoveMouse((int)(nx * stick.MouseSpeed), (int)(-dy * stick.MouseSpeed));
    }

    private static bool TryGetButton(string name, out GamepadButton button)
    {
        button = default;
        var upper = name.ToUpperInvariant();
        switch (upper)
        {
            case "A": button = GamepadButton.A; return true;
            case "B": button = GamepadButton.B; return true;
            case "X": button = GamepadButton.X; return true;
            case "Y": button = GamepadButton.Y; return true;
            case "LB": button = GamepadButton.LB; return true;
            case "RB": button = GamepadButton.RB; return true;
            case "BACK": button = GamepadButton.Back; return true;
            case "START": button = GamepadButton.Start; return true;
            case "LS": button = GamepadButton.LeftStick; return true;
            case "RS": button = GamepadButton.RightStick; return true;
            case "DPADUP": button = GamepadButton.DpadUp; return true;
            case "DPADDOWN": button = GamepadButton.DpadDown; return true;
            case "DPADLEFT": button = GamepadButton.DpadLeft; return true;
            case "DPADRIGHT": button = GamepadButton.DpadRight; return true;
            default: return false;
        }
    }
}


