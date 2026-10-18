using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ControllerDesktop.Core.Models;
using ControllerDesktop.Core.Profiles;

namespace ControllerDesktop.App
{
    public partial class MappingVisualPage : UserControl
    {
        private AppConfiguration _configuration;
        private MappingProfile _profile = new();
        private readonly Dictionary<string, TextBlock> _labels = new();

        public event Action? MappingChanged;

        public MappingVisualPage(AppConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            LoadProfile();
            DrawController();
            BuildPalette();
        }

        public void Reload(AppConfiguration configuration)
        {
            _configuration = configuration;
            LoadProfile();
            RefreshLabels();
        }

        private MappingProfile CurrentProfile
        {
            get
            {
                var p = _configuration.Profiles.FirstOrDefault(x => x.Name == _configuration.CurrentProfile)
                        ?? _configuration.Profiles.FirstOrDefault();
                if (p == null)
                {
                    p = new MappingProfile { Name = _configuration.CurrentProfile };
                    _configuration.Profiles.Add(p);
                }
                return p;
            }
        }

        private void LoadProfile()
        {
            _profile = CurrentProfile;
        }

        private BindingLookup Lookup => new(_profile.Bindings);

        // Button positions on the canvas (label anchor + controller point), calibrated to the official Nova Lite render
        private static readonly (string Name, Point Dot, Point Label)[] Buttons =
        {
            ("Y",  new Point(516, 223), new Point(720, 180)),
            ("X",  new Point(492, 243), new Point(720, 225)),
            ("B",  new Point(539, 244), new Point(720, 270)),
            ("A",  new Point(516, 264), new Point(720, 315)),
            ("LB", new Point(292, 206), new Point(20, 190)),
            ("RB", new Point(547, 206), new Point(720, 80)),
            ("LT", new Point(372, 237), new Point(20, 120)),
            ("RT", new Point(467, 237), new Point(720, 130)),
            ("BACK", new Point(419, 230), new Point(280, 490)),
            ("START", new Point(419, 306), new Point(440, 490)),
            ("LS", new Point(292, 261), new Point(20, 260)),
            ("RS", new Point(482, 318), new Point(560, 470)),
            ("DPAD", new Point(357, 332), new Point(60, 400))
        };

        private void DrawController()
        {
            Diagram.Children.Clear();
            _labels.Clear();

            // Official GameSir Nova Lite render (transparent background), desaturated to white
            var img = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Assets/NovaLite.png")),
                Width = 760
            };
            Canvas.SetLeft(img, 120); Canvas.SetTop(img, 40);
            Diagram.Children.Add(img);

            // Connector lines + labels
            foreach (var b in Buttons)
            {
                var line = new Line
                {
                    X1 = b.Dot.X, Y1 = b.Dot.Y,
                    X2 = b.Label.X - 10, Y2 = b.Label.Y + 14,
                    Stroke = new SolidColorBrush(Color.FromRgb(0xB4,0xB6,0xC4)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 3, 2 }
                };
                Diagram.Children.Add(line);
                var dot = new Ellipse
                {
                    Width = 10, Height = 10,
                    Stroke = new SolidColorBrush(Color.FromRgb(0x5A,0x5C,0x68)),
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(dot, b.Dot.X - 5); Canvas.SetTop(dot, b.Dot.Y - 5);
                Diagram.Children.Add(dot);

                var border = new Border
                {
                    Width = 118, Height = 28,
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(0x2A,0x2B,0x31)),
                    Cursor = Cursors.Hand,
                    Tag = b.Name,
                    AllowDrop = true
                };
                var tb = new TextBlock
                {
                    Foreground = Brushes.White,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                border.Child = tb;
                _labels[b.Name] = tb;
                Canvas.SetLeft(border, b.Label.X);
                Canvas.SetTop(border, b.Label.Y);
                border.MouseEnter += (_, _) => border.Background = new SolidColorBrush(Color.FromRgb(0x3D,0x40,0x4C));
                border.MouseLeave += (_, _) => border.Background = new SolidColorBrush(Color.FromRgb(0x2A,0x2B,0x31));
                border.MouseLeftButtonDown += Label_Click;
                border.DragEnter += Label_DragEnter;
                border.Drop += Label_Drop;
                border.DragOver += (_, e) => e.Effects = DragDropEffects.Copy;
                Diagram.Children.Add(border);
            }
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            var lookup = Lookup;
            foreach (var b in Buttons)
            {
                if (!_labels.TryGetValue(b.Name, out var tb)) continue;
                var display = b.Name == "DPAD"
                    ? string.Join("/", new[] { "DPADUP", "DPADDOWN", "DPADLEFT", "DPADRIGHT" }
                        .Select(d => lookup.Describe(d)))
                    : lookup.Describe(b.Name);
                tb.Text = $"{b.Name} → {display}";
            }
        }

