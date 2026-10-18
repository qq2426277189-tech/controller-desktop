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
            var name = binding.Button.Trim().ToUpperInvariant();

            if (name is "LT" or "RT")
            {
                var trigPrev = name == "LT" ? previous.LeftTrigger : previous.RightTrigger;
                var trigNow = name == "LT" ? current.LeftTrigger : current.RightTrigger;
                var trigWas = trigPrev > 100;
                var trigIs = trigNow > 100;
                if (trigWas != trigIs) ApplyAction(binding, trigIs);
                continue;
            }

            if (!TryGetButton(name, out var button)) continue;

            var wasDown = previous.Has(button);
            var isDown = current.Has(button);
            if (wasDown == isDown) continue;

            ApplyAction(binding, isDown);
        }
    }

    private void ApplyAction(ButtonBinding binding, bool isDown)
    {
        switch (binding.Action)
        {
            case ActionType.MouseLeft:
                _backend.MouseLeft(isDown);
                break;
            case ActionType.MouseRight:
                _backend.MouseRight(isDown);
                break;
            case ActionType.MouseMiddle:
                _backend.MouseMiddle(isDown);
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

    private double _accX, _accY;

    /// <summary>Continuous mouse movement, legacy 0.5.1 curve + accumulator for smooth motion.</summary>
    private void ApplyStick(StickSettings stick, short x, short y)
    {
        var nx = ApplyDeadzone(x, stick.Deadzone);
        var ny = ApplyDeadzone(y, stick.Deadzone);
        if (nx == 0 && ny == 0) { _accX = 0; _accY = 0; return; }

        var speed = Math.Max(stick.MouseSpeed, 6);
        double sx = nx * (6.0 + Math.Abs(nx) * (speed - 6.0));
        double syRaw = ny * (6.0 + Math.Abs(ny) * (speed - 6.0));
        double sy = stick.InvertY ? syRaw : -syRaw;

        _accX += sx;
        _accY += sy;
        int dx = (int)_accX;
        int dy = (int)_accY;
        _accX -= dx;
        _accY -= dy;
        if (dx != 0 || dy != 0)
            _backend.MoveMouse(dx, dy);
    }

    private static double ApplyDeadzone(short value, int deadzone)
    {
        if (Math.Abs((int)value) <= deadzone) return 0.0;
        double sign = value < 0 ? -1.0 : 1.0;
        double magnitude = (Math.Abs((int)value) - deadzone) / (double)(32767 - deadzone);
        return sign * Math.Min(1.0, magnitude);
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



