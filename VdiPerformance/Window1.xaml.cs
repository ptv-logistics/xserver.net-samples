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
using System.Diagnostics;

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

            // can set InfiniteZoom = true
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // for displaying fps (is this really accurate)?
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

        string profile = "ajax"; // specific basemap profile
        bool singleTileBackground = false; // render background layer as single tile (non-tiled)
        bool useSingleLayerBM = false; // render basemap as one single image

        bool useSymLazyUpdate = true;  // recalculate the sizes only after EndViewportChanged
        bool useSymBitmapCaching = true;  // use bitmap caching for WPF symbols

        bool usePanSimpleShape = false; // use a simlple / non-filled shape for the drag/select rectangle
        bool usePanMoveWhileDragging = true; // move the map while panning
        
        // some /random coodinates
        List<Point> coordinates;

        // our tweaked PanAndZoom interactor
        PanAndZoom customPanAndZoom;

        // the cache for bitmaps
        private Dictionary<Tuple<Color, double>, BitmapSource> bitmapCache = new Dictionary<Tuple<Color, double>, BitmapSource>();

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            this.hardwareLevel.Text = "Hardware Level: " + (RenderCapability.Tier >> 16).ToString();

            var center = new System.Windows.Point(8.4, 49); // KA
                
            InitiaizeBasemap();

            // Map.MouseDragMode = DragMode.SelectOnShift;
            // replace the standard pan/zoom interactor with an extened VDI interactor
            customPanAndZoom = new VdiPerformance.PanAndZoom() { MoveWhileDragigng = usePanMoveWhileDragging, UseSimpleSelectShape = usePanSimpleShape };
            // get the map container grid
            var grid = MapElementExtensions.FindChild<Grid>(Map);
            // get the old interactor
            var pz = Map.FindRelative<Ptv.XServer.Controls.Map.Gadgets.PanAndZoom>();
            // exchange the interactor
            grid.Children.Remove(pz);
            grid.Children.Add(customPanAndZoom);

            // create some random coordnates
            CreateCoordinates(center);

            // initialize the symbol layer
            AddSymbols(useSymBitmapCaching, useSymLazyUpdate);

            // set map center
            this.Map.SetMapLocation(center, 9);
        }

        public void CreateCoordinates(Point center)
        {
            var radius = 1; // radius in degrees of latitude
            
            var rand = new Random();
            Func<Point, double, Point> randomCoordinate = (c, r) =>
            {
                // we wnat a circle, so we aopt longitude factor
                var conf = Math.Cos(center.Y / 360 * 2 * Math.PI);

                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new System.Windows.Point
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
            var password = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9"; // this token is only for test purpose
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
                    UntiledProvider = new XMapTiledProvider(url, XMapMode.Town) { User = user, Password = password, CustomProfile = profile + "-fg" },
                    Copyright = copyrightText,
                    MaxRequestSize = maxRequestSize,
                    Caption = MapLocalizer.GetString(MapStringId.Labels),
                    Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
                };
                Map.Layers.Insert(0, labelLayer);
            }

            Map.Layers.Insert(0, baseLayer);
        }

        public void AddSymbols(bool bitmapCache, bool lazyUpdate)
        {
            var layer = Map.Layers["Symbols"];
            if (layer != null)
                Map.Layers.Remove(layer);

            var symbolLayer = new ShapeLayer("Symbols");
            symbolLayer.LazyUpdate = lazyUpdate;
            Map.Layers.Add(symbolLayer);

            for (int i = 0; i < coordinates.Count; i++)
            {
                var symbolSize = 25;
                var color = (i % 3 == 0) ? Colors.Blue : (i % 3 == 1) ? Colors.Green : Colors.Red;

                FrameworkElement symbol = null;
                if (useSymBitmapCaching)
                {
                    var bitmap = GetCachedBitap(color, symbolSize);
                    symbol = new Image { Source = bitmap };
                }
                else
                {
                    symbol = new Pyramid() { Color = color };
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

        private ImageSource GetCachedBitap(Color color, double size)
        {
            var key = Tuple.Create(color, size);
            if (bitmapCache.ContainsKey(key))
                return bitmapCache[key];
            else
            {
                var pin = new Pyramid();
                pin.Width = pin.Height = size;
                pin.Color = color;

                pin.Measure(new Size(pin.Width, pin.Height));
                pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

                double hdFactor = 2; // resultion relative to 96 DPI
                var bitmap = new RenderTargetBitmap((int)(pin.Width * hdFactor), (int)(pin.Height * hdFactor),
                    96 * hdFactor, 96 * hdFactor, PixelFormats.Pbgra32);

                bitmap.Render(pin);
                bitmap.Freeze();

                bitmapCache[key] = bitmap;
                return bitmap;
            }            
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
            profile = useReucedProfile.IsChecked.Value ? "gravelpit" : "ajax";
            InitiaizeBasemap();
        }

        private void useSymbolLazyUpdate_Click(object sender, RoutedEventArgs e)
        {
            useSymLazyUpdate = useSymbolLazyUpdate.IsChecked.Value;
            AddSymbols(useSymBitmapCaching, useSymLazyUpdate);
        }

        private void useBitmapCaching_Click(object sender, RoutedEventArgs e)
        {
            useSymBitmapCaching = useBitmapCaching.IsChecked.Value;
            AddSymbols(useSymBitmapCaching, useSymLazyUpdate);
        }

        private void useFilledShape_Click(object sender, RoutedEventArgs e)
        {
            usePanSimpleShape = !useFilledShape.IsChecked.Value;
            customPanAndZoom.UseSimpleSelectShape = usePanSimpleShape; 
        }

        private void moveWhileDragging_Click(object sender, RoutedEventArgs e)
        {
            usePanMoveWhileDragging = moveWhileDragging.IsChecked.Value;
            customPanAndZoom.MoveWhileDragigng = usePanMoveWhileDragging;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
