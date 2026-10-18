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

        // Button positions on the canvas (label anchor + controller point)
        private static readonly (string Name, Point Dot, Point Label)[] Buttons =
        {
            ("Y",  new Point(560, 155), new Point(640, 90)),
            ("X",  new Point(500, 210), new Point(640, 160)),
            ("B",  new Point(620, 210), new Point(640, 230)),
            ("A",  new Point(560, 265), new Point(640, 300)),
            ("LB", new Point(330, 130), new Point(180, 90)),
            ("RB", new Point(590, 130), new Point(640, 40)),
            ("LT", new Point(390, 80),  new Point(180, 30)),
            ("RT", new Point(530, 80),  new Point(640, -20)),
            ("BACK", new Point(415, 205), new Point(280, 160)),
            ("START", new Point(505, 205), new Point(500, 160)),
            ("LS", new Point(355, 300), new Point(120, 220)),
            ("RS", new Point(565, 330), new Point(120, 300)),
            ("DPAD", new Point(395, 380), new Point(120, 400))
        };

        private void DrawController()
        {
            Diagram.Children.Clear();
            _labels.Clear();

            // Body
            { var body = new Ellipse { Width = 360, Height = 340, Fill = Brushes.White, Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 3 }; Canvas.SetLeft(body, 280); Canvas.SetTop(body, 110); Diagram.Children.Add(body); }
            // Left grip, right grip
            { var gl = new Rectangle { Width = 90, Height = 200, RadiusX = 45, RadiusY = 60, Fill = Brushes.White, Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 3 }; Canvas.SetLeft(gl, 300); Canvas.SetTop(gl, 220); Diagram.Children.Add(gl); }
            { var gr = new Rectangle { Width = 90, Height = 200, RadiusX = 45, RadiusY = 60, Fill = Brushes.White, Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 3 }; Canvas.SetLeft(gr, 530); Canvas.SetTop(gr, 220); Diagram.Children.Add(gr); }
            // Triggers (top)
            { var t1 = new Rectangle { Width = 60, Height = 30, RadiusX = 10, RadiusY = 15, Fill = new SolidColorBrush(Color.FromRgb(0xEE,0xEE,0xF3)), Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 2 }; Canvas.SetLeft(t1, 360); Canvas.SetTop(t1, 60); Diagram.Children.Add(t1); }
            { var t2 = new Rectangle { Width = 60, Height = 30, RadiusX = 10, RadiusY = 15, Fill = new SolidColorBrush(Color.FromRgb(0xEE,0xEE,0xF3)), Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 2 }; Canvas.SetLeft(t2, 500); Canvas.SetTop(t2, 60); Diagram.Children.Add(t2); }
            // Sticks
            { var s1 = new Ellipse { Width = 66, Height = 66, Fill = new SolidColorBrush(Color.FromRgb(0xDD,0xDD,0xE4)), Stroke = new SolidColorBrush(Color.FromRgb(0x77,0x77,0x88)), StrokeThickness = 2 }; Canvas.SetLeft(s1, 322); Canvas.SetTop(s1, 267); Diagram.Children.Add(s1); }
            { var s2 = new Ellipse { Width = 66, Height = 66, Fill = new SolidColorBrush(Color.FromRgb(0xDD,0xDD,0xE4)), Stroke = new SolidColorBrush(Color.FromRgb(0x77,0x77,0x88)), StrokeThickness = 2 }; Canvas.SetLeft(s2, 532); Canvas.SetTop(s2, 297); Diagram.Children.Add(s2); }
            // DPad
            { var d1 = new Rectangle { Width = 26, Height = 74, Fill = new SolidColorBrush(Color.FromRgb(0xDD,0xDD,0xE4)), Stroke = new SolidColorBrush(Color.FromRgb(0x77,0x77,0x88)), StrokeThickness = 2 }; Canvas.SetLeft(d1, 382); Canvas.SetTop(d1, 343); Diagram.Children.Add(d1); }
            { var d2 = new Rectangle { Width = 74, Height = 26, Fill = new SolidColorBrush(Color.FromRgb(0xDD,0xDD,0xE4)), Stroke = new SolidColorBrush(Color.FromRgb(0x77,0x77,0x88)), StrokeThickness = 2 }; Canvas.SetLeft(d2, 358); Canvas.SetTop(d2, 367); Diagram.Children.Add(d2); }
            // Face buttons
            foreach (var b in Buttons.Where(b => b.Name is "A" or "B" or "X" or "Y"))
            {
                var fb = new Ellipse { Width = 40, Height = 40, Fill = new SolidColorBrush(Color.FromRgb(0x2E,0x2E,0x36)) };
                Canvas.SetLeft(fb, b.Dot.X - 20); Canvas.SetTop(fb, b.Dot.Y - 20);
                Diagram.Children.Add(fb);
            }
            // LB/RB bumps
            { var b1 = new Rectangle { Width = 70, Height = 24, RadiusX = 10, RadiusY = 10, Fill = new SolidColorBrush(Color.FromRgb(0xCC,0xCC,0xD4)), Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 2 }; Canvas.SetLeft(b1, 295); Canvas.SetTop(b1, 118); Diagram.Children.Add(b1); }
            { var b2 = new Rectangle { Width = 70, Height = 24, RadiusX = 10, RadiusY = 10, Fill = new SolidColorBrush(Color.FromRgb(0xCC,0xCC,0xD4)), Stroke = new SolidColorBrush(Color.FromRgb(0x88,0x88,0x99)), StrokeThickness = 2 }; Canvas.SetLeft(b2, 555); Canvas.SetTop(b2, 118); Diagram.Children.Add(b2); }

            // Connector lines + labels
            foreach (var b in Buttons)
            {
                var line = new Line
                {
                    X1 = b.Dot.X, Y1 = b.Dot.Y,
                    X2 = b.Label.X - 10, Y2 = b.Label.Y + 14,
                    Stroke = new SolidColorBrush(Color.FromRgb(0xAA,0xAA,0xBB)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 3, 2 }
                };
                Diagram.Children.Add(line);
                var dot = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(Color.FromRgb(0x77,0x77,0x88)) };
                Canvas.SetLeft(dot, b.Dot.X - 3); Canvas.SetTop(dot, b.Dot.Y - 3);
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


