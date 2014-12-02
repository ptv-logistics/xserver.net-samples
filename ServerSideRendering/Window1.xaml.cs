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
            var xmapMetaInfo = new XMapMetaInfo("http://80.146.239.180/xmap/ws/XMap");

            Map.InsertTrafficInfoLayer(xmapMetaInfo, "Traffic", "traffic.ptv-traffic", "Traffic information");
            Map.InsertRoadEditorLayer(xmapMetaInfo, "TruckAttributes", "truckattributes", "truckattributes",
                "Truck attributes");
            Map.InsertPoiLayer(xmapMetaInfo, "Poi", "default.points-of-interest", "Points of interest");
        }
    }
}
