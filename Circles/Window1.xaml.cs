using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Windows.Controls.Primitives;
using Ptv.XServer.Controls.Map.Tools;

namespace Circles
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            Map.Loaded += Map_Loaded;
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            AddCircles(myLayer);
        }

        /// <summary>
        /// Shows how to render circles with a geographic radius of 250km
        /// Note: you'll have to take the latitude into account!
        /// </summary>
        /// <param name="layer"></param>
        public void AddCircles(ShapeLayer layer)
        {
            for (int lon = -180; lon <= +180; lon = lon + 10)
            {
                for (int lat = -80; lat <= +80; lat = lat + 10)
                {
                    double radius = 250000; // radius in meters
                    double cosB = Math.Cos(lat / 360.0 * (2 * Math.PI)); // factor depends on latitude
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

                    // bonus: show how to add a popup that the location (a the ellipse center)
                    ellipse.Tag = new Point(lon, lat);
                    ellipse.MouseEnter += Ellipse_MouseEnter;
                    ellipse.MouseLeave += Ellipse_MouseLeave;
                }
            }
        }

        private readonly Popup popup = new Popup();
        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            var ellipse = (Ellipse)sender;

            // the location to display the popup
            var location = (Point)ellipse.Tag;

            // create a new wpf text blox
            var border = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(2,2,1,1) };
            var textBlock = new TextBlock
            {
                Padding = new Thickness(2),
                Background = Brushes.Yellow,
                Foreground = Brushes.Black,
                Text =  GeoTransform.LatLonToString(location.Y, location.X, true)
            };
            border.Child = textBlock;
            popup.Child = border;

            // get the MapView object from the control
            var mapView = MapElementExtensions.FindChild<MapView>(Map);
            // transform
            var popupLocation = mapView.WgsToCanvas(Map, location);

            popup.Placement = PlacementMode.RelativePoint;
            popup.PlacementTarget = Map;
            popup.HorizontalOffset = popupLocation.X;
            popup.VerticalOffset = popupLocation.Y;

            popup.IsOpen = true;
            popup.StaysOpen = false;
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            popup.IsOpen = false;
        }
    }
}
