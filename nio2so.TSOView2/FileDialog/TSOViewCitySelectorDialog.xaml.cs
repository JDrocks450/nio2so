using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace nio2so.TSOView2.FileDialog
{
    /// <summary>
    /// Interaction logic for TSOViewCitySelectorDialog.xaml
    /// </summary>
    public partial class TSOViewCitySelectorDialog : Window, INotifyPropertyChanged
    {
        private string _citiesDir;

        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// The enclosing "Cities" folder for release versions.
        /// <para/>If in Pre-Alpha, you can use the GameData folder for this.
        /// The dialog will filter out any folders without bitmaps in it.
        /// </summary>
        public string? CitiesDirectory
        {
            get => _citiesDir;
            set
            {
                _citiesDir = value;
                PropertyChanged?.Invoke(this, new(nameof(CitiesDirectory)));
            }
        }
        /// <summary>
        /// The selected folder that contains the city the User wishes to open
        /// </summary>
        public string? FileName
        {
            get; private set;
        }

        private readonly List<string> _citiesInDirectory = new();

        public TSOViewCitySelectorDialog()
        {
            InitializeComponent();

            Loaded += TSOViewCitySelectorDialog_Loaded;
        }

        private void UI_CitiesEnclosingDirectoryChanged()
        {
            if (string.IsNullOrWhiteSpace(CitiesDirectory)) return; // Cities path is null!
            if (!Directory.Exists(CitiesDirectory)) return; // Directory not found!!

            string searchToken = "elevation*";
            _citiesInDirectory.Clear();

            foreach(var dir in Directory.EnumerateDirectories(CitiesDirectory))
            { // ** search all directories in the "cities enclosing directory" for ones that have elevation.bmp
                DirectoryInfo subDirectory = new DirectoryInfo(dir);
                if (!subDirectory.GetFiles(searchToken).Any()) continue; // this is not a city directory
                //this is a city directory ... it has searchToken in it
                _citiesInDirectory.Add(dir);                
            }
            //refresh UI combobox
            CityDirectoriesComboBox.SelectionChanged -= CityDirectoriesComboBox_SelectionChanged;
            CityDirectoriesComboBox.SelectedIndex = 0;            
            CityDirectoriesComboBox.ItemsSource = _citiesInDirectory.Select(x => System.IO.Path.GetFileNameWithoutExtension(x));
            CityDirectoriesComboBox.SelectionChanged += CityDirectoriesComboBox_SelectionChanged;
            CityDirectoriesComboBox.SelectedIndex = _citiesInDirectory.Any() ? 0 : -1;
        }

        //ON LOAD
        private void TSOViewCitySelectorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CitiesDirectory = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory;
            UI_CitiesEnclosingDirectoryChanged();
            CityDirectoriesComboBox_SelectionChanged(null, null);
        }  
        
        private void CityDirectoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cityFolderAddress = _citiesInDirectory[CityDirectoriesComboBox.SelectedIndex];
            CityControl.DisplayCity(cityFolderAddress);
            FileName = cityFolderAddress;
        }      

        private void CitiesDirectoryLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFolderDialog folderDialog = new()
            {
                DefaultDirectory = CitiesDirectory ?? @"C:\",
                Title = "Select an enclosing \"Cities\" folder",
                Multiselect = false,
                ValidateNames = true,
                DereferenceLinks = true,
            };

            if (!folderDialog.ShowDialog() ?? true) return;

            CitiesDirectory = folderDialog.FolderName;
            UI_CitiesEnclosingDirectoryChanged();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;            
        }

        private void CanclButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            FileName = null;
        }
    }
}
