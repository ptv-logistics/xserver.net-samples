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
using System.Diagnostics;

namespace VdiPerformance
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

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // for displaying fps (is this really accurate)?
        private readonly List<long> FrameDurations = new List<long>();

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var t = DateTime.Now.Ticks;
            if (FrameDurations.Count == 0)
            {
                FrameDurations.Add(t);
                return;
            }

            while (FrameDurations.Count > 0 && t - FrameDurations[0] > 5 * 10000000)
                FrameDurations.RemoveAt(0);

            fps.Text = FrameDurations.Count / 5 + " fps";

            FrameDurations.Add(t);
        }

        private string profile = "ajax"; // specific basemap profile
        private bool singleTileBackground = false; // render background layer as single tile (non-tiled)
        private bool useSingleLayerBM = false; // render basemap as one single image

        private bool useSymLazyUpdate = true;  // recalculate the sizes only after EndViewportChanged
        private bool useSymBitmapCaching = true;  // use bitmap caching for WPF symbols

        private bool usePanSimpleShape = false; // use a simple / non-filled shape for the drag/select rectangle
        private bool usePanMoveWhileDragging = true; // move the map while panning
        
        // some /random coordinates
        private List<Point> coordinates;

        // our tweaked PanAndZoom interactor
        private PanAndZoom customPanAndZoom;

        // the cache for bitmaps
        private readonly Dictionary<Tuple<Color, double>, BitmapSource> bitmapCache = new Dictionary<Tuple<Color, double>, BitmapSource>();

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            hardwareLevel.Text = "Hardware Level: " + (RenderCapability.Tier >> 16);

            var center = new Point(8.4, 49); // KA
                
            InitializeBasemap();

            // Map.MouseDragMode = DragMode.SelectOnShift;
            // replace the standard pan/zoom interactor with an extended VDI interactor
            customPanAndZoom = new PanAndZoom { MoveWhileDragging = usePanMoveWhileDragging, UseSimpleSelectShape = usePanSimpleShape };
            // get the map container grid
            var grid = MapElementExtensions.FindChild<Grid>(Map);
            // get the old interactor
            var pz = Map.FindRelative<Ptv.XServer.Controls.Map.Gadgets.PanAndZoom>();
            // exchange the interactor
            grid.Children.Remove(pz);
            grid.Children.Add(customPanAndZoom);

            // create some random coordinates
            CreateCoordinates(center);

            // initialize the symbol layer
            AddSymbols();

            // set map center
            Map.SetMapLocation(center, 9);
        }

        public void CreateCoordinates(Point center)
        {
            const int radius = 1; // radius in degrees of latitude
            
            var rand = new Random();
            Func<Point, double, Point> randomCoordinate = (c, r) =>
            {
                // we want a circle, so we adopt longitude factor
                var conf = Math.Cos(center.Y / 360 * 2 * Math.PI);

                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new Point
                {
                    X = c.X + distance / conf * Math.Cos(angle),
                    Y = c.Y + distance * Math.Sin(angle)
                };
            };

            coordinates = new List<Point>();
            for (int i = 0; i < 1000; i++)
            {
                coordinates.Add(randomCoordinate(center, radius));
            }

            // sort points by y, so the overlap nicely on the map
            coordinates = coordinates.OrderBy(p => -p.Y).ToList(); // has no effect?
        }

        public void InitializeBasemap()
        {
            var bgLayer = Map.Layers["Background"];
            if (bgLayer != null)
                Map.Layers.Remove(bgLayer);
            var fgLayer = Map.Layers["Labels"];
            if (fgLayer != null)
                Map.Layers.Remove(fgLayer);

            const string copyrightText = "PTV, TomTom";
            var maxRequestSize = new Size(3840, 2400);
            const string user = "xtok";
            const string password = "BB2A4CCB-65D9-4783-BCA6-529AD7A6F4C4"; // this token is only for test purpose
            const string url = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";

            ILayer baseLayer;
            if (singleTileBackground || useSingleLayerBM)
            {
                baseLayer = new UntiledLayer("Background")
                {
                    UntiledProvider = new XMapTiledProvider(url, XMapMode.Background) { User = user, Password = password,
                        CustomProfile = profile + (useSingleLayerBM? "" : "-bg")},
                    Copyright = copyrightText,
                    Caption = MapLocalizer.GetString(MapStringId.Background),
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
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
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
                };
            }

            if (!useSingleLayerBM)
            {
                var labelLayer = new UntiledLayer("Labels")
                {
                    UntiledProvider = new XMapTiledProvider(url, XMapMode.Town) { User = user, Password = password, CustomProfile = profile + "-fg" },
                    Copyright = copyrightText,
                    MaxRequestSize = maxRequestSize,
                    Caption = MapLocalizer.GetString(MapStringId.Labels),
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png")
                };
                Map.Layers.Insert(0, labelLayer);
            }

            Map.Layers.Insert(0, baseLayer);
        }

        public void AddSymbols()
        {
            var layer = Map.Layers["Symbols"];
            if (layer != null)
                Map.Layers.Remove(layer);

            var symbolLayer = new ShapeLayer("Symbols") {LazyUpdate = useSymLazyUpdate };
            Map.Layers.Add(symbolLayer);

            for (int i = 0; i < coordinates.Count; i++)
            {
                const int symbolSize = 25;
                var color = i % 3 == 0 ? Colors.Blue : i % 3 == 1 ? Colors.Green : Colors.Red;

                FrameworkElement symbol;
                if (useSymBitmapCaching)
                {
                    var bitmap = GetCachedBitmap(color, symbolSize);
                    symbol = new Image { Source = bitmap };
                }
                else
                {
                    symbol = new Pyramid { Color = color };
                    symbol.Width = symbol.Height = symbolSize;
                }

                // log-scaling parameters
                ShapeCanvas.SetScale(symbol, 10);
                ShapeCanvas.SetScaleFactor(symbol, 0.4);

                ShapeCanvas.SetLocation(symbol, coordinates[i]);
                symbol.ToolTip = "Hello";
                symbolLayer.Shapes.Add(symbol);
            }
        }

        private ImageSource GetCachedBitmap(Color color, double size)
        {
            var key = Tuple.Create(color, size);
            if (bitmapCache.ContainsKey(key))
                return bitmapCache[key];

            var pin = new Pyramid();
            pin.Width = pin.Height = size;
            pin.Color = color;

            pin.Measure(new Size(pin.Width, pin.Height));
            pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

            const double hdFactor = 2; // resolution relative to 96 DPI
            var bitmap = new RenderTargetBitmap((int)(pin.Width * hdFactor), (int)(pin.Height * hdFactor),
                96 * hdFactor, 96 * hdFactor, PixelFormats.Pbgra32);

            bitmap.Render(pin);
            bitmap.Freeze();

            bitmapCache[key] = bitmap;
            return bitmap;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.ProcessRenderMode = renderMode.IsChecked.Value ? RenderMode.SoftwareOnly : RenderMode.Default;
        }

        private void useAnimation_Click(object sender, RoutedEventArgs e)
        {
            Map.UseAnimation = useAnimation.IsChecked.Value;
        }

        private void biLinearScaling_Click(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(Map, biLinearScaling.IsChecked.Value ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified);
        }

        private void useSingleTileBG_Checked(object sender, RoutedEventArgs e)
        {
            singleTileBackground = useSingleTileBG.IsChecked.Value;
            InitializeBasemap();
        }

        private void useSingleLayerBaseMap_Checked(object sender, RoutedEventArgs e)
        {
            useSingleLayerBM = useSingleLayerBaseMap.IsChecked.Value;
            InitializeBasemap();
        }

        private void useFlatProfile_Checked(object sender, RoutedEventArgs e)
        {
            profile = useFlatProfile.IsChecked.Value ? "gravelpit" : "ajax";
            InitializeBasemap();
        }

        private void useSymbolLazyUpdate_Click(object sender, RoutedEventArgs e)
        {
            useSymLazyUpdate = useSymbolLazyUpdate.IsChecked.Value;
            AddSymbols();
        }

        private void useBitmapCaching_Click(object sender, RoutedEventArgs e)
        {
            useSymBitmapCaching = useBitmapCaching.IsChecked.Value;
            AddSymbols();
        }

        private void useFilledShape_Click(object sender, RoutedEventArgs e)
        {
            usePanSimpleShape = !useFilledShape.IsChecked.Value;
            customPanAndZoom.UseSimpleSelectShape = usePanSimpleShape; 
        }

        private void moveWhileDragging_Click(object sender, RoutedEventArgs e)
        {
            usePanMoveWhileDragging = moveWhileDragging.IsChecked.Value;
            customPanAndZoom.MoveWhileDragging = usePanMoveWhileDragging;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
