using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.UIs.Controls
{
    /// <summary>
    /// Interaction logic for TSOButton.xaml
    /// </summary>
    public partial class TSOButton : UserControl
    {
        public TSOButton()
        {
            InitializeComponent();
            Content = null;
        }

        public void Reset()
        {
            if (!(Background is ImageBrush brush)) return;           
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(0, 0, brush.ImageSource.Width, brush.ImageSource.Height);
            brush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
            brush.Viewbox = new Rect(0, 0, 1, 1);
        }

        private void this_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(Background is ImageBrush brush)) return;
            if (!IsEnabled) return;
            brush.Viewbox = new Rect(.5, 0, 1, 1);
        }

        private void this_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            Reset();
        }

        private void this_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(Background is ImageBrush brush)) return;
            if (!IsEnabled) return;
            brush.Viewbox = new Rect(.25, 0, 1, 1);
        }

        private void this_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this_MouseEnter(null,null);
        }

        private void this_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) // enabled
            {
                if (IsMouseOver) this_MouseEnter(sender, null);
                else Reset();
                return;
            }
            if (!(Background is ImageBrush brush)) return;
            brush.Viewbox = new Rect(.75, 0, 1, 1);
        }
    }
}
