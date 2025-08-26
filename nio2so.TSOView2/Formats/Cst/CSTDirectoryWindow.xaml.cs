using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.Cst
{
    /// <summary>
    /// Interaction logic for CSTDirectoryWindow.xaml
    /// </summary>
    public partial class CSTDirectoryWindow : Window
    {
        public CSTDirectoryWindow(Window Owner, DirectoryInfo CSTDirectory = default)
        {
            this.Owner = Owner;

            InitializeComponent();
            if (CSTDirectory != default)
            {
                try
                {
                    CSTControl.OpenDirectory(CSTDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open CST Directory '{CSTDirectory.FullName}': {ex.Message}", "Error Opening CST Directory", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            Loaded += CSTDirectoryWindow_Loaded;
        }
        // only load if no directory is open -- this will check by using the CSTControl's IsDirectoryOpen property
        private void CSTDirectoryWindow_Loaded(object sender, RoutedEventArgs e) => _ = !CSTControl.IsDirectoryOpen ? CSTControl.PromptAndOpenDirectory() : true; // lol nice hack around void return type :) // thanks, me :)

        private void OpenCSTDirectoryItem_Click(object sender, RoutedEventArgs e) => CSTControl.PromptAndOpenDirectory();

        private void CloseItem_Click(object sender, RoutedEventArgs e) => Close();
    }
}
