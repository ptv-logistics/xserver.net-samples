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

            // now set the max-zoom to level 30(!)
            Map.MaxZoom = 30;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Map.Layers.Add(new TiledLayer("Mandelbrot") { TiledProvider = new MandelbrotTileProvider() });
        }
    }
}
