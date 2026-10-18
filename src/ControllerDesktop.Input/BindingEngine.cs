using System;
using ControllerDesktop.Core.Models;

namespace ControllerDesktop.Input;

public sealed class BindingEngine
{
    private readonly IInputBackend _backend;

    public BindingEngine(IInputBackend backend)
    {
        _backend = backend;
    }

    /// <summary>Called every gamepad tick.</summary>
    public void Apply(MappingProfile profile, GamepadState previous, GamepadState current)
    {
        try
        {
            ApplyBindings(profile, previous, current);
            ApplyStick(profile.RightStick, current.RightX, current.RightY);
            ApplyStick(profile.LeftStick, current.LeftX, current.LeftY);
        }
        catch
        {
            // Never let an input error crash the app.
        }
    }

    private void ApplyBindings(MappingProfile profile, GamepadState previous, GamepadState current)
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
                case ActionType.None when button == GamepadButton.Y:
                    // handled by the app (Y short = quick menu); no synthetic action.
                    break;
            }
        }
    }

    /// <summary>Continuous mouse movement while the stick is held off-center.</summary>
    private void ApplyStick(StickSettings stick, short x, short y)
    {
        var nx = Math.Abs(x) <= stick.Deadzone ? 0 : x / 32768.0;
        var ny = Math.Abs(y) <= stick.Deadzone ? 0 : y / 32768.0;
        if (nx == 0 && ny == 0) return;
        var dy = stick.InvertY ? -ny : ny;
        // Per-tick delta; at ~62Hz speed 8 gives a comfortable desktop cursor speed.
        _backend.MoveMouse((int)Math.Round(nx * stick.MouseSpeed), (int)Math.Round(-dy * stick.MouseSpeed));
    }

    private static bool TryGetButton(string name, out GamepadButton button)
    {
        button = default;
        switch (name.ToUpperInvariant())
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


