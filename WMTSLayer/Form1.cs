using System.Windows;
using System.Windows.Forms;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.WmtsLayer;

namespace XMap2FactoryTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            formsMap.SetMapLocation(new Point(8.4044, 49.01405), 10);
            formsMap.MaxZoom = 22;

            formsMap.XMapUrl = "xserver2-europe-eu-test";
            formsMap.XMapCredentials = "xtok:EBB3ABF6-C1FD-4B01-9D69-349332944AD9";

            // formsMap.XMapUrl = "eu-n-test";
            formsMap.XMapUrl = "http://172.23.112.32:40000"; // SmarTour-Server, Linux-based
            // formsMap.XMapUrl = "http://172.23.112.154:40000"; // SmarTour-Server, Windows-based
            // formsMap.XMapUrl = "http://xserver-2:40000";
            // formsMap.XMapUrl = "https://xserver2-europe-eu-test.cloud.ptvgroup.com";
            formsMap.XMapCredentials = "xtok:9358789A-A8CF-4CA8-AC99-1C0C4AC07F1E";

            formsMap.Layers.InsertBefore(CreateWmtsLayer(), "Labels"); // WMTS-Layer after shape layer but also in-between background and foreground
        }

        private static ILayer CreateWmtsLayer()
        {
            return new WmtsLayer(
                "WMTSLärmkartierung",
                "http://laermkartierung1.eisenbahn-bundesamt.de/mapproxy/wmts/ballungsraum_wmts/wmtsgrid/{z}/{x}/{y}.png",
                getTileMatrixSet())
            {
                Caption = "Lärmkartierung (WMTS Demo)",
                IsBaseMapLayer = true
            };
        }

        private static TileMatrixSet getTileMatrixSet() => new TileMatrixSet("EPSG:25832")
        {
            new TileMatrix("00", 12980398.9955000000, 204485.0, 6134557.0,      1,      1),
            new TileMatrix("01",  6490199.4977700000, 204485.0, 6134557.0,      2,      2),
            new TileMatrix("02",  3245099.7488800000, 204485.0, 6134557.0,      4,      4),
            new TileMatrix("03",  1622549.8744400000, 204485.0, 6134557.0,      7,      8),
            new TileMatrix("04",   811274.9372210000, 204485.0, 6134557.0,     14,     16),
            new TileMatrix("05",   405637.4686100000, 204485.0, 6134557.0,     28,     32),
            new TileMatrix("06",   202818.7343050000, 204485.0, 6134557.0,     56,     64),
            new TileMatrix("07",   101409.3671530000, 204485.0, 6134557.0,    111,    128),
            new TileMatrix("08",    50704.6835763000, 204485.0, 6134557.0,    222,    256),
            new TileMatrix("09",    25352.3417882000, 204485.0, 6134557.0,    443,    512),
            new TileMatrix("10",    12676.1708941000, 204485.0, 6134557.0,    885,   1024),
            new TileMatrix("11",     6338.0854470400, 204485.0, 6134557.0,   1770,   2048),
            new TileMatrix("12",     3169.0427235200, 204485.0, 6134557.0,   3540,   4096),
            new TileMatrix("13",     1584.5213617600, 204485.0, 6134557.0,   7080,   8192),
            new TileMatrix("14",      792.2606808800, 204485.0, 6134557.0,  14160,  16384),
            new TileMatrix("15",      396.1303404400, 204485.0, 6134557.0,  28320,  32768),
            new TileMatrix("16",      198.0651702200, 204485.0, 6134557.0,  56639,  65536),
            new TileMatrix("17",       99.0325851100, 204485.0, 6134557.0, 113278, 131072),
            new TileMatrix("18",       49.5162925550, 204485.0, 6134557.0, 226555, 262144),
            new TileMatrix("19",       24.7581462775, 204485.0, 6134557.0, 453109, 524288)
        };
    }
}

