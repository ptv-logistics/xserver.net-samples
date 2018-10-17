// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ptv.XServer.Demo.UseCases.Selection
{
    public class SelectionUseCase : UseCase
    {
        /// <summary>
        /// Adds a ShapeLayer for the markers.
        /// </summary>
        protected override void Enable()
        {
            var myLayer = new ShapeLayer("ManySymbols") {LazyUpdate = true};
            wpfMap.Layers.Add(myLayer);

            var center = new Point(8.4, 49); // KA
            const int radius = 1; // radius in degrees of latitude
            
            var rand = new Random();
            Func<Point, double, Point> randomCoordinate = (c, r) =>
            {
                var angle = rand.NextDouble() * 2 * Math.PI;
                var distance = r * Math.Sqrt(rand.NextDouble());

                return new Point { X = c.X + distance * Math.Cos(angle), Y = c.Y + distance * Math.Sin(angle) };
            };

            const bool useBitmapCache = true;

            var pin = new Pyramid();
            pin.Width = pin.Height = 10;

            pin.Measure(new Size(pin.Width, pin.Height));
            pin.Arrange(new Rect(0, 0, pin.Width, pin.Height));

            var bitmap = new RenderTargetBitmap((int)pin.Width, (int)pin.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(pin);
            bitmap.Freeze(); 
            for (var i = 0; i < 500; i++)
            {
                FrameworkElement symbol;
                if(useBitmapCache)
                    symbol = new Image { Source = bitmap };
                else
                    symbol = new Pyramid();
                symbol.Width = pin.Height = 10;
                ShapeCanvas.SetLocation(symbol, randomCoordinate(center, radius));
                symbol.ToolTip = "Hello";
                myLayer.Shapes.Add(symbol);
            }

            wpfMap.SetMapLocation(center, 9);

            // get the map view containing the content
            var mapView = Controls.Map.Tools.MapElementExtensions.FindChild<Controls.Map.MapView>(wpfMap);

            selectInteractor = new SelectInteractor(mapView, myLayer.Shapes);
        }

        SelectInteractor selectInteractor;

        /// <summary>
        /// Deletes the layer for the markers from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["ManySymbols"]);

            selectInteractor.Remove();
        }
    }
}
