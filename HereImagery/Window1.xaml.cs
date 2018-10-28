using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NokiaDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            // setting the "infinite zoom" option fixes artifacts at deep zoom levels
            GlobalOptions.InfiniteZoom = true;
            
            // "over-zoom" - go beyond the normal zoom level
            Map.MaxZoom = 23;

            Map.Loaded += Map_Loaded;
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            // To demonstrate the decomposed tiles, a rectangular shape is added.
            shapeLayer = new ShapeLayer("CustomShapes") { Caption = "Custom Shapes", Opacity=.8 };
            Map.Layers.Add(shapeLayer);

            var poly = new MapPolygon
            {
                Points = new PointCollection(new[] { new Point(7.9,48.9), new Point(7.9,49.1), new Point(8.1,49.1), new Point(8.1,48.9) }),
                Fill = new SolidColorBrush(Colors.Blue),
                MapStrokeThickness = 3,
                Stroke = new SolidColorBrush(Colors.Black)
            };

            shapeLayer.Shapes.Add(poly);
        }

        private ShapeLayer shapeLayer;

        private void ModeChecked(object sender, RoutedEventArgs e)
        {
            const string appId = "<Your App Id>";
            const string appKey = "<Your App Key>";

            string mode = (sender as RadioButton).Name;

            Map.Layers.RemoveNokiaLayers();
            switch (mode)
            {
                case "M0": // basic road map
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
                case "M1": // map decomposed into basemap and street 
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.NormalDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
                case "M2": // map decomposed into basemap and labels
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.NormalDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.LabelTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
                case "M3": // map decomposed into basemap, street and labels
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.NormalDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.LabelTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
                case "M4":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.SatelliteDay, appId, appKey);
                    break;
                case "M5":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.SatelliteDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
                case "M6":
                    Map.Layers.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.TerrainDay, appId, appKey);
                    break;
                case "M7":
                    Map.Layers.AddNokiaLayer(Nokia.Type.BaseTile, Nokia.Scheme.TerrainDay, appId, appKey);
                    Map.Layers.AddNokiaLayer(Nokia.Type.StreetTile, Nokia.Scheme.NormalDay, appId, appKey);
                    break;
            }

            // move shape layer under the topmost content layer
            if(shapeLayer != null)
                Map.Layers.Move(0, Map.Layers.Count <= 2 ? 1 : Map.Layers.Count - 2);
        }
    }
}
