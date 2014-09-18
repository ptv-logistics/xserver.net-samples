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

namespace Circles
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
        /// Shows how to render circles with a geographic radius of 250km
        /// Note: you'll have to take the latitude into account!
        /// </summary>
        /// <param name="layer"></param>
        public void AddPinWithLabel(ShapeLayer layer)
        {
            for (int lon = -180; lon <= +180; lon = lon + 10)
            {
                for (int lat = -80; lat <= +80; lat = lat + 10)
                {
                    double radius = 250000; // radius in meters
                    double cosB = Math.Cos((lat / 360.0) * (2 * Math.PI)); // factor depends on latitude
                    double ellipseSize = Math.Abs(1.0 / cosB * radius) * 2; // size mercator units

                    var ellipse = new Ellipse
                    {
                        Width = ellipseSize,
                        Height = ellipseSize,
                        Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 25000
                    };

                    ShapeCanvas.SetScaleFactor(ellipse, 1); // scale linear
                    ShapeCanvas.SetLocation(ellipse, new Point(lon, lat));
                    layer.Shapes.Add(ellipse);
                }
            }
        }
    }
}
