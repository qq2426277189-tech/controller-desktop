using System;
using System.Runtime.InteropServices;

namespace ControllerDesktop.App
{
    internal static class ForegroundWatcher
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        public static string GetForegroundProcessName()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return "";
                GetWindowThreadProcessId(hwnd, out var pid);
                var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                return proc.ProcessName;
            }
            catch { return ""; }
        }

        public static bool IsBilibili() =>
            GetForegroundProcessName() is "哔哩哔哩" or "bilibili";
    }
}
