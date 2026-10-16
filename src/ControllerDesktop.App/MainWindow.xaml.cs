using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ControllerDesktop.Core;
using ControllerDesktop.Core.Localization;
using ControllerDesktop.Core.Models;
using ControllerDesktop.Core.Profiles;

namespace ControllerDesktop.App
{
    public partial class MainWindow : Window
    {
        private readonly LanguageService _language = new();
        private readonly ProfileStore _store = new();
        private AppConfiguration _configuration;
        private bool _updating;

        public MainWindow()
        {
            InitializeComponent();
            _configuration = _store.Load();
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
            PageDescription.Text = index switch
            {
                1 => _language.T("MappingPlaceholder"),
                2 => _language.T("ProfilesPlaceholder"),
                3 => _language.T("DriverPlaceholder"),
                4 => _language.T("SettingsPlaceholder"),
                _ => _language.T("FrameworkReady")
            };
            StatusText.Text = _language.T("CurrentProfile") + ": " + _configuration.CurrentProfile + Environment.NewLine +
                _language.T("Status") + ": " + _language.T("FrameworkReady");
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_updating) UpdatePage();
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

