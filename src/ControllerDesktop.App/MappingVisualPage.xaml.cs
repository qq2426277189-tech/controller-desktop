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
        private string? _selectedButton;
        private readonly Dictionary<string, Border> _buttonListItems = new();
        private readonly Dictionary<string, UIElement> _diagramLabels = new();

        public event Action? MappingChanged;

        public MappingVisualPage(AppConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            LoadProfile();
            BuildButtonList();
            DrawController();
        }

        public void Reload(AppConfiguration configuration)
        {
            _configuration = configuration;
            LoadProfile();
            RefreshButtonList();
            RefreshDiagram();
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

        // 按键定义：名称、显示名、手柄上的位置（相对于图片）、标签框位置、是否在左侧
        // 图片信息：Controller111.png 宽639px 高550px，在Canvas中居中显示
        // ButtonPos 坐标是相对于639x550图片左上角的位置，指向按键中心点
        private static readonly (string Name, string DisplayName, Point ButtonPos, Point LabelPos, bool IsLeft)[] Buttons =
        {
            // 顶部扳机（上方的竖长方形按键）
            ("LT", "LT 扳机", new Point(205, 70), new Point(30, 20), true),
            ("RT", "RT 扳机", new Point(428, 70), new Point(650, 20), false),

            // 肩键（扳机下方的横条按键）
            ("LB", "LB 肩键", new Point(205, 120), new Point(30, 70), true),
            ("RB", "RB 肩键", new Point(428, 120), new Point(650, 70), false),

            // 左摇杆（左侧大圆圈）
            ("LS", "左摇杆", new Point(175, 235), new Point(30, 140), true),
            ("L3", "左杆按下", new Point(175, 235), new Point(30, 190), true),

            // 方向键（中下方的十字键）
            ("DPAD_UP", "方向键 ↑", new Point(245, 295), new Point(30, 250), true),
            ("DPAD_DOWN", "方向键 ↓", new Point(245, 345), new Point(30, 300), true),
            ("DPAD_LEFT", "方向键 ←", new Point(220, 320), new Point(30, 350), true),
            ("DPAD_RIGHT", "方向键 →", new Point(270, 320), new Point(30, 400), true),

            // 中间功能按键（两个小圆点）
            ("BACK", "BACK", new Point(260, 235), new Point(150, 500), true),
            ("START", "START", new Point(350, 235), new Point(450, 500), false),

            // 右侧XYAB按钮（右侧四叶草形状）
            ("Y", "Y 按钮", new Point(433, 210), new Point(650, 130), false),
            ("B", "B 按钮", new Point(458, 235), new Point(650, 180), false),
            ("A", "A 按钮", new Point(433, 260), new Point(650, 230), false),
            ("X", "X 按钮", new Point(408, 235), new Point(650, 280), false),

            // 右摇杆（右下方大圆圈）
            ("RS", "右摇杆", new Point(373, 320), new Point(650, 350), false),
            ("R3", "右杆按下", new Point(373, 320), new Point(650, 400), false),
        };

        #region 左侧按键列表

        private void BuildButtonList()
        {
            ButtonListPanel.Children.Clear();
            _buttonListItems.Clear();

            foreach (var btn in Buttons)
            {
                var item = CreateButtonListItem(btn.Name, btn.DisplayName);
                ButtonListPanel.Children.Add(item);
                _buttonListItems[btn.Name] = item;
            }
        }

        private Border CreateButtonListItem(string buttonName, string displayName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursors.Hand,
                Tag = buttonName
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var indicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var stack = new StackPanel();
            var nameText = new TextBlock
            {
                Text = displayName,
                FontWeight = FontWeights.Medium,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27))
            };

            var mappingText = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
                Margin = new Thickness(0, 2, 0, 0),
                Tag = "mapping"
            };

            stack.Children.Add(nameText);
            stack.Children.Add(mappingText);

            grid.Children.Add(indicator);
            grid.Children.Add(stack);
            Grid.SetColumn(stack, 1);

            border.Child = grid;

            border.MouseEnter += (s, e) =>
            {
                border.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
            };
            border.MouseLeave += (s, e) =>
            {
                if (_selectedButton != buttonName)
                    border.Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));
            };
            border.MouseLeftButtonDown += (s, e) => SelectButton(buttonName);

            UpdateButtonListItemMapping(border, buttonName);

            return border;
        }

        private void UpdateButtonListItemMapping(Border item, string buttonName)
        {
            var stack = ((item.Child as Grid)?.Children[1] as StackPanel);
            var mappingText = stack?.Children[1] as TextBlock;
            if (mappingText == null) return;

            var mapping = Lookup.Describe(buttonName);
            mappingText.Text = mapping == "无" ? "未配置" : mapping;
        }

        private void RefreshButtonList()
        {
            foreach (var kvp in _buttonListItems)
            {
                UpdateButtonListItemMapping(kvp.Value, kvp.Key);
            }
        }

        private void SelectButton(string buttonName)
        {
            _selectedButton = buttonName;

            foreach (var kvp in _buttonListItems)
            {
                kvp.Value.Background = kvp.Key == buttonName
                    ? new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE))
                    : new SolidColorBrush(Color.FromRgb(0xF9, 0xFA, 0xFB));
            }

            HighlightDiagramLabel(buttonName);
            ShowConfigPanel(buttonName);
        }

        #endregion

        #region 中间手柄图示

        private void DrawController()
        {
            DiagramCanvas.Children.Clear();
            _diagramLabels.Clear();

            var img = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Assets/Controller111.png")),
                Width = 639,
                Height = 550
            };
            Canvas.SetLeft(img, (900 - 639) / 2 + 50);
            Canvas.SetTop(img, 40);
            DiagramCanvas.Children.Add(img);

            foreach (var btn in Buttons)
            {
                // 图片在Canvas中的位置: left=(900-639)/2+50≈180, top=40
                var imageOffsetX = (900 - 639) / 2.0 + 50; // 约180
                var imageOffsetY = 40;

                // ButtonPos是相对于图片的坐标，需要加上图片的偏移得到Canvas坐标
                var adjustedButtonPos = new Point(btn.ButtonPos.X + imageOffsetX, btn.ButtonPos.Y + imageOffsetY);
                var adjustedLabelPos = new Point(btn.LabelPos.X, btn.LabelPos.Y);

                DrawButtonAnnotation(btn.Name, btn.DisplayName, adjustedButtonPos, adjustedLabelPos, btn.IsLeft);
            }

            RefreshDiagram();
        }

        private void DrawButtonAnnotation(string buttonName, string displayName, Point buttonPos, Point labelPos, bool isLeft)
        {
            var line = new Line
            {
                X1 = buttonPos.X, Y1 = buttonPos.Y,
                X2 = labelPos.X + (isLeft ? 100 : 10), Y2 = labelPos.Y + 16,
                Stroke = new SolidColorBrush(Color.FromArgb(100, 0x94, 0xA3, 0xB8)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            };
            DiagramCanvas.Children.Add(line);

            var dot = new Ellipse
            {
                Width = 12,
                Height = 12,
                Stroke = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                StrokeThickness = 2.5,
                Fill = Brushes.White
            };
            Canvas.SetLeft(dot, buttonPos.X - 6);
            Canvas.SetTop(dot, buttonPos.Y - 6);
            DiagramCanvas.Children.Add(dot);

            var labelBorder = new Border
            {
                Width = isLeft ? 100 : 100,
                Height = 32,
                Background = new SolidColorBrush(Color.FromArgb(245, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(6),
                Tag = buttonName,
                Cursor = Cursors.Hand
            };

            var labelStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var nameBlock = new TextBlock
            {
                Text = displayName,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var mappingBlock = new TextBlock
            {
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0),
                Tag = "mapping"
            };

            labelStack.Children.Add(nameBlock);
            labelStack.Children.Add(mappingBlock);
            labelBorder.Child = labelStack;

            Canvas.SetLeft(labelBorder, labelPos.X);
            Canvas.SetTop(labelBorder, labelPos.Y);

            labelBorder.MouseEnter += (s, e) =>
            {
                labelBorder.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
                labelBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
            };
            labelBorder.MouseLeave += (s, e) =>
            {
                if (_selectedButton != buttonName)
                {
                    labelBorder.Background = new SolidColorBrush(Color.FromArgb(245, 0xFF, 0xFF, 0xFF));
                    labelBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB));
                }
            };
            labelBorder.MouseLeftButtonDown += (s, e) => SelectButton(buttonName);

            DiagramCanvas.Children.Add(labelBorder);
            _diagramLabels[buttonName] = labelBorder;
        }

        private void RefreshDiagram()
        {
            foreach (var kvp in _diagramLabels)
            {
                if (kvp.Value is Border border && border.Child is StackPanel stack)
                {
                    var mappingBlock = stack.Children[1] as TextBlock;
                    if (mappingBlock != null)
                    {
                        var mapping = Lookup.Describe(kvp.Key);
                        mappingBlock.Text = mapping == "无" ? "未配置" : mapping;
                    }
                }
            }
        }

        private void HighlightDiagramLabel(string buttonName)
        {
            foreach (var kvp in _diagramLabels)
            {
                if (kvp.Value is Border border)
                {
                    if (kvp.Key == buttonName)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE));
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
                        border.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        border.Background = new SolidColorBrush(Color.FromArgb(245, 0xFF, 0xFF, 0xFF));
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB));
                        border.BorderThickness = new Thickness(1.5);
                    }
                }
            }
        }

        #endregion

        #region 右侧配置面板

        private void ShowConfigPanel(string buttonName)
        {
            ConfigPanel.Visibility = Visibility.Visible;
            CurrentButtonName.Text = Buttons.First(b => b.Name == buttonName).DisplayName;

            var existingBinding = _profile.Bindings.FirstOrDefault(b => b.Button == buttonName);

            if (existingBinding != null)
            {
                switch (existingBinding.Action)
                {
                    case ActionType.Key:
                        // 判断是否为组合键
                        if (existingBinding.Value.Contains("+"))
                        {
                            ActionTypeCombo.SelectedIndex = 1; // 组合键
                            var parts = existingBinding.Value.Split('+');
                            CtrlCheckBox.IsChecked = parts.Contains("CTRL");
                            ShiftCheckBox.IsChecked = parts.Contains("SHIFT");
                            AltCheckBox.IsChecked = parts.Contains("ALT");
                            WinCheckBox.IsChecked = parts.Contains("WIN");
                            var mainKey = parts.LastOrDefault(p => !new[] { "CTRL", "SHIFT", "ALT", "WIN" }.Contains(p));
                            CapturedComboKeyDisplay.Text = mainKey ?? "点击捕获主按键";
                            UpdateComboKeyPreview();
                        }
                        else
                        {
                            ActionTypeCombo.SelectedIndex = 0; // 单键
                            CapturedKeyDisplay.Text = existingBinding.Value;
                        }
                        break;
                    case ActionType.MouseLeft:
                        ActionTypeCombo.SelectedIndex = 2;
                        break;
                    case ActionType.MouseRight:
                        ActionTypeCombo.SelectedIndex = 3;
                        break;
                    case ActionType.MouseMiddle:
                        ActionTypeCombo.SelectedIndex = 4;
                        break;
                    case ActionType.MouseX1:
                        ActionTypeCombo.SelectedIndex = 5;
                        break;
                    case ActionType.MouseX2:
                        ActionTypeCombo.SelectedIndex = 6;
                        break;
                    case ActionType.Wheel:
                        ActionTypeCombo.SelectedIndex = existingBinding.Value == "up" ? 7 : 8;
                        break;
                    default:
                        ActionTypeCombo.SelectedIndex = 9; // 无操作
                        break;
                }
            }
            else
            {
                ActionTypeCombo.SelectedIndex = 9;
            }

            BuildQuickKeyPanel();
        }

        private void ActionType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ActionTypeCombo.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag as string;
                KeyInputPanel.Visibility = tag == "Key" ? Visibility.Visible : Visibility.Collapsed;
                ComboKeyInputPanel.Visibility = tag == "Combo" ? Visibility.Visible : Visibility.Collapsed;

                if (tag == "Combo")
                {
                    UpdateComboKeyPreview();
                }
            }
        }

        private void BuildQuickKeyPanel()
        {
            QuickKeyPanel.Children.Clear();

            var quickKeys = new[]
            {
                "ESC", "SPACE", "ENTER", "TAB",
                "UP", "DOWN", "LEFT", "RIGHT",
                "W", "A", "S", "D",
                "Q", "E", "R", "F",
                "1", "2", "3", "4",
                "CTRL", "ALT", "SHIFT",
                "F1", "F2", "F11", "F12"
            };

            foreach (var key in quickKeys)
            {
                var btn = new Button
                {
                    Content = key,
                    Width = 56,
                    Height = 32,
                    Margin = new Thickness(0, 0, 6, 6),
                    Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),
                    BorderThickness = new Thickness(1),
                    FontSize = 11,
                    Tag = key
                };
                btn.Click += QuickKey_Click;
                QuickKeyPanel.Children.Add(btn);
            }
        }

        private void QuickKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string key)
            {
                CapturedKeyDisplay.Text = key;
            }
        }

        private void CaptureKey_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedButton == null) return;
            var dlg = new CaptureKeyWindow(_selectedButton) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
            if (dlg.Result != null)
            {
                CapturedKeyDisplay.Text = dlg.Result;
            }
        }

        private void SaveMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedButton == null) return;

            var existing = _profile.Bindings.FirstOrDefault(b => b.Button == _selectedButton);

            if (ActionTypeCombo.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag as string;

                if (tag == "None")
                {
                    if (existing != null) _profile.Bindings.Remove(existing);
                }
                else
                {
                    if (existing == null)
                    {
                        existing = new ButtonBinding { Button = _selectedButton };
                        _profile.Bindings.Add(existing);
                    }

                    switch (tag)
                    {
                        case "Key":
                            existing.Action = ActionType.Key;
                            existing.Value = CapturedKeyDisplay.Text == "点击捕获按键" ? "" : CapturedKeyDisplay.Text;
                            break;
                        case "Combo":
                            existing.Action = ActionType.Key; // 组合键也存为Key类型
                            var comboParts = new List<string>();
                            if (CtrlCheckBox.IsChecked == true) comboParts.Add("CTRL");
                            if (ShiftCheckBox.IsChecked == true) comboParts.Add("SHIFT");
                            if (AltCheckBox.IsChecked == true) comboParts.Add("ALT");
                            if (WinCheckBox.IsChecked == true) comboParts.Add("WIN");
                            var mainKey = CapturedComboKeyDisplay.Text;
                            if (mainKey != "点击捕获主按键") comboParts.Add(mainKey);
                            existing.Value = string.Join("+", comboParts);
                            break;
                        case "MouseLeft":
                            existing.Action = ActionType.MouseLeft;
                            existing.Value = "";
                            break;
                        case "MouseRight":
                            existing.Action = ActionType.MouseRight;
                            existing.Value = "";
                            break;
                        case "MouseMiddle":
                            existing.Action = ActionType.MouseMiddle;
                            existing.Value = "";
                            break;
                        case "MouseX1":
                            existing.Action = ActionType.MouseX1;
                            existing.Value = "";
                            break;
                        case "MouseX2":
                            existing.Action = ActionType.MouseX2;
                            existing.Value = "";
                            break;
                        case "MouseWheelUp":
                            existing.Action = ActionType.Wheel;
                            existing.Value = "up";
                            break;
                        case "MouseWheelDown":
                            existing.Action = ActionType.Wheel;
                            existing.Value = "down";
                            break;
                    }
                }
            }

            new ProfileStore().Save(_configuration);
            RefreshButtonList();
            RefreshDiagram();
            MappingChanged?.Invoke();

            SaveMappingButton.Content = "✓ 已保存";
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            timer.Tick += (s, args) =>
            {
                SaveMappingButton.Content = "保存配置";
                timer.Stop();
            };
            timer.Start();
        }

        private void ClearMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedButton == null) return;

            var existing = _profile.Bindings.FirstOrDefault(b => b.Button == _selectedButton);
            if (existing != null)
            {
                _profile.Bindings.Remove(existing);
                new ProfileStore().Save(_configuration);
                RefreshButtonList();
                RefreshDiagram();
                MappingChanged?.Invoke();

                ActionTypeCombo.SelectedIndex = 4;
                CapturedKeyDisplay.Text = "点击捕获按键";
            }
        }

        #region 组合键处理

        private void CaptureComboKey_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new KeyCaptureDialog { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true)
            {
                CapturedComboKeyDisplay.Text = dlg.Result;
                UpdateComboKeyPreview();
            }
        }

        private void ModifierKey_Changed(object sender, RoutedEventArgs e)
        {
            UpdateComboKeyPreview();
        }

        private void UpdateComboKeyPreview()
        {
            var parts = new List<string>();

            if (CtrlCheckBox.IsChecked == true) parts.Add("Ctrl");
            if (ShiftCheckBox.IsChecked == true) parts.Add("Shift");
            if (AltCheckBox.IsChecked == true) parts.Add("Alt");
            if (WinCheckBox.IsChecked == true) parts.Add("Win");

            var mainKey = CapturedComboKeyDisplay.Text;
            if (mainKey != "点击捕获主按键")
            {
                parts.Add(mainKey);
            }

            ComboKeyPreview.Text = parts.Count > 0 ? string.Join(" + ", parts) : "未设置";
        }

        #endregion

        #endregion
    }

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
                ActionType.MouseX1 => "鼠标侧键1",
                ActionType.MouseX2 => "鼠标侧键2",
                ActionType.Wheel => b.Value == "up" ? "滚轮上" : "滚轮下",
                ActionType.Key => b.Value,
                ActionType.MediaKey => "媒体键",
                _ => "无"
            };
        }
    }
}
