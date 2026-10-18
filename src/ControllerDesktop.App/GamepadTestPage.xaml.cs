using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ControllerDesktop.Core.Localization;
using ControllerDesktop.Input;

namespace ControllerDesktop.App
{
    public partial class GamepadTestPage : UserControl
    {
        private readonly DispatcherTimer _timer;
        private readonly LanguageService _language;

        public GamepadTestPage(LanguageService language)
        {
            InitializeComponent();
            _language = language;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += (_, _) => Refresh();
            Loaded += (_, _) => { Refresh(); _timer.Start(); };
            Unloaded += (_, _) => _timer.Stop();
            HintText.Text = language.T("TestHint");
        }

        private void Refresh()
        {
            var pad = App.Gamepad;
            ConnectionText.Text = (_language.T("GamepadConnected") + ": ") + (pad.Connected ? "✔" : "✘");
            if (!pad.Connected)
            {
                ButtonsText.Text = "";
                SticksText.Text = "";
                return;
            }
            var s = pad.Last;
            var sb = new StringBuilder();
            foreach (GamepadButton b in Enum.GetValues(typeof(GamepadButton)))
                if (s.Has(b)) sb.Append(b).Append(' ');
            ButtonsText.Text = (_language.T("Buttons") + ": ") + (sb.Length == 0 ? "-" : sb.ToString());
            SticksText.Text = string.Format("L: {0,6}, {1,6}   R: {2,6}, {3,6}   LT: {4,3} RT: {5,3}",
                s.LeftX, s.LeftY, s.RightX, s.RightY, s.LeftTrigger, s.RightTrigger);
        }
    }
}


