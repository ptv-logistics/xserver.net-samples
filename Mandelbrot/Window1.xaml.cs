using Ptv.XServer.Controls.Map.Layers.Tiled;
using System.Windows;

namespace Mandelbrot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            // setting the infinite zoom option fixes artifacts at deep zoom levels
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            // now set the max-zoom to level 30(!)
            this.Map.MaxZoom = 30;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Map.Layers.Add(new TiledLayer("Mandelbrot") { TiledProvider = new MandelbrotTileProvider() });
        }
    }
}
