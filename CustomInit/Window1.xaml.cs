using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace CustomInit
{
    /// <summary>
    /// This class shows the advanced initialization of the xServer layer. It shows how the initialization can be customized
    /// and gets the xServer meta data in a separate thread, without blocking the ui.
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            const string url = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            const string token = "Insert your xToken here"; 

            // v1: Use Meta info
            InitializeMap(Map1, url, token);

            // v2: direct initialization for xServer-internet
            InsertXMapBaseLayers(Map2.Layers, url, "PTV AG, TomTom", new Size(3840, 2400), "xtok", token);

            // v3: direct initialization for xserver-internet-2
            Map3.Layers.Add(new TiledLayer("Background")
            {
                TiledProvider = new RemoteTiledProvider()
                {
                    MinZoom = 0,
                    MaxZoom = 22,
                    RequestBuilderDelegate = (x, y, z) =>
                    $"https://s0{1 + (x + y) % 4}-xserver2-test.cloud.ptvgroup.com/services/rest/XMap/tile/{z}/{x}/{y}?xtok={token}",
                },
                Copyright = $"© { DateTime.Now.Year } PTV AG, HERE",
                IsBaseMapLayer = true,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
            });
        }

        /// <summary>
        /// Asynchronous initialization of the map
        /// </summary>
        private async void InitializeMap(IMap map, string url, string token)
        {
            // the tools class XMapMetaInfo contains the information required to initialize the xServer layers
            // When instantiated with the url, it tries to read the attribution text and the maximum request size from the xMap configuration
            // To avoid blocking the application init, call it asynchronously
            var meta = await Task.Run(() => new XMapMetaInfo(url));
            meta.SetCredentials("xtok", token); // set the basic authentication properties, e.g. xtok/token for xServer internet

            // Initialize the map
            InsertXMapBaseLayers(map.Layers, meta);
        }

        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta)
        {
            InsertXMapBaseLayers(layers, meta.Url, meta.CopyrightText, meta.MaxRequestSize, meta.User, meta.Password);
        }

        // Initialize the xServer base map layers
        public void InsertXMapBaseLayers(LayerCollection layers, string url, string copyrightText, Size maxRequestSize, string user, string password)
        {
            var baseLayer = new TiledLayer("Background")
            {
                TiledProvider = new XMapTiledProvider(url, XMapMode.Background) { User = user, Password = password, ContextKey = "in case of context key" },
                Copyright = copyrightText,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                IsBaseMapLayer = true,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProvider(url, XMapMode.Town) { User = user, Password = password, ContextKey = "in case of context key" },
                Copyright = copyrightText,
                MaxRequestSize = maxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png")
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);
        }
    }
}
