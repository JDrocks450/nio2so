using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.PreAlpha.Protocol.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace nio2so.TSOView2.Formats.Network
{
    /// <summary>
    /// Interaction logic for TSOPacketLibraryControl.xaml
    /// </summary>
    public partial class TSOPacketLibraryControl : UserControl
    {
        record TypeNavigationToken : NavigationToken
        {
            public Type DisplayType { get; }
            
            public TypeNavigationToken(Type DisplayType, TreeViewItem NavigationNode) : base(DisplayType.Name, NavigationNode)
            {
                this.DisplayType = DisplayType;
            }

            protected TypeNavigationToken(NavigationToken original) : base(original)
            {
            }
        }
        record NavigationToken(string Token, TreeViewItem NavigationNode)
        {            
            public Dictionary<string, NavigationToken> NestedTokens { get; } = new();            

            public NavigationToken EnsureToken(string Name, TreeViewItem Node)
            {
                if (NestedTokens.ContainsKey(Name)) return NestedTokens[Name];
                NavigationToken token = new(Name, Node);
                basic(token);
                return token;
            }
            public TypeNavigationToken EnsureToken(Type TypedNode, TreeViewItem Node)
            {
                string Name = TypedNode.Name;
                if (NestedTokens.ContainsKey(Name)) return (TypeNavigationToken)NestedTokens[Name];
                TypeNavigationToken token = new(TypedNode, Node);
                basic(token);
                return token;
            }
            private void basic(NavigationToken token)
            {
                string Name = token.Token;
                NestedTokens.Add(Name, token);
                var node = token.NavigationNode;
                NavigationNode.Items.Add(node);
                node.Header = Name;
                node.IsExpanded = true;
            }
        }

        private readonly Dictionary<TreeViewItem, TypeNavigationToken> _selectableTypes = new();
        private string searchFilter = "";
        public TSOPacketLibraryControl()
        {
            InitializeComponent();

            VoltronTab.IsSelected = true;
        }

        private void Load_VoltronPanel()
        {
            //Load the navigation tree with Voltron.PreAlpha.Protocol packets
            Load_VoltronNavigationTree([typeof(TSOPreAlphaPDUFactory).Assembly]);
        }

        private void Load_VoltronNavigationTree(Assembly[] ProtocolAssemblies, Type? BasicPacketType = default)
        {
            _selectableTypes.Clear();

            if (BasicPacketType == default) BasicPacketType = typeof(TSOVoltronPacket);

            Assembly protocolAssembly = BasicPacketType.Assembly;
            HashSet<Assembly> protocols = [.. ProtocolAssemblies];
            protocols.Add(protocolAssembly);

            TreeViewItem root = new() { Header = "nio2so", IsExpanded = true };            

            foreach(var assembly in protocols)
            {
                NavigationToken baseNode = new(assembly.FullName, root);
                foreach (Type definedType in assembly.ExportedTypes)
                {
                    if (!definedType.IsAssignableTo(BasicPacketType)) continue;
                    if (!DoSearchResult(definedType)) continue;
                    string[] tokens = definedType.FullName.Split('.');
                    NavigationToken current = baseNode;
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        if (i == 0) continue;
                        string token = tokens[i];
                        current = current.EnsureToken(token, new());
                    }
                    var typeNode = current.EnsureToken(definedType, new());
                    if (!_selectableTypes.TryAdd(typeNode.NavigationNode, typeNode))
                    {
                        typeNode.NavigationNode.Foreground = System.Windows.Media.Brushes.Red;
                        continue; // skip this type, it is already added
                    }
                    typeNode.NavigationNode.Selected += Voltron_TypeSelected;
                }
            }            

            NamespaceTypesListing.Items.Clear();
            NamespaceTypesListing.Items.Add(root);            
        }    

        private bool DoSearchResult(Type Type)
        {
            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                if (!Type.Name.ToLower().Contains(searchFilter.ToLower()))
                    return false;
            }
            return true; // let everything pass if there's no search filter
        }

        private void Voltron_TypeSelected(object sender, RoutedEventArgs e)
        {
            if (!_selectableTypes.TryGetValue((TreeViewItem)sender, out TypeNavigationToken? typeNav) || typeNav == default)
            {
                MessageBox.Show("This selection could not be found. Please make your selection again.");
                return;
            }
            try
            {
                var packet = ActivateInstancePacket(typeNav);
                if (packet == null)                
                    throw new NullReferenceException("The reflected type could not be instantiated. It may not have a parameterless constructor.");
                VoltronPacketProperties.DisplayPDU(packet, false);
            }
            catch (Exception exception)
            {
                typeNav.NavigationNode.Foreground = System.Windows.Media.Brushes.Red;
                VoltronPacketProperties.DisplayException(exception);
            }
        }

        private TSOVoltronPacket? ActivateInstancePacket(TypeNavigationToken typeNav)
        {            
            Type type = typeNav.DisplayType;
            return (TSOVoltronPacket)type?.Assembly?.CreateInstance(type.FullName);
        }

        private void GenerateAll_Click(object sender, RoutedEventArgs e)
        {
            TSOVoltronSerializer.CreatingSerializationGraphs = true;

            StringBuilder builder = new();
            foreach (var typenavigationItem in _selectableTypes.Values)
            {
                TSOVoltronPacket? instance = null;
                try
                {
                    instance = ActivateInstancePacket(typenavigationItem);
                }
                catch { }
                if (instance == null) continue;

                instance.MakeBodyFromProperties();
                var graph = instance.MySerializedGraph;
                string text = TSOVoltronSerializer.GenerateSerializationSummary(graph, false, true);
                builder.AppendLine(text);
            }
            GenerateAll.Content = "Copied!";
            Clipboard.SetText(builder.ToString());
        }

        private void MainNavigationTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainNavigationTabs.SelectedItem == VoltronTab)
                Load_VoltronPanel();
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchBox.IsEnabled = false;
            searchFilter = SearchBox.Text;
            Load_VoltronPanel();
            SearchBox.IsEnabled = true;
        }
    }
}
