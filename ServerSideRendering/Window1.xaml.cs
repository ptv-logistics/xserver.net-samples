using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.Tools;
using System.Windows;

namespace ServerSideRendering
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            InitMapLayers();
        }

        private void InitMapLayers()
        {
            const string cluster = "eu-n-test";

            Map.SetMapLocation(new Point(9.182778, 48.775556), 12);

            var xmapMetaInfo = new XMapMetaInfo("https://xmap-" + cluster + ".cloud.ptvgroup.com/xmap/ws/XMap");
            xmapMetaInfo.SetCredentials("xtok", "BB2A4CCB-65D9-4783-BCA6-529AD7A6F4C4");
            InsertXMapBaseLayers(Map.Layers, xmapMetaInfo, "gravelpit");

            Map.InsertTrafficInfoLayer(xmapMetaInfo, "Traffic", "traffic.ptv-traffic", "Traffic information");
            Map.InsertRoadEditorLayer(xmapMetaInfo, "TruckAttributes", "truckattributes", "truckattributes", "Truck attributes");
            Map.InsertPoiLayer(xmapMetaInfo, "Poi", "default.points-of-interest", "Points of interest");
        }
        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta, string profile)
        {
            var baseLayer = new TiledLayer("Background")
            {
                TiledProvider = new ExtendedXMapTiledProvider(meta.Url, meta.User, meta.Password)
                {
                    ContextKey = "in case of context key",
                    CustomProfile = profile + "-bg"
                },
                Copyright = meta.CopyrightText,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                IsBaseMapLayer = true,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProvider(
                    meta.Url, XMapMode.Town)
                {
                    User = meta.User, Password = meta.Password, ContextKey = "in case of context key",
                    CustomProfile = profile + "-fg"
                },               
                Copyright = meta.CopyrightText,
                MaxRequestSize = meta.MaxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png")
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);
        }
    }
}
