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

        // Vector controller layout: Dot = exact key center on the drawing; Label = binding chip top-left
        private static readonly (string Name, Point Dot, Point Label)[] Buttons =
        {
            ("Y",     new Point(450, 205), new Point(700, 210)),
            ("X",     new Point(405, 240), new Point(700, 250)),
            ("B",     new Point(495, 240), new Point(700, 290)),
            ("A",     new Point(450, 275), new Point(700, 330)),
            ("LT",    new Point(330,  95), new Point( 20, 100)),
            ("RT",    new Point(570,  95), new Point(700, 100)),
            ("LB",    new Point(300, 155), new Point( 20, 160)),
            ("RB",    new Point(600, 155), new Point(700, 150)),
            ("LS",    new Point(340, 285), new Point( 20, 240)),
            ("RS",    new Point(560, 340), new Point(700, 430)),
            ("DPAD",  new Point(450, 330), new Point( 20, 330)),
            ("BACK",  new Point(415, 365), new Point( 20, 400)),
            ("START", new Point(485, 365), new Point(700, 380))
        };

        private static readonly Dictionary<string, double> DotRadius = new()
        {
            ["Y"] = 20, ["X"] = 20, ["B"] = 20, ["A"] = 20,
            ["LT"] = 26, ["RT"] = 26, ["LB"] = 22, ["RB"] = 22,
            ["LS"] = 30, ["RS"] = 30, ["DPAD"] = 30, ["BACK"] = 14, ["START"] = 14
        };

        private void DrawController()
        {
            Diagram.Children.Clear();
            _labels.Clear();

            DrawControllerBody();
            DrawControllerControls();

            foreach (var b in Buttons)
            {
                DrawMarker(b.Name, b.Dot);

                double r = DotRadius[b.Name];
                bool leftSide = b.Label.X < 300;
                var line = new Line
                {
                    X1 = b.Dot.X + (leftSide ? -r : r),
                    Y1 = b.Dot.Y,
                    X2 = leftSide ? b.Label.X + 124 : b.Label.X - 10,
                    Y2 = b.Label.Y + 14,
                    Stroke = new SolidColorBrush(Color.FromRgb(0xB4, 0xB6, 0xC4)),
                    StrokeThickness = 1.5,
                    StrokeDashArray = new DoubleCollection { 3, 2 }
                };
                Diagram.Children.Add(line);

                var border = new Border
                {
                    Width = 118, Height = 28,
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2B, 0x31)),
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
                border.MouseEnter += (_, _) => border.Background = new SolidColorBrush(Color.FromRgb(0x3D, 0x40, 0x4C));
                border.MouseLeave += (_, _) => border.Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2B, 0x31));
                border.MouseLeftButtonDown += Label_Click;
                border.DragEnter += Label_DragEnter;
                border.Drop += Label_Drop;
                border.DragOver += (_, e) => e.Effects = DragDropEffects.Copy;
                Diagram.Children.Add(border);
            }
            RefreshLabels();
        }

        private void DrawControllerBody()
        {
            var fill = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF8));
            var stroke = new SolidColorBrush(Color.FromRgb(0xD5, 0xD7, 0xE2));

            // grips
            Diagram.Children.Add(new Ellipse
            {
                Width = 140, Height = 230,
                Fill = fill, Stroke = stroke, StrokeThickness = 2,
                RenderTransform = new RotateTransform(-18)
            });
            Canvas.SetLeft(Diagram.Children[^1], 210);
            Canvas.SetTop(Diagram.Children[^1], 300);

            Diagram.Children.Add(new Ellipse
            {
                Width = 140, Height = 230,
                Fill = fill, Stroke = stroke, StrokeThickness = 2,
                RenderTransform = new RotateTransform(18)
            });
            Canvas.SetLeft(Diagram.Children[^1], 550);
            Canvas.SetTop(Diagram.Children[^1], 300);

            // main body
            Diagram.Children.Add(new Border
            {
                Width = 440, Height = 250,
                CornerRadius = new CornerRadius(110, 110, 70, 70),
                Background = fill,
                BorderBrush = stroke,
                BorderThickness = new Thickness(2)
            });
            Canvas.SetLeft(Diagram.Children[^1], 230);
            Canvas.SetTop(Diagram.Children[^1], 155);
        }

        private void DrawControllerControls()
        {
            var keyFill = new SolidColorBrush(Color.FromRgb(0xE7, 0xE9, 0xF1));
            var keyStroke = new SolidColorBrush(Color.FromRgb(0xB9, 0xBC, 0xCB));
            var text = new SolidColorBrush(Color.FromRgb(0x5A, 0x5C, 0x68));

            // Triggers (top layer, above bumpers like a real Xbox controller)
            AddKeyShape("LT", 300, 70, 64, 40, 12, keyFill, keyStroke, "LT");
            AddKeyShape("RT", 536, 70, 64, 40, 12, keyFill, keyStroke, "RT");
            // Bumpers
            AddKeyShape("LB", 262, 132, 78, 30, 12, keyFill, keyStroke, "LB");
            AddKeyShape("RB", 560, 132, 78, 30, 12, keyFill, keyStroke, "RB");

            // Face buttons
            AddCircle("Y", 450, 205, 19, keyFill, keyStroke, "Y");
            AddCircle("X", 405, 240, 19, keyFill, keyStroke, "X");
            AddCircle("B", 495, 240, 19, keyFill, keyStroke, "B");
            AddCircle("A", 450, 275, 19, keyFill, keyStroke, "A");

            // Sticks
            AddStick("LS", 340, 285);
            AddStick("RS", 560, 340);

            // D-pad cross
            foreach (var (x, y, w, h) in new[] { (438, 306, 24, 48), (426, 318, 48, 24) })
            {
                Diagram.Children.Add(new Rectangle
                {
                    Width = w, Height = h,
                    RadiusX = 5, RadiusY = 5,
                    Fill = keyFill, Stroke = keyStroke, StrokeThickness = 2
                });
                Canvas.SetLeft(Diagram.Children[^1], x);
                Canvas.SetTop(Diagram.Children[^1], y);
            }

            // BACK / START
            AddKeyShape("BACK", 401, 354, 28, 20, 8, keyFill, keyStroke, "◁");
            AddKeyShape("START", 471, 354, 28, 20, 8, keyFill, keyStroke, "▷");
        }

        private void AddKeyShape(string tag, double x, double y, double w, double h, double radius, Brush fill, Brush stroke, string caption)
        {
            Diagram.Children.Add(new Border
            {
                Width = w, Height = h,
                CornerRadius = new CornerRadius(radius),
                Background = fill,
                BorderBrush = stroke,
                BorderThickness = new Thickness(2),
                Child = new TextBlock
                {
                    Text = caption,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x5C, 0x68)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            Canvas.SetLeft(Diagram.Children[^1], x);
            Canvas.SetTop(Diagram.Children[^1], y);
        }

        private void AddCircle(string tag, double cx, double cy, double r, Brush fill, Brush stroke, string caption)
        {
            Diagram.Children.Add(new Ellipse { Width = r * 2, Height = r * 2, Fill = fill, Stroke = stroke, StrokeThickness = 2 });
            Canvas.SetLeft(Diagram.Children[^1], cx - r);
            Canvas.SetTop(Diagram.Children[^1], cy - r);
            Diagram.Children.Add(new TextBlock
            {
                Text = caption,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x5C, 0x68))
            });
            Canvas.SetLeft(Diagram.Children[^1], cx - 5);
            Canvas.SetTop(Diagram.Children[^1], cy - 8);
        }

        private void AddStick(string tag, double cx, double cy)
        {
            Diagram.Children.Add(new Ellipse
            {
                Width = 62, Height = 62,
                Fill = new SolidColorBrush(Color.FromRgb(0xE7, 0xE9, 0xF1)),
                Stroke = new SolidColorBrush(Color.FromRgb(0xB9, 0xBC, 0xCB)),
                StrokeThickness = 2
            });
            Canvas.SetLeft(Diagram.Children[^1], cx - 31);
            Canvas.SetTop(Diagram.Children[^1], cy - 31);
            Diagram.Children.Add(new Ellipse
            {
                Width = 34, Height = 34,
                Fill = new SolidColorBrush(Color.FromRgb(0xDD, 0xE0, 0xEA)),
                Stroke = new SolidColorBrush(Color.FromRgb(0xB9, 0xBC, 0xCB)),
                StrokeThickness = 2
            });
            Canvas.SetLeft(Diagram.Children[^1], cx - 17);
            Canvas.SetTop(Diagram.Children[^1], cy - 17);
        }

        private void DrawMarker(string name, Point p)
        {
            var dot = new Ellipse
            {
                Width = 10, Height = 10,
                Stroke = new SolidColorBrush(Color.FromRgb(0x5A, 0x5C, 0x68)),
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(dot, p.X - 5);
            Canvas.SetTop(dot, p.Y - 5);
            Diagram.Children.Add(dot);
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
                Background = new SolidColorBrush(Color.FromRgb(0xE6, 0xE6, 0xEE)),
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
