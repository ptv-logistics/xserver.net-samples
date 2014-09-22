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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Map.Layers.Add(new TiledLayer("Mandelbrot") { TiledProvider = new MandelbrotTileProvider() });
        }
    }
}
