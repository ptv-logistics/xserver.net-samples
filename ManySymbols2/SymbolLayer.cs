using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ManySymbols
{
    /// <summary> The object representing our location-based data. </summary>
    public class Location
    {
        /// <summary> Gets or sets the title of the location. </summary>
        public String Title { get; set; }

        /// <summary> Gets or sets the coordinate (Lon/Lat) of the location. </summary>
        public Point Coordinate { get; set; }
    }

    // The basic rending information for a symbol. This information will be used for caching the symbols as bitmaps
    public class BaseSymbol
    {
        public int Type { get; set; }

        public int Size { get; set; }

        public Color Color { get; set; }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + Size.GetHashCode() + Color.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as BaseSymbol;
            return other != null && other.Type == Type && other.Size == Size && other.Color == Color;
        }
    }

    // the extended rendering information for a symbol, including tool tips
    public class Symbol : BaseSymbol
    {
        public string Tooltip { get; set; }
    }

    public class LocationCanvas : WorldCanvas
    {
        // The list of objects
        public IEnumerable<Location> dataSource { get; set; }

        /// <summary> The minimus Zoom factor for which the symbols are displayed </summary>
        public double MinZoom = 0;

        // The Style for objects
        public Func<Location, Symbol> MapStyle;

        private List<ScaleTransform> adjustTransforms = new List<ScaleTransform>();

        private double currentZoom = -99;

        public LocationCanvas(MapView mapView)
            : base(mapView)
        {
            // the default style
            MapStyle = location => new Symbol{Type = 1, Color = Colors.Red, Size = 16, Tooltip = location.Title};
        }

        // a bitmap cache for the symbols
        private Dictionary<BaseSymbol, RenderTargetBitmap> symbolCache = new Dictionary<BaseSymbol, RenderTargetBitmap>();
        private RenderTargetBitmap createBitmap(BaseSymbol symbol)
        {
            if (symbolCache.ContainsKey(symbol))
                return symbolCache[symbol];

            FrameworkElement element;
            switch (symbol.Type)
            {
                default: // ToDo: support different types
                    element = new Pyramid {Color = symbol.Color};
                    break;
            }
            element.Width = element.Height = symbol.Size;

            element.Measure(new Size(element.Width, element.Height));
            element.Arrange(new Rect(0, 0, element.Width, element.Height));

            var bitmap = new RenderTargetBitmap((int)element.Width * 2, (int)element.Height * 2, 192, 192, PixelFormats.Pbgra32);
            bitmap.Render(element);
            bitmap.Freeze();

            symbolCache.Add(symbol, bitmap);
            return bitmap;
        }

        /// <summary> Adjust the transformation for logarithmic scaling. </summary>
        public void AdjustScales()
        {
            if (MapView.FinalZoom == currentZoom)
                return;

            if (MapView.FinalZoom < MinZoom)
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                foreach (var adjustTransform in this.adjustTransforms)
                {
                    adjustTransform.ScaleX = Math.Pow(MapView.FinalScale, logicalScaleFactor);
                    adjustTransform.ScaleY = Math.Pow(MapView.FinalScale, logicalScaleFactor);
                }

                Visibility = Visibility.Visible;
                Opacity = 1.0;

                currentZoom = MapView.FinalZoom;
            }
        }

        double logicalScaleFactor = 0.85;
        private void CreateObjects()
        {
            currentZoom = MapView.FinalZoom;

            this.Children.Clear();
            this.adjustTransforms.Clear();
            int cnt = 0;
            ScaleTransform adjustTransform = null;

            foreach (var location in dataSource)
            {
                if (cnt % 128 == 0) // tweak: group the scale transforms in 128er packages
                {
                    adjustTransform = new ScaleTransform
                    {
                        ScaleX = Math.Pow(MapView.FinalScale, logicalScaleFactor),
                        ScaleY = Math.Pow(MapView.FinalScale, logicalScaleFactor)
                    };
                    adjustTransforms.Add(adjustTransform);
                }
                cnt++;

                addSymbol(new Point(location.Coordinate.X, location.Coordinate.Y), MapStyle(location), adjustTransform);
            }            
        }

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            switch (updateMode)
            {
                case UpdateMode.Refresh:
                {
                    CreateObjects();

                    break;
                }
                case UpdateMode.EndTransition:
                {
                    AdjustScales();

                    break;
                }
                case UpdateMode.WhileTransition:
                {
                    // fade out while zooming
                    Opacity = 1.0 - Math.Abs(currentZoom - MapView.CurrentZoom) * .125;

                    break;
                }
            }
        }

        private void addSymbol(Point coordinate, Symbol symbol, ScaleTransform adjustTransform)
        {
            // create button and set pin template
            var image = new Image
            {
                // create cached bimap
                Source = createBitmap(symbol),

                // scale around center
                RenderTransformOrigin = new Point(.5, .5),

                // set render transform for scaling
                RenderTransform = adjustTransform,
            };

            image.Width = image.Height = symbol.Size;

            // set tool tip information
            ToolTipService.SetToolTip(image, symbol.Tooltip);

            // tranform to map coordinates
            var mercatorPoint = GeoTransform.WGSToPtvMercator(coordinate);

            // set position and add to canvas (invert y-ordinate)
            SetLeft(image, mercatorPoint.X - image.Width / 2);
            SetTop(image, -(mercatorPoint.Y + image.Height / 2));
            Children.Add(image);
        }
    }
}
