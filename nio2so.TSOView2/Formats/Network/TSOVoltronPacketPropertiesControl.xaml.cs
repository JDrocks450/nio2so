﻿using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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

            TSOVoltronSerializer.CreatingSerializationGraphs = true;

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
                
                TreeViewItem currentItem = new()
                {
                    Header = GetCodeBlock(CurrentItem),
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

        TextBlock GetCodeBlock(TSOVoltronSerializerGraphItem CurrentItem)
        {
            TextBlock block = new TextBlock();

            //**error message
            if (CurrentItem == null)
            {
                block.Inlines.Add(new Run("ERROR: Serialization frame was null.") { Foreground = Brushes.Red });
                goto done;
            }

            void processAttributes(IEnumerable<Attribute> CustomAttributes)
            {
                if (CustomAttributes == null) 
                    return;

                System.Windows.FontWeight weight = FontWeights.Normal;
                System.Windows.FontStyle style = FontStyles.Italic;

                Run? lastRun = null;
                foreach(var attribute in CustomAttributes)
                {
                    if (!attribute.GetType().FullName.Contains("nio2so")) continue;
                    block.Inlines.Add(new Run("[") { FontStyle = style });
                    //processType(attribute.GetType(), weight, style);
                    block.Inlines.Add(new Run(attribute.GetType().Name) { Foreground = Brushes.Purple, FontStyle = style });
                    lastRun = new Run("]") { FontStyle = style };
                    block.Inlines.Add(lastRun);
                }
                if (lastRun != null) lastRun.Text += " ";
            }

            //**add type label
            void processType(Type type, FontWeight inheritWeight = default, FontStyle inheritStyle = default)
            {
                processAttributes(type.GetCustomAttributes());

                bool arraySwitch = type.IsArray;
                //**name of type only
                if (type.IsArray)
                    type = type.GetElementType() ?? typeof(object);
                block.Inlines.Add(new Run(type.Name)
                {
                    Foreground = (CurrentItem.SerializedType.IsClass) ? Brushes.Green : 
                                 (CurrentItem.SerializedType.IsEnum) ? Brushes.YellowGreen : Brushes.Blue,
                    FontWeight = inheritWeight,
                    FontStyle = inheritStyle
                });
                if (arraySwitch)
                    block.Inlines.Add(new Run($"[{(ShowValues ? (CurrentItem.SerializedValue as Array)?.Length ?? 0 : "")}]")
                    {
                        FontWeight = inheritWeight,
                        FontStyle = inheritStyle
                    });

            }
            processAttributes(CurrentItem?.PropertyInfo?.GetCustomAttributes());
            processType(CurrentItem.SerializedType, FontWeights.Bold); 
            //**add name label
            block.Inlines.Add(new Run(" " + CurrentItem.PropertyName)
            {
                Foreground = Brushes.Black
            });
            //**add runtime value label (if applicable)
            if (!ShowValues) goto done;

            block.Inlines.Add(new Run(" = ") { Foreground = Brushes.Black }); // = 
            block.Inlines.Add(new Run((CurrentItem.SerializedValueStringFormat ?? "default"))
            { // value in turquoise or langword in blue
                Foreground = CurrentItem.SerializedValueStringFormat == null ? Brushes.Blue : Brushes.Coral,
                FontWeight = FontWeights.Bold
            });
        done:
            return block;
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
