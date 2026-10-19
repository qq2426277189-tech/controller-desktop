using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ControllerDesktop.Core;
using ControllerDesktop.Core.Profiles;

namespace ControllerDesktop.App
{
    public partial class DiagnosticsPage : UserControl
    {
        private readonly DispatcherTimer _timer;

        public DiagnosticsPage()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (_, _) => Refresh();
            Loaded += (_, _) => { Refresh(); _timer.Start(); };
            Unloaded += (_, _) => _timer.Stop();

            RuntimeHeader.Text = "运行状态";
            GamepadHeader.Text = "手柄";
            ForegroundHeader.Text = "前台程序";
            PathsHeader.Text = "路径";
            LogHeader.Text = "最近日志";
        }

        private void Refresh()
        {
            var pad = App.Gamepad;
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "2.0.0";

            RuntimeText.Text =
                $"版本:        {version}{Environment.NewLine}" +
                $"输入后端:    {App.Backend.Name}（无驱动）{Environment.NewLine}" +
                $"运行时长:    {FormatUptime(DateTime.Now - Process.GetCurrentProcess().StartTime)}";

            if (!pad.Connected)
            {
                GamepadText.Text = "未连接（请检查 XInput 模式）";
            }
            else
            {
                var s = pad.Last;
                var buttons = string.Join(" ", Enum.GetValues<ControllerDesktop.Input.GamepadButton>()
                    .Where(b => s.Has(b)));
                GamepadText.Text =
                    "已连接" + Environment.NewLine +
                    "按键: " + (buttons.Length == 0 ? "-" : buttons) + Environment.NewLine +
                    string.Format("左摇杆: {0,6},{1,6}   右摇杆: {2,6},{3,6}", s.LeftX, s.LeftY, s.RightX, s.RightY) + Environment.NewLine +
                    string.Format("LT: {0,3}   RT: {1,3}", s.LeftTrigger, s.RightTrigger);
            }

            var info = ForegroundWatcher.GetForegroundInfo();
            ForegroundText.Text = info == null
                ? "无法读取"
                : $"程序: {info.ProcessName}{Environment.NewLine}路径: {(string.IsNullOrEmpty(info.ProcessPath) ? "-" : info.ProcessPath)}{Environment.NewLine}全屏: {(info.IsFullscreen ? "是" : "否")}";

            PathsText.Text =
                $"应用目录:  {AppPaths.Root}{Environment.NewLine}" +
                $"配置目录:  {AppPaths.ConfigDir}{Environment.NewLine}" +
                $"日志目录:  {AppPaths.LogsDir}{Environment.NewLine}" +
                $"配置文件:  {new ProfileStore().StorePath}";

            LogText.Text = ReadLogTail();

            UpdatedText.Text = "更新于 " + DateTime.Now.ToString("HH:mm:ss");
        }

        private static string FormatUptime(TimeSpan span) =>
            $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";

        private static string ReadLogTail()
        {
            try
            {
                var path = Path.Combine(AppPaths.LogsDir, "runtime.log");
                if (!File.Exists(path)) return "(暂无日志)";
                var lines = File.ReadAllLines(path);
                var tail = lines.Skip(Math.Max(0, lines.Length - 12));
                return string.Join(Environment.NewLine, tail);
            }
            catch (Exception ex)
            {
                return "(读取日志失败: " + ex.Message + ")";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => Refresh();

        private void OpenLogs_Click(object sender, RoutedEventArgs e) => OpenFolder(AppPaths.LogsDir);

        private void OpenConfig_Click(object sender, RoutedEventArgs e) => OpenFolder(AppPaths.ConfigDir);

        private static void OpenFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
            }
            catch { }
        }
    }
}
