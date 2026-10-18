using System;
using System.IO;
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
        private MappingVisualPage? _visualPage;
        private GamepadTestPage? _testPage;
        private QuickMenuWindow? _quickMenu;
        private DateTime _yDownAt = DateTime.MinValue;
        private bool _yLongHandled;
        private bool _updating;
        private bool _realExit;
        public void RealExit() { _realExit = true; Close(); Application.Current.Shutdown(); }

        public MainWindow()
        {
            InitializeComponent();
            Closing += (_, e) =>
            {
                if (!_realExit) { e.Cancel = true; Hide(); } // minimize to tray instead of exit
            };
            _configuration = _store.Load();
            ProfileStore.EnsureDefaults(_configuration);
            AppRuleLists.Load(AppPaths.ConfigDir);
            App.Gamepad.Tick += OnGamepadTick;
            DesktopRecovery.Notice += msg => { try { File.AppendAllText(System.IO.Path.Combine(AppPaths.LogsDir, "runtime.log"), DateTime.Now.ToString("HH:mm:ss ") + msg + Environment.NewLine); } catch { } };
            DesktopRecovery.RuntimeLog += msg => { try { File.AppendAllText(System.IO.Path.Combine(AppPaths.LogsDir, "runtime.log"), DateTime.Now.ToString("HH:mm:ss ") + msg + Environment.NewLine); } catch { } };
            _mappingPage = new MappingEditorPage(_language, _store, _configuration);
            _visualPage = new MappingVisualPage(_configuration);
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
            NavList.Items.Add("高级编辑");
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
            if (index == 1 && _visualPage != null)
            {
                PageDescription.Text = "";
                PageHost.Content = _visualPage;
            }
            else if (index == 2 && _testPage != null)
            {
                PageDescription.Text = "";
                PageHost.Content = _testPage;
            }
            else if (index == 3 && _mappingPage != null)
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
                    4 => _language.T("ProfilesPlaceholder"),
                    5 => _language.T("DriverPlaceholder"),
                    6 => _language.T("SettingsPlaceholder"),
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

                        private bool _mappingEnabled = true;

        private bool ComputeMappingEnabled()
        {
            var info = ForegroundWatcher.GetForegroundInfo();
            if (info == null || string.IsNullOrEmpty(info.ProcessName)) return true;
            var process = info.ProcessName;
            if (process == "ControllerDesktop") return true;
            if (process is "哔哩哔哩" or "bilibili") return false;
            if (AppRuleLists.PassthroughGames.Contains(process)) return false;
            if (AppRuleLists.MouseGames.Contains(process)) return true;
            if (!string.IsNullOrEmpty(info.ProcessPath) &&
                info.ProcessPath.IndexOf("\\steamapps\\common\\", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            // Fullscreen no longer auto-passthrough (browsers/video players are fullscreen too).
            // Only passthrough if the game explicitly asks for it via passthrough-games.txt
            // or if it's a Steam game.
            return true;
        }

        private void OnGamepadTick(GamepadState previous, GamepadState current)
        {
            var profile = _configuration.Profiles.FirstOrDefault(p => p.Name == _configuration.CurrentProfile);
            if (profile == null) return;

            _mappingEnabled = ComputeMappingEnabled();
            if (!_mappingEnabled)
            {
                // Game/bilibili passthrough: hand everything to the game.
                DesktopRecovery.HandleChord(previous, current); // X+B still works as emergency exit
                if (current.Has(GamepadButton.Y) && !previous.Has(GamepadButton.Y))
                    ShowQuickMenu(); // Y still opens menu so user can force-recover
                DesktopRecovery.StopStickTracking();
                return;
            }

            DesktopRecovery.HandleChord(previous, current);
            Dispatcher.BeginInvoke(() => { HandleYButton(previous, current); HandleMenuNav(previous, current); });
            if (_quickMenu != null)
            {
                // Safety: if the menu window leaked (never shown / closed without event), clear it.
                if (!_quickMenu.IsLoaded) _quickMenu = null;
                else return; // menu open: skip mappings
            }

            var stickActive = Math.Abs(current.RightX) > 9000 || Math.Abs(current.RightY) > 9000;
            if (stickActive) DesktopRecovery.TrackStickActivity(); else DesktopRecovery.StopStickTracking();
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
                // Tap toggles the menu: open if closed, close if open.
                if (_quickMenu == null || !_quickMenu.IsLoaded) ShowQuickMenu();
                else CloseQuickMenu();
            }
            else if (pressedNow && !_yLongHandled && (DateTime.Now - _yDownAt).TotalMilliseconds >= 600)
            {
                _yLongHandled = true;
                CloseQuickMenu();
                App.Backend.KeyTap("ALT+TAB");
            }
            else if (!pressedNow && pressedBefore)
            {
                _yDownAt = DateTime.MinValue;
                _yLongHandled = false;
            }
        }

        private void HandleMenuNav(GamepadState previous, GamepadState current)
        {
            if (_quickMenu == null || !_quickMenu.IsLoaded) return;
            if (current.Has(GamepadButton.DpadUp) && !previous.Has(GamepadButton.DpadUp)) _quickMenu.MoveSelection(-1);
            if (current.Has(GamepadButton.DpadDown) && !previous.Has(GamepadButton.DpadDown)) _quickMenu.MoveSelection(1);
            if (current.Has(GamepadButton.A) && !previous.Has(GamepadButton.A)) _quickMenu.RunCurrent();
        }

        private void ShowQuickMenu()
        {
            if (_quickMenu != null) return;
            var items = new List<(string, Action)>
            {
                ("强制恢复桌面控制", () => DesktopRecovery.ForceRecover("手动恢复")),
                ("最小化当前窗口", () => App.Backend.KeyTap("LWIN+DOWN")),
                ("关闭当前窗口", () => App.Backend.KeyTap("ALT+F4")),
                ("切换窗口 (Alt+Tab)", () => App.Backend.KeyTap("ALT+TAB")),
                ("显示桌面 (Win+D)", () => App.Backend.KeyTap("LWIN+D")),
                ("返回上一页 (Alt+←)", () => App.Backend.KeyTap("ALT+LEFT")),
                ("全屏切换 (F11)", () => App.Backend.KeyTap("F11")),
                ("播放/暂停", () => App.Backend.KeyTap("MEDIA_PLAY")),
                ("音量+", () => App.Backend.KeyTap("VOLUME_UP")),
                ("音量-", () => App.Backend.KeyTap("VOLUME_DOWN")),
                ("静音", () => App.Backend.KeyTap("VOLUME_MUTE"))
            };
            try
            {
                _quickMenu = new QuickMenuWindow(items);
                _quickMenu.Closed += (_, _) => _quickMenu = null;
                _quickMenu.Show();
                _quickMenu.Activate();
            }
            catch
            {
                _quickMenu = null;
            }
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















