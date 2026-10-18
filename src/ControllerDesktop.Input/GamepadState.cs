using System;

namespace ControllerDesktop.Input;

public readonly struct GamepadState
{
    public ushort Buttons { get; init; }
    public short LeftX { get; init; }
    public short LeftY { get; init; }
    public short RightX { get; init; }
    public short RightY { get; init; }
    public byte LeftTrigger { get; init; }
    public byte RightTrigger { get; init; }

    public bool Has(GamepadButton button) => (Buttons & (ushort)button) != 0;

    public static bool operator ==(GamepadState a, GamepadState b) =>
        a.Buttons == b.Buttons && a.LeftX == b.LeftX && a.LeftY == b.LeftY &&
        a.RightX == b.RightX && a.RightY == b.RightY &&
        a.LeftTrigger == b.LeftTrigger && a.RightTrigger == b.RightTrigger;
    public static bool operator !=(GamepadState a, GamepadState b) => !(a == b);
    public override bool Equals(object? obj) => obj is GamepadState s && this == s;
    public override int GetHashCode() => HashCode.Combine(Buttons, LeftX, LeftY, RightX, RightY, LeftTrigger, RightTrigger);
}

[Flags]
public enum GamepadButton : ushort
{
    DpadUp = 0x0001,
    DpadDown = 0x0002,
    DpadLeft = 0x0004,
    DpadRight = 0x0008,
    Start = 0x0010,
    Back = 0x0020,
    LeftStick = 0x0040,
    RightStick = 0x0080,
    LB = 0x0100,
    RB = 0x0200,
    Y = 0x0800,
    B = 0x1000,
    A = 0x2000,
    X = 0x4000
}
