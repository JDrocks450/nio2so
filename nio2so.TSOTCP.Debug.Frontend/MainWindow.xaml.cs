using nio2so.TSOTCP.Debug.Frontend.Windows;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOTCP.Debug.Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "ButtonPacketLog":
                    PacketLogWindow wnd = new();
                    wnd.Closed += delegate
                    {
                        Show();    
                    };
                    wnd.Show();
                    Hide();
                    break;
                default:
                    Close();
                    break;
            }
        }
    }
}