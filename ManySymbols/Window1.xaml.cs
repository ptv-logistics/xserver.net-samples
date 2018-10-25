using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;

namespace ManySymbols
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
            var myLayer = new ShapeLayer("MyLayer") { LazyUpdate = true };
            Map.Layers.Add(myLayer);
            AddPinWithLabel(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Charles Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapShape
        /// </summary>
        /// <param name="layer"></param>
        public void AddPinWithLabel(ShapeLayer layer)
        {
            const int radius = 1; // radius in degrees of latitude
            var center = new Point(8.4, 49); // Karlsruhe

            var rand = new Random();
            Func<Point, double, Point> randomCoordinate = (c, r) =>
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new Point
                {
                    X = c.X + distance * Math.Cos(angle),
                    Y = c.Y + distance * Math.Sin(angle)
                };
            };

            var pin = new Pyramid();
            pin.Width = pin.Height = 10;

            pin.Measure(new Size(pin.Width, pin.Height));
            pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

            var bitmap = new RenderTargetBitmap((int)pin.Width, (int)pin.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(pin);
            bitmap.Freeze(); 
            for (int i = 0; i < 5000; i++)
            {
                FrameworkElement symbol = new Image { Source = bitmap };
                symbol.Width = pin.Height = 10;
                ShapeCanvas.SetLocation(symbol, randomCoordinate(center, radius));
                symbol.ToolTip = "Hello";
                layer.Shapes.Add(symbol);
            }

            Map.SetMapLocation(center, 9);
        }
    }
}
