using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.UseCases.FeatureLayer;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FeatureLayers
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            referenceTime.Value = DateTime.Now;        

            InitializeMap();

            Map.SetMapLocation(new Point(8.4, 49), 14);
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
        private static XMapMetaInfo GetMetaInfo()
        {
            // the tools class XMapMetaInfo contains the information required to initialize the xServer layers
            // When instantiated with the url, it tries to read the attribution text and the maximum request size from the xMap configuration
            // var meta = new XMapMetaInfo("http://127.0.0.1:50010/xmap/ws/XMap"); // custom xmap with reverse proxy

            var meta = new XMapMetaInfo("https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap"); // xServer internet
            meta.SetCredentials("xtok", "EBB3ABF6-C1FD-4B01-9D69-349332944AD9"); // set the basic authentication properties, e.g. xtok/token for xServer internet

            return meta;
        }

        // Initialize the xServer base map layers
        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta)
        {
            var baseLayer = new TiledLayer("Background")
            {
                TiledProvider = new XMapTiledProviderEx(meta.Url, XMapMode.Background) { User = meta.User, Password = meta.Password, CustomProfile = "silkysand-bg" },
                Copyright = meta.CopyrightText,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                IsBaseMapLayer = true,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png")                
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProviderEx(meta.Url, XMapMode.Town) { User = meta.User, Password = meta.Password, CustomProfile = "silkysand-fg" },
                Copyright = meta.CopyrightText,
                MaxRequestSize = meta.MaxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png")
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);

            InitFeatureLayers();
        }

        private FeatureLayerPresenter flPresenter;
        private void InitFeatureLayers()
        {
            flPresenter = new FeatureLayerPresenter(Map)
            {
                ReferenceTime = referenceTime.Value,
                UseTrafficIncidents = true,
                UseRestrictionZones = false,
                UseTruckAttributes = false,
                UseSpeedPatterns = false
            };

            flPresenter.RefreshMap();
        }

        private void referenceTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (flPresenter == null)
                return;

            flPresenter.ReferenceTime = referenceTime.Value;

            flPresenter.RefreshMap();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (flPresenter == null)
                return;

            flPresenter.UseTrafficIncidents = trafficIncidents.IsChecked.Value;
            flPresenter.UseRestrictionZones = restrictionZones.IsChecked.Value;
            flPresenter.UseTruckAttributes = truckAttributes.IsChecked.Value;
            flPresenter.UseSpeedPatterns = speedPatterns.IsChecked.Value;

            flPresenter.RefreshMap();
        }
    }
}
