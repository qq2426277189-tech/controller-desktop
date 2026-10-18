using System;
using System.Windows;
using ControllerDesktop.Core.Profiles;
using ControllerDesktop.Input;

namespace ControllerDesktop.App
{
    public partial class App : Application
    {
        public static XInputGamepad Gamepad { get; } = new();
        public static SendInputBackend Backend { get; } = new();
        public static BindingEngine Engine { get; } = new(Backend);
        public static ProfileStore Store { get; } = new();

                protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
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

