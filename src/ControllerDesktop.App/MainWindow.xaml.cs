using System.Windows;

namespace ControllerDesktop.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Chinese_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "ControllerDesktop";
            SubtitleText.Text = "手柄桌面控制 v2";
        }

        private void English_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "ControllerDesktop";
            SubtitleText.Text = "Gamepad desktop control v2";
        }
    }
}
