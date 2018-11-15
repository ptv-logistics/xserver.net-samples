using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
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
        private static string token = "Insert your xToken here";

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

            var meta = new XMapMetaInfo("https://api-test.cloud.ptvgroup.com/xmap/ws/XMap");
            meta.SetCredentials("xtok", token);
            XMapLayerFactory.InsertXMapBaseLayers(Map.Layers, meta);
            Map.XMapStyle = "gravelpit";

            xynLayer = new XynLayer(meta, "xyn", "XYN Segments")
            {
                UntiledProvider = new XMapTiledProvider(meta.Url, XMapMode.Custom)
                {
                    User = meta.User,
                    Password = meta.Password,
                }
            };

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
    }
}
