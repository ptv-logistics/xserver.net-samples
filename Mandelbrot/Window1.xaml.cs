using Ptv.XServer.Controls.Map.Layers.Tiled;
using System.Windows;

namespace Mandelbrot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            // setting the infinite zoom option fixes artifacts at deep zoom levels
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            // now set the max-zoom to level 30(!)
            Map.MaxZoom = 30;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Map.Layers.Add(new TiledLayer("Mandelbrot") { TiledProvider = new MandelbrotTileProvider() });
        }
    }
}
