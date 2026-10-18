using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ControllerDesktop.Core.Localization;
using ControllerDesktop.Core.Models;
using ControllerDesktop.Core.Profiles;

namespace ControllerDesktop.App
{
    public partial class MappingEditorPage : UserControl
    {
        private readonly LanguageService _language;
        private readonly ProfileStore _store;
        private AppConfiguration _configuration;
        private MappingProfile _profile = new();
        private ObservableCollection<ButtonBinding> _bindings = new();

        public MappingEditorPage(LanguageService language, ProfileStore store, AppConfiguration configuration)
        {
            InitializeComponent();
            ActionColumn.ItemsSource = Enum.GetValues(typeof(ActionType));
            _language = language;
            _store = store;
            _configuration = configuration;
            _language.LanguageChanged += (_, _) => ApplyLanguage();
            LoadProfile();
            ApplyLanguage();
        }

        public void Reload(AppConfiguration configuration)
        {
            _configuration = configuration;
            LoadProfile();
            ApplyLanguage();
        }

        private MappingProfile CurrentProfile
        {
            get
            {
                var profile = _configuration.Profiles.FirstOrDefault(p => p.Name == _configuration.CurrentProfile)
                              ?? _configuration.Profiles.FirstOrDefault();
                if (profile == null)
                {
                    profile = new MappingProfile { Name = _configuration.CurrentProfile };
                    _configuration.Profiles.Add(profile);
                }
                return profile;
            }
        }

        private void LoadProfile()
        {
            _profile = CurrentProfile;
            _bindings = new ObservableCollection<ButtonBinding>(_profile.Bindings);
            BindingsGrid.ItemsSource = _bindings;
            MouseSpeedBox.Text = _profile.LeftStick.MouseSpeed.ToString();
            DeadzoneBox.Text = _profile.LeftStick.Deadzone.ToString();
            SensitivityBox.Text = _profile.LeftStick.Sensitivity.ToString("0.##");
            InvertYBox.IsChecked = _profile.LeftStick.InvertY;
        }

        private void ApplyLanguage()
        {
            if (_profile == null) return;
            BindingsHeader.Text = _language.T("BindingsHeader");
            BindingsHint.Text = _language.T("BindingsHint");
            ButtonColumn.Header = _language.T("GamepadButton");
            ActionColumn.Header = _language.T("Action");
            ValueColumn.Header = _language.T("Target");
            StickHeader.Text = _language.T("StickSettingsHeader");
            MouseSpeedLabel.Text = _language.T("MouseSpeed");
            DeadzoneLabel.Text = _language.T("Deadzone");
            SensitivityLabel.Text = _language.T("Sensitivity");
            InvertYLabel.Text = _language.T("InvertY");
            SaveButton.Content = _language.T("Save");
            ResetButton.Content = _language.T("ResetChanges");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Commit pending cell edits so the newest row is included.
            BindingsGrid.CommitEdit(DataGridEditingUnit.Row, true);
            _bindings.RemoveWhere(b => string.IsNullOrWhiteSpace(b.Button) && b.Action == ActionType.None);
            _profile.Bindings = _bindings.ToList();

            if (int.TryParse(MouseSpeedBox.Text, out var speed)) _profile.LeftStick.MouseSpeed = Math.Clamp(speed, 1, 50);
            if (int.TryParse(DeadzoneBox.Text, out var deadzone)) _profile.LeftStick.Deadzone = Math.Clamp(deadzone, 0, 32000);
            if (double.TryParse(SensitivityBox.Text, out var sens)) _profile.LeftStick.Sensitivity = Math.Clamp(sens, 0.1, 5.0);
            _profile.LeftStick.InvertY = InvertYBox.IsChecked == true;

            _store.Save(_configuration);
            SaveStatus.Text = string.Format(_language.T("SavedProfile"), _profile.Name, DateTime.Now.ToString("HH:mm:ss"));
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            LoadProfile();
            SaveStatus.Text = "";
        }
    }

    internal static class ObservableCollectionExtensions
    {
        public static void RemoveWhere<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            for (var i = collection.Count - 1; i >= 0; i--)
                if (predicate(collection[i])) collection.RemoveAt(i);
        }
    }
}


