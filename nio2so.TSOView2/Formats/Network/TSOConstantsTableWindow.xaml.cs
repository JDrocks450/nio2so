using nio2so.Formats.Util.Endian;
using nio2so.Voltron.PreAlpha.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace nio2so.TSOView2.Formats.Network
{
    /// <summary>
    /// Interaction logic for TSOConstantsTableWindow.xaml
    /// </summary>
    public partial class TSOConstantsTableWindow : Window
    {
        readonly List<Type> _enumTypeSelection = new();
        Type? _enumTypeSelected = null;
        string[]? _enumNames = null;
        readonly List<string> _userSelectableItems = new();
        string? _searchTerm = null;

        public TSOConstantsTableWindow(Window? owner = default)
        {
            InitializeComponent();

            Owner = owner;

            Loaded += WindowLoaded;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTypeSelection();
        }

        private void RefreshTypeSelection()
        {
            _enumTypeSelection.Clear();

            Type defaultSelection = typeof(TSO_PreAlpha_MasterConstantsTable);

            //**find all ENUM types
            foreach (var type in defaultSelection.Assembly.ExportedTypes)
            {
                if (!type.IsEnum) continue;
                if (type == defaultSelection) _enumTypeSelection.Insert(0, type);
                else _enumTypeSelection.Add(type);
            }

            TypeSwitcher.SelectionChanged -= ComboBox_SelectionChanged;
            //**display names of the enum types in this assembly module
            TypeSwitcher.ItemsSource = _enumTypeSelection.Select(x => x.Name);
            if (_enumTypeSelection.Any())
            {
                TypeSwitcher.SelectionChanged += ComboBox_SelectionChanged;
                TypeSwitcher.SelectedIndex = 0; // invoke event on type changed
            }
        }

        private void RefreshListing()
        {
            if (TypeSwitcher.SelectedIndex < 0) return;
            _enumTypeSelected = _enumTypeSelection.ElementAtOrDefault(TypeSwitcher.SelectedIndex);
            if (_enumTypeSelected == default) return;
            if (!_enumTypeSelected.IsEnum) return; // like how would this even happen... why am i checking for this

            EnumBaseTypeLabel.Text = _enumTypeSelected.GetEnumUnderlyingType().Name;

            //valid selection
            _enumNames = Enum.GetNames(_enumTypeSelected);
            HandleSearch();             
        }

        private void HandleSearch()
        {
            void setUserSelectableItems(IEnumerable<string> selections)
            {
                var enumType = _enumTypeSelected.GetEnumUnderlyingType();
                ConstantsListing.ItemsSource = enumType == typeof(uint) ||
                    enumType == typeof(int)  ||
                    enumType == typeof(ushort) ?
                    selections.Select(x => $"{string.Format($"{{0:{(enumType == typeof(ushort) ? "X4" : "X8")}}}", 
                    Convert.ChangeType(Enum.Parse(_enumTypeSelected, x), enumType))}: {x}") : selections;
                    
                    
                _userSelectableItems.Clear();
                _userSelectableItems.AddRange(selections);

                ConstantsAmountLabel.Text = selections.Count().ToString();
            }

            if (_enumNames == null) return;
            if (!string.IsNullOrWhiteSpace(_searchTerm) && _enumTypeSelected != null)
            {
               if (uint.TryParse(_searchTerm, out uint SearchValue) &&
                     Enum.IsDefined(_enumTypeSelected, Convert.ChangeType(SearchValue, _enumTypeSelected.GetEnumUnderlyingType()))) // INT SEARCH
                {
                    setUserSelectableItems(_enumNames.Where(x => x == Enum.GetName(_enumTypeSelected, 
                        Convert.ChangeType(SearchValue, _enumTypeSelected.GetEnumUnderlyingType()))));
                    return;
                }
                if (_searchTerm.StartsWith("0x") && // HEX SEARCH   
                    uint.TryParse(_searchTerm.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint Value) &&
                    Enum.IsDefined(_enumTypeSelected, Convert.ChangeType(Value, _enumTypeSelected.GetEnumUnderlyingType())))
                {
                    //search has to start with 0x,
                    //has to be a valid Hex number
                    //hex value has to be on the type selected
                    setUserSelectableItems(_enumNames.Where(x => x == Enum.GetName(_enumTypeSelected, Value)));
                    return;
                }
            }
            //normal name string search
            setUserSelectableItems(string.IsNullOrWhiteSpace(_searchTerm) ?
                _enumNames : _enumNames.Where(x => x.Contains(_searchTerm)));
        }

        private void ShowValue(object SelectedValue)
        {
            if (SelectedValue == default) return;
            if (_enumTypeSelected == default) return;
            ConstantNameBox.Text = Enum.GetName(_enumTypeSelected, SelectedValue);
            ConstantHexBox.Text = ConstantBytesBox.Text = "";
            ConstantDecBox.Text = SelectedValue.ToString();
            try
            {
                if (_enumTypeSelected.GetEnumUnderlyingType() == typeof(uint) ||
                    _enumTypeSelected.GetEnumUnderlyingType() == typeof(int) ||
                        _enumTypeSelected.GetEnumUnderlyingType() == typeof(ushort))
                {
                    uint value = (uint)Convert.ChangeType(SelectedValue, typeof(uint));
                    ConstantHexBox.Text = "0x" + value.ToString("X8");
                    ConstantDecBox.Text = value.ToString();
                    byte[] bytes = EndianBitConverter.Little.GetBytes(value);
                    ConstantBytesBox.Text = string.Join(" ", bytes.Select(x => x.ToString("X2")));
                    bytes = EndianBitConverter.Big.GetBytes(value);
                    ConstantBytesBigEndianBox.Text = string.Join(" ", bytes.Select(x => x.ToString("X2")));
                }
            }
            catch(InvalidCastException e) { }
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchBox.IsEnabled = false;
            _searchTerm = SearchBox.Text;
            RefreshListing();
            SearchBox.IsEnabled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshListing();
        }

        private void ConstantsListing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConstantsListing.SelectedIndex < 0) return;
            string? selectedValue = _userSelectableItems.ElementAtOrDefault(ConstantsListing.SelectedIndex);
            if (selectedValue == default) return;
            if(Enum.TryParse(_enumTypeSelected, selectedValue, out object EnumValue))
                ShowValue(EnumValue);
        }
    }
}
