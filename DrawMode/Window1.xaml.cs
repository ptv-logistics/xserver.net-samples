using System.Windows;
using Ptv.XServer.Controls.Map;

namespace DrawMode
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
            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);

            // initialize draw control with label
            new DrawControl(mapView);
        }
    }
}
