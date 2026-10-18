using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControllerDesktop.App
{
    public partial class QuickMenuWindow : Window
    {
        private readonly List<(string Label, Action Run)> _items;
        private int _index;

        public QuickMenuWindow(List<(string Label, Action Run)> items)
        {
            InitializeComponent();
            _items = items;
            List.ItemsSource = items;
            List.SelectedIndex = 0;
            Deactivated += (_, _) => Close();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _index = List.SelectedIndex;
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) => RunCurrent();

                private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _index = Math.Max(0, _index - 1); List.SelectedIndex = _index; e.Handled = true; break;
                case Key.Down: _index = Math.Min(_items.Count - 1, _index + 1); List.SelectedIndex = _index; e.Handled = true; break;
                case Key.A: RunCurrent(); e.Handled = true; break;
                case Key.B: Close(); e.Handled = true; break;
            }
        }

        private void RunCurrent()
        {
            if (_index < 0 || _index >= _items.Count) return;
            Close();
            _items[_index].Run();
        }
    }
}

