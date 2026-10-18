using System;
using System.Windows;
using System.Windows.Input;

namespace ControllerDesktop.App
{
    public partial class CaptureKeyWindow : Window
    {
        public string? Result { get; private set; }
        private bool _gotResult;

        public CaptureKeyWindow(string buttonName)
        {
            InitializeComponent();
            PromptText.Text = $"为 [{buttonName}] 设置新动作";
            SubText.Text = "现在按一个键盘键，或点击鼠标左/右/中键；也可以在左侧列表拖拽过来。Esc 取消。";
            PreviewKeyDown += OnKey;
            PreviewMouseDown += OnMouse;
            Closed += (_, _) => { if (!_gotResult) Result = null; };
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            string? text = key switch
            {
                Key.Escape => null, // cancel
                Key.LeftCtrl or Key.RightCtrl => null,
                Key.LeftShift or Key.RightShift => null,
                Key.LeftAlt or Key.RightAlt => null,
                Key.LWin or Key.RWin => null,
                _ => MapKey(key)
            };
            if (text == null) return;
            // modifiers
            var mods = Keyboard.Modifiers;
            var combo = "";
            if (mods.HasFlag(ModifierKeys.Control)) combo += "CTRL+";
            if (mods.HasFlag(ModifierKeys.Alt)) combo += "ALT+";
            if (mods.HasFlag(ModifierKeys.Shift)) combo += "SHIFT+";
            if (mods.HasFlag(ModifierKeys.Windows)) combo += "LWIN+";
            Result = combo + text;
            ResultText.Text = Result;
            e.Handled = true;
        }

        private void OnMouse(object sender, MouseButtonEventArgs e)
        {
            Result = e.ChangedButton switch
            {
                MouseButton.Left => "MouseLeft",
                MouseButton.Right => "MouseRight",
                MouseButton.Middle => "MouseMiddle",
                MouseButton.XButton1 => null,
                MouseButton.XButton2 => null,
                _ => null
            };
            if (Result != null) ResultText.Text = Result;
            e.Handled = true;
        }

        private static string? MapKey(Key key) => key switch
        {
            Key.A => "A", Key.B => "B", Key.C => "C", Key.D => "D", Key.E => "E",
            Key.F => "F", Key.G => "G", Key.H => "H", Key.I => "I", Key.J => "J",
            Key.K => "K", Key.L => "L", Key.M => "M", Key.N => "N", Key.O => "O",
            Key.P => "P", Key.Q => "Q", Key.R => "R", Key.S => "S", Key.T => "T",
            Key.U => "U", Key.V => "V", Key.W => "W", Key.X => "X", Key.Y => "Y", Key.Z => "Z",
            Key.D0 => "0", Key.D1 => "1", Key.D2 => "2", Key.D3 => "3", Key.D4 => "4",
            Key.D5 => "5", Key.D6 => "6", Key.D7 => "7", Key.D8 => "8", Key.D9 => "9",
            Key.F1 => "F1", Key.F2 => "F2", Key.F3 => "F3", Key.F4 => "F4",
            Key.F5 => "F5", Key.F6 => "F6", Key.F7 => "F7", Key.F8 => "F8",
            Key.F9 => "F9", Key.F10 => "F10", Key.F11 => "F11", Key.F12 => "F12",
            Key.Space => "SPACE", Key.Tab => "TAB", Key.Enter => "ENTER", Key.Back => "BACKSPACE",
            Key.Up => "UP", Key.Down => "DOWN", Key.Left => "LEFT", Key.Right => "RIGHT",
            _ => null
        };

        private void Ok_Click(object sender, RoutedEventArgs e) { _gotResult = true; DialogResult = true; Close(); }
        private void Clear_Click(object sender, RoutedEventArgs e) { Result = ""; _gotResult = true; DialogResult = true; Close(); }
        private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}
