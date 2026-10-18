using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ControllerDesktop.Input;

public sealed class SendInputBackend : IInputBackend
{
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public string Name => "SendInput";

    public void Initialize() { }
    public void Shutdown() { }

    public void MoveMouse(int deltaX, int deltaY)
    {
        var input = new INPUT { type = INPUT_MOUSE };
        input.union.mi.dx = deltaX;
        input.union.mi.dy = deltaY;
        input.union.mi.dwFlags = MOUSEEVENTF_MOVE;
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    public void MouseLeft(bool down) => MouseButton(down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP);
    public void MouseRight(bool down) => MouseButton(down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP);
    public void MouseMiddle(bool down) => MouseButton(down ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP);

    private static void MouseButton(uint flags)
    {
        var input = new INPUT { type = INPUT_MOUSE };
        input.union.mi.dwFlags = flags;
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    public void Wheel(int delta)
    {
        var input = new INPUT { type = INPUT_MOUSE };
        input.union.mi.mouseData = unchecked((uint)delta);
        input.union.mi.dwFlags = MOUSEEVENTF_WHEEL;
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    public void KeyTap(string key)
    {
        if (!TryParseCombo(key, out var modifiers, out var vk)) return;
        foreach (var m in modifiers) KeyEvent(m, false);
        KeyEvent(vk, false);
        KeyEvent(vk, true);
        for (var i = modifiers.Count - 1; i >= 0; i--) KeyEvent(modifiers[i], true);
    }

    public static bool TryParseCombo(string text, out List<ushort> modifiers, out ushort vk)
    {
        modifiers = new List<ushort>();
        vk = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var parts = text.Split('+');
        for (var i = 0; i < parts.Length; i++)
        {
            if (!TryParseVk(parts[i].Trim(), out var code)) return false;
            if (i < parts.Length - 1) modifiers.Add(code);
            else vk = code;
        }
        return vk != 0;
    }

    private static bool TryParseVk(string name, out ushort vk)
    {
        vk = name.ToUpperInvariant() switch
        {
            "ESC" => 0x1B,
            "TAB" => 0x09,
            "ENTER" => 0x0D,
            "SPACE" => 0x20,
            "BACKSPACE" => 0x08,
            "UP" => 0x26,
            "DOWN" => 0x28,
            "LEFT" => 0x25,
            "RIGHT" => 0x27,
            "LWIN" or "WIN" => 0x5B,
            "LCTRL" or "CTRL" => 0x11,
            "LSHIFT" or "SHIFT" => 0x10,
            "LALT" or "ALT" => 0x12,
            "F11" => 0x7A,
            "F5" => 0x74,
            "MEDIA_PLAY" => 0xB3,
            "MEDIA_PREV" => 0xB1,
            "MEDIA_NEXT" => 0xB0,
            "VOLUME_UP" => 0xAF,
            "VOLUME_DOWN" => 0xAE,
            "VOLUME_MUTE" => 0xAD,
            _ => 0
        };
        if (vk != 0) return true;
        // Single letter A-Z or digit 0-9
        if (name.Length == 1)
        {
            var c = char.ToUpperInvariant(name[0]);
            if (c >= 'A' && c <= 'Z') { vk = (ushort)c; return true; }
            if (c >= '0' && c <= '9') { vk = (ushort)c; return true; }
        }
        return false;
    }

    private static void KeyEvent(ushort vk, bool up)
    {
        var input = new INPUT { type = INPUT_KEYBOARD };
        input.union.ki.wVk = vk;
        input.union.ki.dwFlags = up ? KEYEVENTF_KEYUP : 0;
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}
