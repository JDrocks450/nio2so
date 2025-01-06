using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.Network
{

    /// <summary>
    /// Interaction logic for TSOVoltronPacketPropertiesControl.xaml
    /// </summary>
    public partial class TSOVoltronPacketPropertiesControl : UserControl, IDisposable
    {
        private TSOVoltronPacket? _currentPDU;

        public string CurrentFile
        {
            get; private set;
        }
        /// <summary>
        /// Dictates whether to show runtime values on this PDU
        /// </summary>
        public bool ShowValues
        {
            get; set;
        } = true;
        /// <summary>
        /// Dictates whether when this tool finds a null value it will call the default
        /// constuctor to initialize the value
        /// </summary>
        public bool AutoInitializeValues
        {
            get; set;
        } = false;

        public TSOVoltronPacketPropertiesControl()
        {
            InitializeComponent();
        }

        public bool DisplayPDU(string PDUFileURI, bool ShowValues = true)
        {
            this.ShowValues = ShowValues;

            //CLEAR PREVIOUS VALUE
            Dispose();

            CurrentFile = PDUFileURI;

            //LOAD FROM FILE
            try
            {
                using (FileStream stream = File.OpenRead(PDUFileURI))
                    _currentPDU = TSOPDUFactory.CreatePacketObjectFromDataBuffer(stream);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error Occurred!");
            }

            if (_currentPDU == null) return false;

            //DISPLAY PROPERTIES (SERIALIZATION GRAPH)
            ShowHierarchyProperties();

            return true;
        }

        public bool DisplayPDU(TSOVoltronPacket Packet, bool ShowValues = true)
        {
            this.ShowValues = ShowValues;
            _currentPDU = Packet;
            ShowHierarchyProperties();
            return true;
        }

        /// <summary>
        /// Will display the serialization graph of the loaded PDU (<see cref="_currentPDU"/>)
        /// </summary>
        private void ShowHierarchyProperties()
        {
            if (_currentPDU == null) return;

            _currentPDU.MakeBodyFromProperties();
            var graph = _currentPDU.MySerializedGraph;

            TreeViewItem rootNode = new();

            void ProcessOne(TreeViewItem ParentItem, TSOVoltronSerializerGraphItem CurrentItem)
            {
                if (CurrentItem != null && CurrentItem.SerializedValue == null && AutoInitializeValues)
                {
                    if (CurrentItem.SerializedType.IsClass)
                    {
                        try
                        {
                            CurrentItem.SerializedValue = Activator.CreateInstance(CurrentItem.SerializedType);
                            TSOVoltronSerializer.Serialize(CurrentItem.SerializedValue);
                            CurrentItem = TSOVoltronSerializer.GetLastGraph();
                        }
                        catch (MissingMethodException) { }
                    }
                }
                string text = CurrentItem != null ?
                    $"[{CurrentItem.SerializedType.Name}] {CurrentItem.PropertyName}" +
                    (ShowValues ? $": {CurrentItem.SerializedValueStringFormat}" : "") :
                    "ERROR: Serialization frame was null.";
                TreeViewItem currentItem = new()
                {
                    Header = text,
                    IsExpanded = true
                };
                ParentItem.IsExpanded = true;
                ParentItem.Items.Add(currentItem);
                if (CurrentItem != null)
                    foreach (var graphItem in CurrentItem)
                        ProcessOne(currentItem, graphItem);
            }

            ProcessOne(rootNode, graph);
            TreeViewItem newRootNode = (TreeViewItem)rootNode.Items[0];
            rootNode.Items.Remove(newRootNode);
            rootNode = newRootNode;

            PDUPropertyTree.Items.Clear();
            PDUPropertyTree.Items.Add(rootNode);
        }

        /// <summary>
        /// Disposes the PDU loaded (if applicable)
        /// <para/>This Window is still entirely usable after this, as <see cref="Window"/> itself
        /// is not a disposable type. This will only dispose the <see cref="TSOVoltronPacket"/> this
        /// instance contains, if there is one.
        /// </summary>
        public void Dispose()
        {
            if (_currentPDU != null)
            {
                _currentPDU.Dispose();
                _currentPDU = null;
            }
        }
    }
}
