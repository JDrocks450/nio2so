using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace nio2so.TSOView2
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        public static void ShowAboutBox() => new AboutWindow().Show();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", ((Hyperlink)sender).NavigateUri.OriginalString);
        }
    }
}
