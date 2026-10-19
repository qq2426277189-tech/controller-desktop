using System.Windows;
using System.Windows.Input;

namespace ControllerDesktop.App
{
    /// <summary>Captures a single main key for combo bindings, without modifier keys.</summary>
    public partial class KeyCaptureDialog : Window
    {
        public string? Result { get; private set; }

        public KeyCaptureDialog()
        {
            InitializeComponent();
            PreviewKeyDown += OnKey;
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.Escape)
            {
                DialogResult = false;
                return;
            }

            var text = MapKey(key);
            if (text == null) return; // pure modifier or unsupported key

            Result = text;
            ResultText.Text = text;
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
            Key.NumPad0 => "NUM0", Key.NumPad1 => "NUM1", Key.NumPad2 => "NUM2",
            Key.NumPad3 => "NUM3", Key.NumPad4 => "NUM4", Key.NumPad5 => "NUM5",
            Key.NumPad6 => "NUM6", Key.NumPad7 => "NUM7", Key.NumPad8 => "NUM8",
            Key.NumPad9 => "NUM9",
            Key.F1 => "F1", Key.F2 => "F2", Key.F3 => "F3", Key.F4 => "F4",
            Key.F5 => "F5", Key.F6 => "F6", Key.F7 => "F7", Key.F8 => "F8",
            Key.F9 => "F9", Key.F10 => "F10", Key.F11 => "F11", Key.F12 => "F12",
            Key.Space => "SPACE", Key.Tab => "TAB", Key.Enter => "ENTER", Key.Back => "BACKSPACE",
            Key.Up => "UP", Key.Down => "DOWN", Key.Left => "LEFT", Key.Right => "RIGHT",
            Key.Escape => "ESC", Key.Delete => "DELETE", Key.Home => "HOME", Key.End => "END",
            Key.PageUp => "PAGEUP", Key.PageDown => "PAGEDOWN", Key.Insert => "INSERT",
            Key.OemComma => "COMMA", Key.OemPeriod => "PERIOD", Key.OemMinus => "MINUS",
            Key.OemPlus => "PLUS", Key.OemQuestion => "SLASH", Key.OemSemicolon => "SEMICOLON",
            Key.OemQuotes => "QUOTE", Key.OemOpenBrackets => "LBRACKET",
            Key.OemCloseBrackets => "RBRACKET", Key.OemPipe => "BACKSLASH", Key.OemTilde => "TILDE",
            Key.CapsLock => "CAPSLOCK",
            _ => null
        };

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Result)) return;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
