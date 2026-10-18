using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using ControllerDesktop.Input;

namespace ControllerDesktop.App
{
    /// <summary>Ported from legacy 0.5.1: X+B emergency recovery, stuck-mouse detection and ASUS AsHotplugCtrl handling.</summary>
    internal static class DesktopRecovery
    {
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT p);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr GetShellWindow();
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] private static extern bool ClipCursor(IntPtr rect);
        [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] private static extern bool GetCursorInfo(out CURSORINFO info);
        [DllImport("user32.dll")] private static extern bool ShowCursor(bool show);

        [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO { public int cbSize; public int flags; public IntPtr hCursor; public IntPtr hwnd; }

        private static DateTime _chordStart = DateTime.MinValue;
        private static bool _chordHandled;
        private static bool _stuckTracking;
        private static DateTime _stuckStart = DateTime.MinValue;
        private static int _stuckX, _stuckY;
        private static DateTime _lastRecoveryAt = DateTime.MinValue;
        private static string? _killedProcessName;
        private static DateTime _restartAt = DateTime.MinValue;
        private static DateTime _restartBlockedUntil = DateTime.MinValue;
        private static DateTime _lastRestartAt = DateTime.MinValue;

        public static event Action<string>? Notice;
        public static event Action<string>? RuntimeLog;

        /// <summary>Call every gamepad tick. X+B held 1s triggers emergency recovery.</summary>
        public static void HandleChord(GamepadState previous, GamepadState current)
        {
            bool down = current.Has(GamepadButton.X) && current.Has(GamepadButton.B);
            if (!down) { _chordStart = DateTime.MinValue; _chordHandled = false; return; }
            if (_chordStart == DateTime.MinValue) { _chordStart = DateTime.Now; _chordHandled = false; return; }
            if (!_chordHandled && (DateTime.Now - _chordStart).TotalMilliseconds >= 1000)
            {
                _chordHandled = true;
                ForceRecover("X+B 紧急恢复");
            }
        }

        /// <summary>Call after each MoveMouse while stick is active. Detects cursor not moving (stuck).</summary>
        public static void TrackStickActivity()
        {
            GetCursorPos(out var now);
            if (!_stuckTracking)
            {
                _stuckTracking = true;
                _stuckStart = DateTime.Now;
                _stuckX = now.X; _stuckY = now.Y;
                return;
            }
            if ((DateTime.Now - _stuckStart).TotalMilliseconds < 700) return;

            var moved = Math.Abs(now.X - _stuckX) + Math.Abs(now.Y - _stuckY);
            if (moved <= 2)
            {
                ForceRecover("检测到鼠标卡住");
                _stuckTracking = false;
            }
            else
            {
                _stuckStart = DateTime.Now;
                _stuckX = now.X; _stuckY = now.Y;
            }
        }

        public static void StopStickTracking() { _stuckTracking = false; }

        public static void ForceRecover(string reason)
        {
            try
            {
                RuntimeLog?.Invoke(reason);
                TerminateAsusConflict(reason);
                FocusDesktop();
                ClipCursor(IntPtr.Zero);
                int w = (int)SystemParameters.PrimaryScreenWidth;
                int h = (int)SystemParameters.PrimaryScreenHeight;
                SetCursorPos(w / 2, h / 2);
                // make cursor visible again
                int attempts = 0;
                while (!IsCursorVisible() && attempts < 5) { ShowCursor(true); attempts++; }
                _lastRecoveryAt = DateTime.Now;
                Notice?.Invoke("桌面控制已恢复（" + reason + "）");
            }
            catch { }
        }

        private static void TerminateAsusConflict(string reason)
        {
            var procName = ForegroundProcessName();
            if (!IsAsusConflict(procName)) return;
            if (DateTime.Now < _restartBlockedUntil) return;

            try
            {
                bool killed = false;
                foreach (var p in Process.GetProcessesByName(procName))
                {
                    try { p.Kill(); killed = true; } catch { } finally { p.Dispose(); }
                }
                if (!killed) return;
                _lastRecoveryAt = DateTime.Now;
                _killedProcessName = procName;
                _restartAt = DateTime.Now.AddSeconds(20);
                if (_lastRestartAt != DateTime.MinValue && (DateTime.Now - _lastRestartAt).TotalSeconds < 10)
                {
                    _restartBlockedUntil = DateTime.Now.AddMinutes(10);
                    Notice?.Invoke(procName + " 再次卡住鼠标，已暂停自动恢复 10 分钟");
                }
                RuntimeLog?.Invoke("已终止 " + procName + "，20 秒后尝试重启");
            }
            catch { }
        }

        /// <summary>Call periodically. Restarts ASUS component after we killed it (once cursor is fine again).</summary>
        public static void CheckRestart()
        {
            if (_killedProcessName == null) return;
            if (DateTime.Now < _restartAt || DateTime.Now < _restartBlockedUntil) return;
            if ((DateTime.Now - _lastRecoveryAt).TotalSeconds < 10) return;
            if (Process.GetProcessesByName(_killedProcessName).Length > 0) { _killedProcessName = null; return; }

            var path = GetAsusPath(_killedProcessName);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) { _killedProcessName = null; return; }
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
                Notice?.Invoke("华硕组件已恢复：" + _killedProcessName);
                _killedProcessName = null;
            }
            catch { _killedProcessName = null; }
        }

        private static string ForegroundProcessName() => ForegroundWatcher.GetForegroundProcessName();

        private static bool IsAsusConflict(string? name) =>
            name is "AsHotplugCtrl" or "AsMonitorControl";

        private static string GetAsusPath(string name) => name switch
        {
            "AsHotplugCtrl" => @"C:\Program Files\ASUS\ASUS Hotplug Controller\AsHotplugCtrl.exe",
            "AsMonitorControl" => @"C:\Program Files\ASUS\ASUS Monitor Control\AsMonitorControl.exe",
            _ => ""
        };

        private static void FocusDesktop()
        {
            try
            {
                var shell = GetShellWindow();
                if (shell == IntPtr.Zero) return;
                SetWindowPos(shell, new IntPtr(-1), 0, 0, 0, 0, 0x0040 | 0x0001 | 0x0002);
                SetForegroundWindow(shell);
            }
            catch { }
        }

        private static bool IsCursorVisible()
        {
            var info = new CURSORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO)) };
            return GetCursorInfo(out info) && (info.flags & 1) == 1 && info.hCursor != IntPtr.Zero;
        }
    }
}



