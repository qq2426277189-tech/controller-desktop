using System;
using System.Windows;
using ControllerDesktop.Core.Profiles;
using ControllerDesktop.Input;

namespace ControllerDesktop.App
{
    public partial class App : Application
    {
        private WinForms.NotifyIcon? _trayIcon;

        private static System.Drawing.Icon? TryLoadIcon()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/ControllerDesktop.ico");
                var s = Application.GetResourceStream(uri);
                if (s != null) return new System.Drawing.Icon(s.Stream);
            }
            catch { }
            return System.Drawing.SystemIcons.Application;
        }
        public static XInputGamepad Gamepad { get; } = new();
        public static SendInputBackend Backend { get; } = new();
        public static BindingEngine Engine { get; } = new(Backend);
        public static ProfileStore Store { get; } = new();

                private static void ShowMainWindow()
        {
            var window = Current.MainWindow;
            if (window == null) return;
            window.Show();
            window.WindowState = System.Windows.WindowState.Normal;
            window.Activate();
        }

        private static void OpenFolder(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
            }
            catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                _trayIcon = new WinForms.NotifyIcon
                {
                    Icon = TryLoadIcon(),
                    Text = "手柄桌面控制",
                    Visible = true
                };
                var menu = new WinForms.ContextMenuStrip();
                menu.Items.Add("显示主窗口", null, (_, _) => ShowMainWindow());
                menu.Items.Add(new WinForms.ToolStripSeparator());
                menu.Items.Add("打开配置文件", null, (_, _) => OpenFolder(ControllerDesktop.Core.AppPaths.ConfigDir));
                menu.Items.Add("打开日志目录", null, (_, _) => OpenFolder(ControllerDesktop.Core.AppPaths.LogsDir));
                menu.Items.Add(new WinForms.ToolStripSeparator());
                menu.Items.Add("退出", null, (_, _) => { (Current.MainWindow as MainWindow)?.RealExit(); Shutdown(); });
                _trayIcon.ContextMenuStrip = menu;
                _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
            }
            catch { }
            DispatcherUnhandledException += (_, args) =>
            {
                args.Handled = true;
                MessageBox.Show("Error: " + args.Exception.Message, "ControllerDesktop");
            };
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                try { System.IO.File.AppendAllText(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ControllerDesktop-crash.log"),
                    DateTime.Now + " " + args.ExceptionObject + Environment.NewLine); } catch { }
            };
            Backend.Initialize();
            Gamepad.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Gamepad.Dispose();
            Backend.Shutdown();
            base.OnExit(e);
        }
    }
}



