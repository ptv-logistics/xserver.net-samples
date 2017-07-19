using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Ptv.XServer.Controls.Map.Gadgets;

namespace VdiPerformance
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
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // ToDo: is this really accurate?
        List<long> FrameDurations = new List<long>();
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var t = DateTime.Now.Ticks;
            if (FrameDurations.Count == 0)
            {
                FrameDurations.Add(t);
                return;
            }

            while (FrameDurations.Count > 0 && t - FrameDurations[0] > 5 * 10000000)
                FrameDurations.RemoveAt(0);

            this.fps.Text = (FrameDurations.Count / 5).ToString() + " fps";

            FrameDurations.Add(t);
        }

        string profile = "ajax";
        bool singleTileBackground = false;
        bool useSingleLayerBM = false;

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            this.hardwareLevel.Text = "Hardware Level: " + (RenderCapability.Tier >> 16).ToString();

            InitiaizeBasemap();

            this.Map.Gadgets.Add(Ptv.XServer.Controls.Map.Gadgets.GadgetType.Copyright, new DimmerGadget());

            var myLayer = new ShapeLayer("MyLayer");
            myLayer.LazyUpdate = true; // recalculate the sizes only after EndViewportChanged
            Map.Layers.Add(myLayer);
            //            Map.Layers.SetVisible(myLayer, false);

            AddSymbols(myLayer, true);
        }

        public void InitiaizeBasemap()
        {
            var bgLayer = Map.Layers["Background"];
            if (bgLayer != null)
                Map.Layers.Remove(bgLayer);
            var fgLayer = Map.Layers["Labels"];
            if (fgLayer != null)
                Map.Layers.Remove(fgLayer);

            var copyrightText = "PTV, TomTom";
            var maxRequestSize = new Size(3840, 2400);
            var user = "xtok";
            var password = "30BD1C85-51B0-4CE0-98A9-575837BA9708";
            var url = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";

            ILayer baseLayer;
            if (singleTileBackground || useSingleLayerBM)
            {
                baseLayer = new UntiledLayer("Background")
                {
                    UntiledProvider = new XMapTiledProvider(url, XMapMode.Background) { User = user, Password = password,
                        CustomProfile = profile + (useSingleLayerBM? "" : "-bg")},
                    Copyright = copyrightText,
                    Caption = MapLocalizer.GetString(MapStringId.Background),
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                };
            }
            else
            {
                baseLayer = new TiledLayer("Background")
                {
                    TiledProvider = new XMapTiledProvider(url, XMapMode.Background) { User = user, Password = password, CustomProfile = profile + "-bg" },
                    Copyright = copyrightText,
                    Caption = MapLocalizer.GetString(MapStringId.Background),
                    IsBaseMapLayer = true,
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                };
            }

            if (!useSingleLayerBM)
            {
                var labelLayer = new UntiledLayer("Labels")
                {
                    UntiledProvider = new XMapTiledProvider(url, XMapMode.Town) { User = user, Password = password, CustomProfile = "reduced-fg" },
                    Copyright = copyrightText,
                    MaxRequestSize = maxRequestSize,
                    Caption = MapLocalizer.GetString(MapStringId.Labels),
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
                };
                Map.Layers.Insert(0, labelLayer);
            }

            Map.Layers.Insert(0, baseLayer);
        }

        public void AddSymbols(ShapeLayer layer, bool useBitmapCache)
        {
            layer.Shapes.Clear();
            var center = new System.Windows.Point(8.4, 49); // KA
            var radius = 1; // radius in degrees of latitude
            
            var rand = new Random();
            Func<Point, double, Point> randomCoordinate = (c, r) =>
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new System.Windows.Point
                {
                    X = c.X + distance * Math.Cos(angle),
                    Y = c.Y + distance * Math.Sin(angle)
                };
            };

            List<Point> coordinates = new List<Point>();
            for (int i = 0; i < 2500; i++)
            {
                coordinates.Add(randomCoordinate(center, radius));
            }

            // sort points by y, so the overlap nicely on the map
            coordinates = coordinates.OrderBy(p => p.Y).ToList();

            var symbolSize = 40;

            RenderTargetBitmap bitmap = null;
            if (useBitmapCache)
            {
                var pin = new Pyramid();
                pin.Width = pin.Height = symbolSize;

                pin.Measure(new Size(pin.Width, pin.Height));
                pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

                useBitmapCache = true;

                double hdFactor = 2; // resultion relative to 96 DPI
                bitmap = new RenderTargetBitmap((int)(pin.Width * hdFactor), (int)(pin.Height * hdFactor),
                    96 * hdFactor, 96 * hdFactor, PixelFormats.Pbgra32);
                bitmap.Render(pin);
                bitmap.Freeze();
            }

            for (int i = 0; i < coordinates.Count; i++)
            {
                FrameworkElement symbol = null;
                if (bitmap != null)
                {
                    symbol = new Image { Source = bitmap };
                }
                else
                {
                    symbol = new Pyramid();
                    symbol.Width = symbol.Height = symbolSize;
                }

                // log-scaling parameters
                ShapeCanvas.SetScale(symbol, 10);
                ShapeCanvas.SetScaleFactor(symbol, 0.5);

                ShapeCanvas.SetLocation(symbol, randomCoordinate(center, radius));
                Panel.SetZIndex(symbol, i * 10);
                symbol.ToolTip = "Hello";
                layer.Shapes.Add(symbol);
            }

            this.Map.SetMapLocation(center, 9);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            RenderOptions.ProcessRenderMode = (this.renderMode.IsChecked.Value)? RenderMode.SoftwareOnly
                : RenderMode.Default;
        }

        private void useAnimation_Click(object sender, RoutedEventArgs e)
        {
            Map.UseAnimation = this.useAnimation.IsChecked.Value;
        }

        private void bilinearScaling_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(this.Map, bilinearScaling.IsChecked.Value? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified );
        }

        private void useSingleTileBG_Checked(object sender, RoutedEventArgs e)
        {
            singleTileBackground = useSingleTileBG.IsChecked.Value;
            InitiaizeBasemap();
        }

        private void useSingleLayerBaseMap_Checked(object sender, RoutedEventArgs e)
        {
            useSingleLayerBM = useSingleLayerBaseMap.IsChecked.Value;
            InitiaizeBasemap();
        }

        private void useReucedProfile_Checked(object sender, RoutedEventArgs e)
        {
            profile = useReucedProfile.IsChecked.Value ? "reduced" : "ajax";
            InitiaizeBasemap();
        }
    }

    public class CoordinatesBasedComparer : IComparer<Point>
    {
        public int Compare(Point a, Point b)
        {
            if ((a.X == b.X) && (a.Y == b.Y))
                return 0;
            if ((a.Y > b.Y) || ((a.Y == b.Y) && (a.X > b.X)))
                return -1;

            return 1;
        }
    }
}
