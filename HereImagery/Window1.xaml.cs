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
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.TileProviders;
using System.Text.RegularExpressions;

namespace NokiaDemo
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
            // To demonstrate the decomposed tiles, a rectangluar shape is addded.
            shapeLayer = new ShapeLayer("CustomShapes") { Caption = "Custom Shapes", Opacity=.8 };
            Map.Layers.Add(shapeLayer);

            var poly = new MapPolygon
            {
                Points = new PointCollection(new Point[]{new Point(7.9,48.9), new Point(7.9,49.1), new Point(8.1,49.1), new Point(8.1,48.9)}),
                Fill = new SolidColorBrush(Colors.Blue),
                MapStrokeThickness = 3,
                Stroke = new SolidColorBrush(Colors.Black),
            };

            shapeLayer.Shapes.Add(poly);
        }
        ShapeLayer shapeLayer;

        private void ModeChecked(object sender, RoutedEventArgs e)
        {
            string mode = (sender as RadioButton).Name;
            string appId = "W5fxWKhTxk7V1Ye7A4wC";
            string token = "BPev1lh_RBW4cDLqN-Hzqg";

            Map.Layers.RemoveNokiaLayers();
            switch (mode)
            {
                case "M0": // basic road map
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
                case "M1": // map decomposed into basemap and street 
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.NormalDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
                case "M2": // map decomposed into basemap and labels
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.NormalDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.LabelTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
                case "M3": // map decomposed into basemap, street and labels
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.NormalDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.LabelTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
                case "M4":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.SatelliteDay, appId, token);
                    break;
                case "M5":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.SatelliteDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
                case "M6":
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.TerrainDay, appId, token);
                    break;
                case "M7":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.TerrainDay, appId, token);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, token);
                    break;
            }

            // move shape layer under the topmost content layer
            if(shapeLayer != null)
                Map.Layers.Move(0, Map.Layers.Count <= 2 ? 1 : Map.Layers.Count - 2);
        }
    }
}
