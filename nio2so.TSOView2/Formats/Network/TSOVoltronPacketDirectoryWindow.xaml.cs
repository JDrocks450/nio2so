using Microsoft.Win32;
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

namespace nio2so.TSOView2.Formats.Network
{
    /// <summary>
    /// Interaction logic for TSOVoltronPacketDirectoryWindow.xaml
    /// </summary>
    public partial class TSOVoltronPacketDirectoryWindow : Window
    {
        private readonly List<FileInfo> _selections = new();
        private readonly HashSet<Window> _myWindows = new();

        public string CurrentDirectory { get; private set; }

        public TSOVoltronPacketDirectoryWindow()
        {
            InitializeComponent();

            Loaded += WindowLoaded;
        }

        public TSOVoltronPacketDirectoryWindow(string Directory) : this()
        {
            CurrentDirectory = Directory;
        }

        /// <summary>
        /// Prompts the user to select a new PDU from disk and then returns the <see cref="Window"/>
        /// created
        /// </summary>
        /// <param name="Window"></param>
        /// <returns></returns>
        public static bool TryPromptUserAndCreateDialog(out TSOVoltronPacketDirectoryWindow? Window)
        {
            Window = null;

            //*prompt user
            OpenFolderDialog dlg = new OpenFolderDialog()
            {
                Multiselect = false,
                Title = "Open a Directory containing TSOVoltronPackets in Binary Format (*.dat)",
                ValidateNames = true,
                DereferenceLinks = true
            };
            if (!dlg?.ShowDialog() ?? true)
                return false; // User cancel or error ??

            Window = new(dlg.FolderName);
            return true;
        }
        /// <summary>
        /// Prompts the user to select a new PDU from disk and then shows the window without blocking
        /// execution
        /// </summary>
        /// <param name="Window"></param>
        /// <returns></returns>
        public static bool TryPromptUserAndShowDialog(out TSOVoltronPacketDirectoryWindow? Window)
        {
            if (!TryPromptUserAndCreateDialog(out Window)) return false;
            Window.Show();
            return true;
        }

        private void RefreshListing()
        {
            if (CurrentDirectory == null) return;

            DirectoryListing.Items.Clear();

            DirectoryInfo info = new DirectoryInfo(CurrentDirectory);
            if (!info.Exists) return;

            var results = info.EnumerateFiles("*.dat");

            foreach(FileInfo file in results)
            {
                string? filter = null;
                if (Filter_OutDirectionalFlag.IsChecked ?? false)
                    filter = "OUT";
                if (Filter_InDirectionalFlag.IsChecked ?? false)
                    filter = "IN";

                if (filter != null && !file.Name.StartsWith(filter))
                    continue;

                DirectoryListing.Items.Add(new ListBoxItem()
                {
                    Content = file.Name
                });
                _selections.Add(file);
            }
        }

        private void OpenPDU()
        {
            if (DirectoryListing.SelectedIndex < 0) return;
            var mySelection = _selections.ElementAtOrDefault(DirectoryListing.SelectedIndex);
            if (mySelection == default) return;

            TSOVoltronPacketPropertiesWindow dlg = new(mySelection.FullName);
            dlg.Closed += delegate
            {
                _myWindows.Remove(dlg);
            };
            _myWindows.Add(dlg);
            dlg.Show();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentDirectory != null)
                RefreshListing();
        }

        private void DirectoryListing_MouseDoubleClick(object sender, MouseButtonEventArgs e) => OpenPDU();
        private void OpenButton_Click(object sender, RoutedEventArgs e) => OpenPDU();
        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshListing();
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var wnd in _myWindows)
                wnd?.Close();
            Close();
        }
        private void Filter_BidirectionalFlag_Checked(object sender, RoutedEventArgs e) => RefreshListing();
    }
}
