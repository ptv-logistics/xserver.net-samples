using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Threading.Tasks;

namespace CustomInit
{
    /// <summary>
    /// This class shows the advanced initialization of the xServer layer. It shows how the initialization can be customized
    /// and gets the xServer meta data in a separate thread, without blocking the ui.
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            InitializeMap();
        }

        /// <summary>
        /// Asynchronous initialization of the map
        /// </summary>
        private async void InitializeMap()
        {
            // Get meta info in separate task and await
            var meta = await Task.Run(() => GetMetaInfo());

            // Initialize the map
            InsertXMapBaseLayers(Map.Layers, meta);
        }

        /// <summary>
        /// Get the meta info from xServer
        /// </summary>
        /// <returns></returns>
        private XMapMetaInfo GetMetaInfo()
        {            
            // the tools class XMapMetaInfo contains the information required to intialize the xServer layers
            // When instantiated with the url, it tries to read the attribution text and the maximum request size from the xMap configuration
            // var meta = new XMapMetaInfo("http://127.0.0.1:50010/xmap/ws/XMap"); // custom xmap with reverse proxy

            var meta = new XMapMetaInfo("https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap"); // xServer internet
            meta.SetCredentials("xtok", "678890783139995"); // set the basic authentication properties, e.g. xtok/token for xserver internet

            return meta;
        }

        // Initialize the xServer base map layers
        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta)
        {
            var baseLayer = new TiledLayer("Background") 
            {
                TiledProvider = new XMapTiledProvider(meta.Url, XMapMode.Background) { User = meta.User, Password = meta.Password, ContextKey = "in case of context key" },
                Copyright = meta.CopyrightText,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                IsBaseMapLayer = true,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProvider(meta.Url, XMapMode.Town) { User = meta.User, Password = meta.Password, ContextKey = "in case of context key" },
                Copyright = meta.CopyrightText,
                MaxRequestSize = meta.MaxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);
        }
    }
}