        private void Label_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border { Tag: string btn })
            {
                if (btn == "DPAD")
                {
                    foreach (var d in new[] { "DPADUP", "DPADDOWN", "DPADLEFT", "DPADRIGHT" }) EditBinding(d);
                }
                else EditBinding(btn);
            }
        }

        private void Label_DragEnter(object sender, DragEventArgs e) { }
        private void Label_Drop(object sender, DragEventArgs e)
        {
            if (sender is not Border { Tag: string btn }) return;
            if (!e.Data.GetDataPresent(DataFormats.StringFormat)) return;
            var value = (string)e.Data.GetData(DataFormats.StringFormat)!;
            if (btn == "DPAD")
            {
                foreach (var d in new[] { "DPADUP", "DPADDOWN", "DPADLEFT", "DPADRIGHT" }) SetBinding(d, value);
            }
            else SetBinding(btn, value);
            SaveAndRefresh();
        }

        private void EditBinding(string button)
        {
            var dlg = new CaptureKeyWindow(button) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
            if (dlg.Result == null) return;
            SetBinding(button, dlg.Result);
            SaveAndRefresh();
        }

        private void SetBinding(string button, string value)
        {
            var existing = _profile.Bindings.FirstOrDefault(b => b.Button == button);
            if (value == "")
            {
                if (existing != null) _profile.Bindings.Remove(existing);
                return;
            }
            if (existing == null)
            {
                existing = new ButtonBinding { Button = button };
                _profile.Bindings.Add(existing);
            }
            if (value.StartsWith("Mouse"))
            {
                existing.Action = value switch
                {
                    "MouseLeft" => ActionType.MouseLeft,
                    "MouseRight" => ActionType.MouseRight,
                    "MouseMiddle" => ActionType.MouseMiddle,
                    _ => ActionType.None
                };
                existing.Value = "";
            }
            else
            {
                existing.Action = ActionType.Key;
                existing.Value = value;
            }
        }

        private void SaveAndRefresh()
        {
            new ProfileStore().Save(_configuration);
            RefreshLabels();
            MappingChanged?.Invoke();
        }

        private void BuildPalette()
        {
            PalettePanel.Children.Clear();
            // Mouse entries
            foreach (var (text, value) in new[] {
                ("🖱 鼠标左键", "MouseLeft"), ("🖱 鼠标右键", "MouseRight"), ("🖱 鼠标中键", "MouseMiddle") })
                PalettePanel.Children.Add(MakeChip(text, value));
            // Common keyboard keys
            foreach (var k in new[] {
                "ESC","TAB","ENTER","SPACE","BACKSPACE","UP","DOWN","LEFT","RIGHT",
                "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
                "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
                "1","2","3","4","5","6","7","8","9","0",
                "CTRL","ALT","SHIFT","LWIN",
                "ALT+TAB","ALT+LEFT","ALT+RIGHT","ALT+F4","LWIN+D","LWIN+E","LWIN+L",
                "CTRL+C","CTRL+V","CTRL+X","CTRL+Z","CTRL+S",
                "MEDIA_PLAY","MEDIA_PREV","MEDIA_NEXT","VOLUME_UP","VOLUME_DOWN","VOLUME_MUTE",
                "F11" })
            {
                var label = k.StartsWith("ALT+") || k.StartsWith("LWIN+") || k.StartsWith("CTRL+") ? k : k;
                PalettePanel.Children.Add(MakeChip(label, k));
            }
        }

        private Border MakeChip(string text, string payload)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xE6,0xE6,0xEE)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 6, 6),
                Cursor = Cursors.Hand,
                Tag = payload
            };
            var tb = new TextBlock { Text = text, FontSize = 12 };
            border.Child = tb;
            border.MouseLeftButtonDown += (_, e) =>
            {
                DragDrop.DoDragDrop(border, payload, DragDropEffects.Copy);
                e.Handled = true;
            };
            border.MouseLeftButtonDown += (_, e) =>
            {
                if (e.ClickCount == 2 && _pendingTarget != null) { SetBinding(_pendingTarget, payload); _pendingTarget = null; SaveAndRefresh(); }
            };
            return border;
        }

        private string? _pendingTarget;
    }

    /// <summary>Describes bindings for display.</summary>
    internal sealed class BindingLookup
    {
        private readonly Dictionary<string, ButtonBinding> _map;
        public BindingLookup(IEnumerable<ButtonBinding> bindings) =>
            _map = bindings.Where(b => !string.IsNullOrEmpty(b.Button))
                .GroupBy(b => b.Button.ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());

        public string Describe(string button)
        {
            if (!_map.TryGetValue(button.ToUpperInvariant(), out var b)) return "无";
            return b.Action switch
            {
                ActionType.MouseLeft => "鼠标左键",
                ActionType.MouseRight => "鼠标右键",
                ActionType.MouseMiddle => "鼠标中键",
                ActionType.Wheel => "滚轮 " + b.Value,
                ActionType.Key => b.Value,
                ActionType.MediaKey => "媒体键",
                _ => "无"
            };
        }
    }
}


