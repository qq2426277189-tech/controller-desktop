using System;
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
        if (!Enum.TryParse<ConsoleKey>(key, true, out var consoleKey)) return;
        var vk = (ushort)consoleKey;
        KeyEvent(vk, false);
        KeyEvent(vk, true);
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
