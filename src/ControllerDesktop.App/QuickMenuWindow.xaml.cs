using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            HeaderText.Text = "快捷菜单";
            Render();
            MouseLeftButtonDown += (_, _) => RunCurrent();
        }

        public int SelectedIndex => _index;

        public void MoveSelection(int delta)
        {
            if (_items.Count == 0) return;
            _index = Math.Clamp(_index + delta, 0, _items.Count - 1);
            Render();
        }

        public void RunCurrent()
        {
            if (_index < 0 || _index >= _items.Count) return;
            var action = _items[_index].Run;
            Close();
            action();
        }

        private void Render()
        {
            List.Items.Clear();
            for (int i = 0; i < _items.Count; i++)
            {
                var selected = i == _index;
                var tb = new TextBlock
                {
                    Text = (selected ? "▶ " : "   ") + _items[i].Label,
                    Foreground = selected ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xCC,0xCC,0xCC)),
                    FontWeight = selected ? FontWeights.Bold : FontWeights.Normal,
                    FontSize = 14,
                    Padding = new Thickness(6, 4, 6, 4)
                };
                List.Items.Add(tb);
            }
        }
    }
}
