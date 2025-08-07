using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace nio2so.TSOView2.Controls
{
    /// <summary>
    /// Interaction logic for TSOCityControl.xaml
    /// </summary>
    public partial class TSOCityControl : UserControl
    {
        private record TSOCityControlImageStruct(int Rank, string ImagePath, BitmapSource ImageData);

        /// <summary>
        /// Maps FileName (without extension), <see cref="TSOCityControlImageStruct"/>
        /// </summary>
        private Dictionary<string, TSOCityControlImageStruct> _previewImages = new();
        static string[] priorityNames = {
            "thumbnail.bmp"
        };
        private int currentSlideshowIndex = 0;

        public string CurrentCityFolderAddress
        {
            get; private set;
        }

        public TSOCityControl()
        {
            InitializeComponent();            
        }

        public TSOCityControl(string CityFolderURI) : this()
        {
            DisplayCity(CityFolderURI);
        }

        public bool DisplayCity(string cityFolderAddress)
        {
            if (string.IsNullOrWhiteSpace(cityFolderAddress)) return false;
            if (!Directory.Exists(cityFolderAddress)) return false;

            currentSlideshowIndex = 0;
            CurrentCityFolderAddress = cityFolderAddress;

            //start loading images
            PopulateImages(cityFolderAddress);
            InvalidateImageSlideshow();

            return true;
        }

        private void InvalidateImageSlideshow()
        {
            (string imageName, BitmapSource imageData) getImage()
            {
                ref int currentIndex = ref currentSlideshowIndex;
                if (currentIndex < 0)
                    currentIndex = _previewImages.Count + currentIndex;
                if (currentIndex < 0) currentIndex = 0;
                if (currentIndex >= _previewImages.Count)
                    currentIndex %= _previewImages.Count;
                var imageTuple = _previewImages.Values.OrderBy(x => x.Rank).ElementAt(currentIndex);
                return (System.IO.Path.GetFileName(imageTuple.ImagePath), imageTuple.ImageData);
            }

            if (!_previewImages.Any()) return;

            //populate main thumbnail

            (string imageName, BitmapSource imageData) result = getImage();
            ImageThumb.Source = result.imageData;
            ImageThumbDescBox.Text = result.imageName;

            //populate additional previews
            int origin = currentSlideshowIndex;
            currentSlideshowIndex++;
            Slideshow1.Source = getImage().imageData;
            currentSlideshowIndex++;
            Slideshow2.Source = getImage().imageData;
            currentSlideshowIndex++;
            Slideshow3.Source = getImage().imageData;
            currentSlideshowIndex = origin;
        }

        private void ImageThumbPrevButton_Click(object sender, RoutedEventArgs e)
        {
            currentSlideshowIndex++;
            InvalidateImageSlideshow();
        }

        private void ImageThumbNextButton_Click(object sender, RoutedEventArgs e)
        {
            currentSlideshowIndex--;
            InvalidateImageSlideshow();
        }

        private void PopulateImages(string cityFolderAddress)
        {
            _previewImages.Clear();

            int currentRank = 0;
            foreach (var image in Directory.EnumerateFiles(cityFolderAddress, "*.bmp"))
            {
                currentRank++;
                using (var bmp = System.Drawing.Image.FromFile(image))
                {
                    var managedBmp = bmp.Convert();
                    string myName = System.IO.Path.GetFileName(image);
                    int myRank = currentRank;
                    if (priorityNames.Contains(myName))
                        myRank = 0; // prioritize this image to be displayed first. "recommended" basically
                    _previewImages.Add(System.IO.Path.GetFileNameWithoutExtension(image), new(myRank, image, managedBmp));
                }
            }
        }
    }
}
