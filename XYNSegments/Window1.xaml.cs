using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.Tools;
using System.Windows;
using System.Windows.Input;
using xserver;

namespace XynSegments
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private static string token = "FBB7CABE-0CC9-4831-A252-5FE650FF225A";

        public Window1()
        {
            InitializeComponent();

            InitMapLayers();
        }

        private XynLayer xynLayer;

        private void InitMapLayers()
        {
            Map.SetMapLocation(new System.Windows.Point(8.4044, 49.01405), 14);

            Map.Layers.Clear();

            var xmapMetaInfo = new XMapMetaInfo("https://api-test.cloud.ptvgroup.com/xmap/ws/XMap");
            xmapMetaInfo.SetCredentials("xtok", token);
            InsertXMapBaseLayers(Map.Layers, xmapMetaInfo, "silkysand");

            xynLayer = new XynLayer(xmapMetaInfo, "Segments", "Selected Segments");
            Map.Layers.Add(xynLayer);

            Map.MouseDown += Map_MouseDown;
        }

        private void Map_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var wgs = Map.MouseToGeo(e);

            var xlocate = new XLocateServiceReference.XLocateWSClient();
            xlocate.ClientCredentials.UserName.UserName = "xtok";
            xlocate.ClientCredentials.UserName.Password = token;

            var result = xlocate.findLocation(
                new XLocateServiceReference.Location
                {
                    coordinate =
                        new XLocateServiceReference.Point
                        {
                            point = new XLocateServiceReference.PlainPoint { x = wgs.X, y = wgs.Y }
                        }
                },
                null, null, new[] { XLocateServiceReference.ResultField.XYN },
                new XLocateServiceReference.CallerContext
                {
                    wrappedProperties = new[]
                    {
                            new XLocateServiceReference.CallerContextProperty
                            {
                                key = "CoordFormat",
                                value = "OG_GEODECIMAL"
                            }
                    }
                }
                );

            if (result.wrappedResultList != null && result.wrappedResultList.Length > 0 &&
                result.wrappedResultList[0].wrappedAdditionalFields.Length == 1)
                xynLayer.SetXYN(result.wrappedResultList[0].wrappedAdditionalFields[0].value);
            else
                xynLayer.SetXYN(null);
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
                    User = meta.User,
                    Password = meta.Password,
                    ContextKey = "in case of context key",
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
    }
}
