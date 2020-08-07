using System.Windows;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;

namespace Circles
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
            // http://ows.terrestris.de/dienste.html
            Map.Layers.Add(new TiledLayer("BASEMAP")
            {
                Caption = "Base Map",
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                TiledProvider = new WmsProvider(
                    "http://ows.terrestris.de/osm-gray/service?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&FORMAT=image%2fpng&LAYERS=OSM-WMS&STYLES=,&TILED=true&SRS=EPSG%3a3857", 19),
                Copyright = "© terrestris",
                IsBaseMapLayer = true // overlay layers cannot be moved under a basemap layer
            });

            Map.Layers.Add(new UntiledLayer("EUMET") { 
                Caption = "Air Mass",
                UntiledProvider = new WmsUntiledProvider(
                    "https://eumetview.eumetsat.int/geoserv/wms?service=WMS&request=GetMap&version=1.3.0&layers=meteosat%3Amsg_eview&styles=&format=image%2Fpng&transparent=true&crs=EPSG%3A3857", 19),
                Copyright = "© eumetsat 2015",
                Opacity = .6                
            });
        }
    }
}
