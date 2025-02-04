﻿using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

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
            Load_VoltronNavigationTree();
        }

        private void Load_VoltronNavigationTree()
        {
            _selectableTypes.Clear();

            Type BasicPacketType = typeof(TSOVoltronPacket);

            Assembly protocolAssembly = BasicPacketType.Assembly;

            TreeViewItem root = new() { Header = "nio2so", IsExpanded = true };
            NavigationToken baseNode = new("nio2so", root);

            foreach(Type definedType in protocolAssembly.ExportedTypes)
            {
                if (!definedType.IsAssignableTo(BasicPacketType)) continue;
                if (!DoSearchResult(definedType)) continue;
                string[] tokens = definedType.FullName.Split('.');
                NavigationToken current = baseNode;
                for(int i = 0; i < tokens.Length - 1; i++)
                {
                    if (i == 0) continue;
                    string token = tokens[i];
                    current = current.EnsureToken(token, new());
                }
                var typeNode = current.EnsureToken(definedType, new());
                _selectableTypes.Add(typeNode.NavigationNode, typeNode);
                typeNode.NavigationNode.Selected += Voltron_TypeSelected;
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
            try
            {
                TypeNavigationToken typeNav = _selectableTypes[(TreeViewItem)sender];
                Type type = typeNav.DisplayType;
                var packet = (TSOVoltronPacket)type?.Assembly?.CreateInstance(type.FullName);
                VoltronPacketProperties.DisplayPDU(packet, false);
            }
            catch(Exception exception)
            {               
                MessageBox.Show(exception.Message,  exception.GetType().Name);
            }
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
            Load_VoltronNavigationTree();
            SearchBox.IsEnabled = true;
        }
    }
}
