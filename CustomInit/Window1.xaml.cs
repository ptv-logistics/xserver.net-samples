using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace CustomInit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            // the tools class XMapMetaInfo contains the information required to intialize the xServer layers
            // When instantiated with the url, it tries to read the attribution text and the maximum request size from the xMap configuration
            var meta = new XMapMetaInfo("http://80.146.239.180/xm/xmap/ws/XMap"); // custom xmap with reverse proxy

            // var meta = new XMapMetaInfo("https://xmap-eu-n.cloud.ptvgroup.com/xmap/ws/XMap"); // xServer internet
            // meta.SetCredentials("xtok", "<your token>"); // set the basic authentication properties, e.g. xtok/token for xserver internet

            // Insert the layers
            InsertXMapBaseLayers(Map.Layers, meta);
        }

        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta)
        {
            var baseLayer = new TiledLayer("Background") 
            {          
                TiledProvider = new XMapTiledProvider(meta.Url, XMapMode.Background) { User = meta.User, Password = meta.Password, ContextKey = "in case of context key"},
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
