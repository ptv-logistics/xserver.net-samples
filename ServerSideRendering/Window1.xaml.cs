using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.Tools;
using System.Windows;
using xserver;

namespace ServerSideRendering
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private bool initialized;
        public Window1()
        {
            InitializeComponent();
            initialized = true;

            Cluster_Changed(this, null);
        }

        private void InitMapLayers()
        {
            string cluster = GetCluster();

            switch (cluster)
            {
                case "eu-n-test":
                    Map.SetMapLocation(new System.Windows.Point(9.182778, 48.775556), 12);
                    break;
                case "au-n-test":
                    Map.SetMapLocation(new System.Windows.Point(138.6, -34.92), 12);
                    break;
                case "na-n-test":
                    Map.SetMapLocation(new System.Windows.Point(-71.415, 41.830833), 12);
                    break;
            }


            Map.Layers.Clear();

            var xmapMetaInfo = new XMapMetaInfo("https://xmap-" + cluster + ".cloud.ptvgroup.com/xmap/ws/XMap");
            xmapMetaInfo.SetCredentials("xtok", "0B5DE87D-8A43-46BD-8606-81877BAF244F");
            InsertXMapBaseLayers(Map.Layers, xmapMetaInfo, GetProfile());

            UpdateFeatureLayers();

            Map.InsertTrafficInfoLayer(xmapMetaInfo, "Traffic", "traffic.ptv-traffic", "Traffic information");
            Map.InsertRoadEditorLayer(xmapMetaInfo, "TruckAttributes", "truckattributes", "truckattributes", "Truck attributes");
            Map.InsertPoiLayer(xmapMetaInfo, "Poi", "default.points-of-interest", "Points of interest");

            if (cluster == "eu-n-test")
                Map.InsertDataManagerLayer(xmapMetaInfo, "POS", "t_f07ef3f0_ce7a_4913_90ea_b072ec07e6ff", "Points Of Sales", 10, true);
        }

        public void UpdateFeatureLayers()
        {
            var bgLayer = ((TiledLayer)Map.Layers["Background"]);
            var bgProvider = ((ExtendedXMapTiledProvider) bgLayer.TiledProvider);

            if (GetPreferredRoutes() || GetRestrictionZones())
            {
                string xmlSnippet =
                    @"<Profile xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><FeatureLayer majorVersion=""1"" minorVersion=""0""><GlobalSettings enableTimeDependency=""true""/><Themes>";
                if (GetRestrictionZones())
                    xmlSnippet = xmlSnippet + @"<Theme id=""PTV_RestrictionZones"" enabled=""true"" priorityLevel=""0""></Theme>";
                if (GetPreferredRoutes())
                    xmlSnippet = xmlSnippet + @"<Theme id=""PTV_PreferredRoutes"" enabled=""true"" priorityLevel=""0""></Theme>";
                xmlSnippet = xmlSnippet + @"</Themes></FeatureLayer></Profile>";
                bgProvider.CustomCallerContextProperties = new[]
                {
                    new CallerContextProperty
                    {
                        key = "ProfileXMLSnippet",
                        value = xmlSnippet                            
                    }
                };
            }
            else
                ((ExtendedXMapTiledProvider) bgLayer.TiledProvider).CustomCallerContextProperties = null;

            bgProvider.CustomProfile = GetProfile() + "-bg";
            bgLayer.Refresh();

            var fgLayer = ((UntiledLayer)Map.Layers["Labels"]);
            var fgProvider = ((XMapTiledProvider)fgLayer.UntiledProvider);
            fgProvider.CustomProfile = GetProfile() + "-fg";
            fgLayer.Refresh();
        }

        public void InsertXMapBaseLayers(LayerCollection layers, XMapMetaInfo meta, string profile)
        {
            var baseLayer = new TiledLayer("Background")
            {
                TiledProvider = new ExtendedXMapTiledProvider(meta.Url, meta.User, meta.Password)
                {
                    ContextKey = "in case of context key",
                    CustomProfile = profile + "-bg",
                },
                Copyright = meta.CopyrightText,
                Caption = MapLocalizer.GetString(MapStringId.Background),
                IsBaseMapLayer = true,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
            };

            var labelLayer = new UntiledLayer("Labels")
            {
                UntiledProvider = new XMapTiledProvider(
                    meta.Url, XMapMode.Town)
                {
                    User = meta.User, Password = meta.Password, ContextKey = "in case of context key",
                    CustomProfile = profile + "-fg",
                },               
                Copyright = meta.CopyrightText,
                MaxRequestSize = meta.MaxRequestSize,
                Caption = MapLocalizer.GetString(MapStringId.Labels),
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Labels.png"),
            };

            layers.Add(baseLayer);
            layers.Add(labelLayer);
        }

        private string GetCluster()
        {
            var clusters = new[] { "eu-n-test", "na-n-test", "au-n-test" };
            return clusters[mapCluster.SelectedIndex];
        }

        private string GetProfile()
        {
            var profiles = new[] { "silkysand", "sandbox", "ajax" };
            return profiles[mapProfile.SelectedIndex];
        }

        private bool GetPreferredRoutes()
        {
            return preferredRoutes.IsEnabled && (preferredRoutes.IsChecked?? false);
        }

        private bool GetRestrictionZones()
        {
            return restrictionZones.IsEnabled && (restrictionZones.IsChecked ?? false);
        }

        private void Profile_Changed(object sender, RoutedEventArgs e)
        {
            if (!initialized)
                return;

            UpdateFeatureLayers();
        }

        private void Cluster_Changed(object sender, RoutedEventArgs e)
        {
            if (!initialized)
                return;

            string cluster = GetCluster();
            restrictionZones.IsEnabled = (cluster == "eu-n-test");
            preferredRoutes.IsEnabled = (cluster != "eu-n-test");

            InitMapLayers();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!initialized)
                return;

            UpdateFeatureLayers();
        }
    }
}
