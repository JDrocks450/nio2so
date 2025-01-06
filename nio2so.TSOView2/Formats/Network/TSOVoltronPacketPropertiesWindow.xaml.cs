using Microsoft.Win32;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2.Formats.Network
{
    /// <summary>
    /// Interaction logic for TSOVoltronPacketPropertiesWindow.xaml
    /// </summary>
    public partial class TSOVoltronPacketPropertiesWindow : Window
    {
        public string CurrentFile
        {
            get; private set;
        }

        public TSOVoltronPacketPropertiesWindow()
        {
            InitializeComponent();

            Loaded += WindowLoaded;
        }        

        public TSOVoltronPacketPropertiesWindow(string PDUFileURI) : this()
        {
            CurrentFile = PDUFileURI;
        }
        /// <summary>
        /// Prompts the user to select a new PDU from disk and then returns the <see cref="Window"/>
        /// created
        /// </summary>
        /// <param name="Window"></param>
        /// <returns></returns>
        public static bool TryPromptUserAndCreateDialog(out TSOVoltronPacketPropertiesWindow? Window)
        {
            Window = null;

            //*prompt user
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Multiselect = false,
                Title = "Open a TSOVoltronPacket in Binary Format (*.dat)",
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true
            };
            if (!dlg?.ShowDialog() ?? true)
                return false; // User cancel or error ??

            Window = new(dlg.FileName);
            return true;
        }
        /// <summary>
        /// Prompts the user to select a new PDU from disk and then shows the window without blocking
        /// execution
        /// </summary>
        /// <param name="Window"></param>
        /// <returns></returns>
        public static bool TryPromptUserAndShowDialog(out TSOVoltronPacketPropertiesWindow? Window)
        {
            if (!TryPromptUserAndCreateDialog(out Window)) return false;
            Window.Show();
            return true;
        }
        /// <summary>
        /// Prompts the user to select a new PDU from disk and then shows the window while blocking
        /// execution until its closure
        /// </summary>
        /// <param name="Window"></param>
        /// <returns></returns>
        public static bool TryPromptUserAndShowAsDialog(out TSOVoltronPacketPropertiesWindow? Window)
        {
            if (!TryPromptUserAndCreateDialog(out Window)) return false;
            Window.ShowDialog();
            return true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentFile != default)
                DisplayPDU(CurrentFile);
        }

        private void CloseItem_Click(object sender, RoutedEventArgs e) => Close();

        private void OpenAnotherItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TryPromptUserAndShowDialog(out _);
        }

        public bool DisplayPDU(string PDUFileURI) => PropertiesControl.DisplayPDU(PDUFileURI);
    }
}
