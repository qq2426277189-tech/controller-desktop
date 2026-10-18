using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ControllerDesktop.Core;
using ControllerDesktop.Core.Localization;
using ControllerDesktop.Core.Models;
using ControllerDesktop.Input;
using ControllerDesktop.Core.Profiles;

namespace ControllerDesktop.App
{
    public partial class MainWindow : Window
    {
        private readonly LanguageService _language = new();
        private readonly ProfileStore _store = new();
        private AppConfiguration _configuration;
        private MappingEditorPage? _mappingPage;
        private GamepadTestPage? _testPage;
        private QuickMenuWindow? _quickMenu;
        private DateTime _yDownAt = DateTime.MinValue;
        private bool _yLongHandled;
        private bool _updating;

        public MainWindow()
        {
            InitializeComponent();
            _configuration = _store.Load();
            ProfileStore.EnsureDefaults(_configuration);
            App.Gamepad.Tick += OnGamepadTick;
            _mappingPage = new MappingEditorPage(_language, _store, _configuration);
            _testPage = new GamepadTestPage(_language);
            _language.SetLanguage(_configuration.Language);
            Loaded += (_, _) => ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            _updating = true;
            Title = _language.T("AppTitle");
            TitleText.Text = _language.T("AppTitle");
            SubtitleText.Text = _language.T("FrameworkReady");
            ChineseButton.Content = "中文";
            EnglishButton.Content = "English";

            var selected = NavList.SelectedIndex;
            NavList.Items.Clear();
            NavList.Items.Add(_language.T("Dashboard"));
            NavList.Items.Add(_language.T("Mapping"));
            NavList.Items.Add(_language.T("GamepadTest"));
            NavList.Items.Add(_language.T("Profiles"));
            NavList.Items.Add(_language.T("Driver"));
            NavList.Items.Add(_language.T("Settings"));
            NavList.SelectedIndex = selected >= 0 ? selected : 0;
            _updating = false;

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0";
            VersionText.Text = _language.T("Version") + " " + version + "   |   " + _store.StorePath;
            UpdatePage();
        }

        private void UpdatePage()
        {
            var index = NavList.SelectedIndex;
            PageTitle.Text = NavList.SelectedItem as string ?? "";
            if (index == 2 && _testPage != null)
            {
                PageDescription.Text = "";
                PageHost.Content = _testPage;
            }
            else if (index == 1 && _mappingPage != null)
            {
                _mappingPage.Reload(_configuration);
                PageDescription.Text = "";
                PageHost.Content = _mappingPage;
            }
            else
            {
                PageHost.Content = null;
                PageDescription.Text = index switch
                {
                    3 => _language.T("ProfilesPlaceholder"),
                    4 => _language.T("DriverPlaceholder"),
                    5 => _language.T("SettingsPlaceholder"),
                    _ => _language.T("FrameworkReady")
                };
            }
            StatusText.Text = _language.T("CurrentProfile") + ": " + _configuration.CurrentProfile + Environment.NewLine +
                _language.T("Status") + ": " + _language.T("FrameworkReady");
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_updating) UpdatePage();
        }

                        private void OnGamepadTick(GamepadState previous, GamepadState current)
        {
            var profile = _configuration.Profiles.FirstOrDefault(p => p.Name == _configuration.CurrentProfile);
            if (profile == null) return;

            // Bilibili passthrough: no synthetic input while its window is foreground.
            if (ForegroundWatcher.IsBilibili()) return;

            HandleYButton(previous, current);
            if (_quickMenu != null) return; // menu open: skip mappings

            App.Engine.Apply(profile, previous, current);
        }

        private void HandleYButton(GamepadState previous, GamepadState current)
        {
            var pressedNow = current.Has(GamepadButton.Y);
            var pressedBefore = previous.Has(GamepadButton.Y);

            if (pressedNow && !pressedBefore)
            {
                _yDownAt = DateTime.Now;
                _yLongHandled = false;
            }
            else if (pressedNow && !_yLongHandled && (DateTime.Now - _yDownAt).TotalMilliseconds >= 600)
            {
                _yLongHandled = true;
                CloseQuickMenu();
                App.Backend.KeyTap("ALT+TAB");
            }
            else if (!pressedNow && pressedBefore)
            {
                if (!_yLongHandled) ShowQuickMenu();
                _yDownAt = DateTime.MinValue;
                _yLongHandled = false;
            }
        }

        private void ShowQuickMenu()
        {
            if (_quickMenu != null) return;
            var items = new List<(string, Action)>
            {
                ("切换窗口 (Alt+Tab)", () => App.Backend.KeyTap("ALT+TAB")),
                ("显示桌面 (Win+D)", () => App.Backend.KeyTap("LWIN+D")),
                ("返回上一页 (Alt+←)", () => App.Backend.KeyTap("ALT+LEFT")),
                ("全屏切换 (F11)", () => App.Backend.KeyTap("F11")),
                ("播放/暂停", () => App.Backend.KeyTap("MEDIA_PLAY")),
                ("音量+", () => App.Backend.KeyTap("VOLUME_UP")),
                ("音量-", () => App.Backend.KeyTap("VOLUME_DOWN")),
                ("静音", () => App.Backend.KeyTap("VOLUME_MUTE"))
            };
            _quickMenu = new QuickMenuWindow(items);
            _quickMenu.Closed += (_, _) => _quickMenu = null;
            _quickMenu.Show();
            _quickMenu.Activate();
        }

        private void CloseQuickMenu()
        {
            _quickMenu?.Close();
            _quickMenu = null;
        }

        private void Chinese_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Language = "zh-CN";
            _language.SetLanguage("zh-CN");
            _store.Save(_configuration);
            ApplyLanguage();
        }

        private void English_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Language = "en-US";
            _language.SetLanguage("en-US");
            _store.Save(_configuration);
            ApplyLanguage();
        }
    }
}









