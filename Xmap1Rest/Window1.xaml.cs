using System;
using System.Windows;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
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
            Map.Layers.Add(new TiledLayer("Background")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 22,
                    RequestBuilderDelegate = (x, y, z) =>
                            $"https://api{1+(x+y)%4}-test.cloud.ptvgroup.com/WMS/GetTile/xmap-silkysand-bg/{x}/{y}/{z}.png",
                },
                Copyright = $"© { DateTime.Now.Year } PTV AG, TomTom",
                IsBaseMapLayer = true,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
            });

            // -fg layers require the xServer-internet token
            var token = "Insert your xToken here";
            Map.Layers.Add(new UntiledLayer("Labels")
            {
                UntiledProvider = new WmsUntiledProvider(
                    $"https://api-test.cloud.ptvgroup.com/WMS/WMS?xtok={token}&service=WMS&request=GetMap&version=1.1.1&layers=xmap-silkysand-fg&styles=&format=image%2Fpng&transparent=true&srs=EPSG%3A3857", 19),
                Copyright = $"© { DateTime.Now.Year } PTV AG, TomTom",
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png")
            });
        }
    }
}
