using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ControllerDesktop.App
{
    internal static class ForegroundWatcher
    {
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
        [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern bool QueryFullProcessImageName(IntPtr hProcess, int flags, StringBuilder text, ref int size);

        public record Info(string ProcessName, string ProcessPath, IntPtr WindowHandle, bool IsFullscreen);

        public static Info? GetForegroundInfo()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return null;
                GetWindowThreadProcessId(hwnd, out var pid);
                var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                var sb = new StringBuilder(1024);
                int size = sb.Capacity;
                string path = "";
                try { QueryFullProcessImageName(proc.Handle, 0, sb, ref size); path = sb.ToString(); } catch { }
                // WS_BORDER+WS_CAPTION absent = likely fullscreen borderless
                int style = GetWindowLong(hwnd, -16); // GWL_STYLE
                bool fullscreen = (style & 0x00C00000) == 0; // WS_CAPTION
                return new Info(proc.ProcessName, path, hwnd, fullscreen);
            }
            catch { return null; }
        }

        public static string GetForegroundProcessName()
        {
            var info = GetForegroundInfo();
            return info?.ProcessName ?? "";
        }

        public static bool IsBilibili() =>
            GetForegroundProcessName() is "哔哩哔哩" or "bilibili";
    }
}
