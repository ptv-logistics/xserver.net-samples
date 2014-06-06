using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Printing;
using System.IO;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map.Symbols;

namespace SymbolsAndLabels
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            AddPinWithLabel(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Chales Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapSape
        /// </summary>
        /// <param name="layer"></param>
        public void AddPinWithLabel(ShapeLayer layer)
        {
            var pin = new Pin();
            pin.Width = pin.Height = 30;
            ShapeCanvas.SetLocation(pin, new Point(8.4, 49));
            ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
            layer.Shapes.Add(pin);

            TextBlock tb = new TextBlock { Text = "Hello" };
            tb.Background = new SolidColorBrush(Colors.White);
            tb.Foreground = new SolidColorBrush(Colors.Black);
            ShapeCanvas.SetLocation(tb, new Point(8.4, 49));
            ShapeCanvas.SetAnchor(tb, LocationAnchor.LeftTop);
            layer.Shapes.Add(tb);
        }
    }
}
