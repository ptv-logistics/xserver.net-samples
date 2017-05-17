using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace XSTwo
{
    public partial class Form1 : Form
    {
        private static string myToken = "06DEED74-0CA6-43F1-99F3-298E4B394631";

        public Form1()
        {
            InitializeComponent();
        }

        private void formsMap1_Load(object sender, EventArgs e)
        {
            // set the inifinite zoom option to allow hight zoom levels for xMap-2
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            // initialize the first map
            InitializeMap1();

            // initialize the second map
            InitializeMap2();
        }

        private void InitializeMap1()
        {
            // center and radius for our map
            var center = new Point(8.4044, 49.01405);

            // the maixum zoom level of the map
            formsMap1.MaxZoom = 22;

            // set the map center to Karlsruhe
            formsMap1.SetMapLocation(center, 16);

            // insert base map layer
            formsMap1.Layers.Add(new TiledLayer("Background")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 22,
                    RequestBuilderDelegate = (x, y, z) => string.Format("https://s0{0}-xserver2-europe-test.cloud.ptvgroup.com/services/rest/XMap/tile/{1}/{2}/{3}?storedProfile={4}&xtok={5}",
                        "1234"[(x ^ y) % 4], z, x, y, "silkysand", myToken)
                },
                IsBaseMapLayer = true, // set to the basemap category -> cannot be moved on top of overlays
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                Caption = MapLocalizer.GetString(MapStringId.Background),
                Copyright = "PTV, TOMTOM"
            });

            // add custom layer
            var myLayer = new ShapeLayer("MyLayer");
            formsMap1.Layers.Add(myLayer);
            AddCircle(myLayer, center, 250);
        }
        private void InitializeMap2()
        {
            // center and radius for our map
            var center = new Point(8.4044, 49.01405);

            // the second map instance is initialized with two basemap layers
            formsMap2.MaxZoom = 22;

            // set the map center to Karlsruhe
            formsMap2.SetMapLocation(center, 16);

            // add a base layer with only background and transport
            formsMap2.Layers.Add(new TiledLayer("Background")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 22,
                    RequestBuilderDelegate = (x, y, z) => string.Format("https://s0{0}-xserver2-europe-test.cloud.ptvgroup.com/services/rest/XMap/tile/{1}/{2}/{3}?storedProfile={4}&layers=background,transport&xtok={5}",
                        "1234"[(x ^ y) % 4], z, x, y, "silkysand", myToken)
                },
                IsBaseMapLayer = true, // set to the basemap category -> cannot be moved on top of overlays
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                Caption = MapLocalizer.GetString(MapStringId.Background),
                Copyright = "PTV, TOMTOM"
            });

            // now a custom shape layer in-betwee background and labels
            var myLayer = new ShapeLayer("MyLayer");
            formsMap2.Layers.Add(myLayer);
            AddCircle(myLayer, center, 250);

            // now add the labels overlay layer
            formsMap2.Layers.Add(new TiledLayer("Labels")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 22,
                    RequestBuilderDelegate = (x, y, z) => string.Format("https://s0{0}-xserver2-europe-test.cloud.ptvgroup.com/services/rest/XMap/tile/{1}/{2}/{3}?storedProfile={4}&layers=labels&xtok={5}",
                        "1234"[(x ^ y) % 4], z, x, y, "silkysand", myToken)
                },
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Copyright = "PTV, TOMTOM"
            });
        }

        /// <summary>
        /// Add a circle with a geographic radius to a shape layer
        /// </summary>
        /// <param name="layer"> The layer. </param>
        /// <param name="center"> The center (longitude, latitude). </param>
        /// <param name="radius"> The radius in meters. </param>
        public void AddCircle(ShapeLayer layer, Point center, double radius)
        {
            // calculate the size in mercator units
            double cosB = Math.Cos((center.Y / 360.0) * (2 * Math.PI)); // factor depends on latitude
            double ellipseSize = Math.Abs(1.0 / cosB * radius) * 2; // size mercator units

            // add the ellipse shape
            var ellipse = new Ellipse
            {
                Width = ellipseSize,
                Height = ellipseSize,
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 25
            };

            ShapeCanvas.SetScaleFactor(ellipse, 1); // scale linear
            ShapeCanvas.SetLocation(ellipse, center);
            layer.Shapes.Add(ellipse);
        }
    }
}
