using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System.Threading.Tasks;
using System.Windows;

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

            string url = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            string token = "678890783139995"; // just a test-token here. Use your own token

            // v1: Use Meta info
            InitializeMap(Map1, url, token);

            // v2: direct initialization
            InsertXMapBaseLayers(Map2.Layers, url, "PTV, TomTom", new Size(3840, 2400), "xtok", token);
        }

        /// <summary>
        /// Asynchronous initialization of the map
        /// </summary>
        private async void InitializeMap(WpfMap map, string url, string token)
        {
            // the tools class XMapMetaInfo contains the information required to intialize the xServer layers
            // When instantiated with the url, it tries to read the attribution text and the maximum request size from the xMap configuration
            // To avoid blocking the application init, call it asynchronously
            var meta = await Task.Run(() => new XMapMetaInfo(url));
            meta.SetCredentials("xtok", token); // set the basic authentication properties, e.g. xtok/token for xserver internet

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
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProvider(url, XMapMode.Town) { User = user, Password = password, ContextKey = "in case of context key" },
                Copyright = copyrightText,
                MaxRequestSize = maxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);
        }
    }
}
