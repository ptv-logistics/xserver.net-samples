using System.Net;
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

            formsMap.Layers.Add(CreateWmtsLayer()); 
            formsMap.Layers.Add(CreateWmtsLayer25832());
        }

        private static ILayer CreateWmtsLayer()
        {
            return new WmtsLayer(
                "TopPlusOpen",
                "https://sgx.geodatenzentrum.de/wmts_topplus_open/tile/1.0.0/web/default/WEBMERCATOR/{z}/{y}/{x}.png",
                getTileMatrixSet3857())
            {
                Caption = "Topo (WMTS Demo)",
                IsBaseMapLayer = true
            };
        }

        private static ILayer CreateWmtsLayer25832()
        {
            return new WmtsLayer(
                "WMTSLärmkartierung",
                "https://geoinformation.eisenbahn-bundesamt.de/mapproxy/wmts/verkehrsweg_wmts/wmtsgrid/{z}/{x}/{y}.png",
                getTileMatrixSet25832())
            {
                Caption = "Verkehrsweg (WMTS Demo)",
            };
        }

        private static TileMatrixSet getTileMatrixSet25832() => new TileMatrixSet("EPSG:25832")
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

        private static TileMatrixSet getTileMatrixSet3857() => new TileMatrixSet("EPSG:3857")
        {
            new TileMatrix("00",559082264.02871764,     -20037508.342789244,20037508.342789244,     1,     1),
            new TileMatrix("01",279541132.01435882,     -20037508.342789244,20037508.342789244,     2,     2),
            new TileMatrix("02",139770566.00717941,     -20037508.342789244,20037508.342789244,     4,     4),
            new TileMatrix("03", 69885283.003589705,    -20037508.342789244,20037508.342789244,     8,     8),
            new TileMatrix("04", 34942641.501794852,    -20037508.342789244,20037508.342789244,    16,    16),
            new TileMatrix("05", 17471320.750897426,    -20037508.342789244,20037508.342789244,    32,    32),
            new TileMatrix("06",  8735660.3754487131,   -20037508.342789244,20037508.342789244,    64,    64),
            new TileMatrix("07",  4367830.1877243565,   -20037508.342789244,20037508.342789244,   128,   128),
            new TileMatrix("08",  2183915.0938621783,   -20037508.342789244,20037508.342789244,   256,   256),
            new TileMatrix("09",  1091957.5469310891,   -20037508.342789244,20037508.342789244,   512,   512),
            new TileMatrix("10",   545978.77346554457,  -20037508.342789244,20037508.342789244,  1024,  1024),
            new TileMatrix("11",   272989.38673277228,  -20037508.342789244,20037508.342789244,  2048,  2048),
            new TileMatrix("12",   136494.69336638614,  -20037508.342789244,20037508.342789244,  4096,  4096),
            new TileMatrix("13",    68247.346683193071, -20037508.342789244,20037508.342789244,  8192,  8192),
            new TileMatrix("14",    34123.673341596535, -20037508.342789244,20037508.342789244, 16384, 16384),
            new TileMatrix("15",    17061.836670798268, -20037508.342789244,20037508.342789244, 32768, 32768),
            new TileMatrix("16",     8530.9183353991339,-20037508.342789244,20037508.342789244, 65536, 65536),
            new TileMatrix("17",     4265.4591676995669,-20037508.342789244,20037508.342789244,131072,131072),
            new TileMatrix("18",     2132.7295838497835,-20037508.342789244,20037508.342789244,262144,262144)
        };
    }
}
