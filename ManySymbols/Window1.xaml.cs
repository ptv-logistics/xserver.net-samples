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

namespace ManySymbols
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
            myLayer.LazyUpdate = true;
            Map.Layers.Add(myLayer);

            AddPinWithLabel(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Chales Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapSape
        /// </summary>
        /// <param name="layer"></param>
        public void AddPinWithLabel(ShapeLayer layer)
        {

            var center = new System.Windows.Point(8.4, 49); // KA
            var radius = 1; // radius in degrees of latitude
            
            var rand = new Random();
            Func<System.Windows.Point, double, System.Windows.Point> randomCoordinate = (c, r) =>
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new System.Windows.Point
                {
                    X = c.X + distance * Math.Cos(angle),
                    Y = c.Y + distance * Math.Sin(angle)
                };
            };

            bool useBitmapCache = true;

            var pin = new Pyramid();
            pin.Width = pin.Height = 10;

            pin.Measure(new Size(pin.Width, pin.Height));
            pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

            var bitmap = new RenderTargetBitmap((int)pin.Width, (int)pin.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(pin);
            bitmap.Freeze(); 
            for (int i = 0; i < 5000; i++)
            {
                FrameworkElement symbol = null;
                if(useBitmapCache)
                    symbol = new Image { Source = bitmap };
                else
                    symbol = new Pyramid();
                symbol.Width = pin.Height = 10;
                ShapeCanvas.SetLocation(symbol, randomCoordinate(center, radius));
                symbol.ToolTip = "Hello";
                layer.Shapes.Add(symbol);
            }

            this.Map.SetMapLocation(center, 9);
        }
    }
}
