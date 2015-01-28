using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows.Controls.DataVisualization.Charting;
using Ptv.XServer.Controls.Map;
using System.Printing;
using System.IO;
using Ptv.XServer.Demo.Tools;
using Ptv.XServer.Controls.Map.Tools;

namespace ServerSideRendering
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var xmapMetaInfo = new XMapMetaInfo("https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap");
            xmapMetaInfo.SetCredentials("xtok", <insert your xserver internet token>);
            XMapLayerFactory.InsertXMapBaseLayers(Map.Layers, xmapMetaInfo);
            Map.XMapStyle = "sandbox";

            Map.InsertTrafficInfoLayer(xmapMetaInfo, "Traffic", "traffic.ptv-traffic", "Traffic information");
            Map.InsertRoadEditorLayer(xmapMetaInfo, "TruckAttributes", "truckattributes", "truckattributes", "Truck attributes");
            Map.InsertPoiLayer(xmapMetaInfo, "Poi", "default.points-of-interest", "Points of interest");
            Map.InsertDataManagerLayer(xmapMetaInfo, "POS", "t_f07ef3f0_ce7a_4913_90ea_b072ec07e6ff", "Points Of Sales", 10, true);
        }
    }
}
