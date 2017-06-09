using System.Net;
using System.Threading.Tasks;
using FormsMapCS.XRouteServiceReference;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Windows.Forms;
using System.Windows.Media;
using System.Linq;
using Point = System.Windows.Point;

namespace FormsMapCS
{
    public partial class Form1 : Form
    {
        private const string token = "30BD1C85-51B0-4CE0-98A9-575837BA9708";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public async void Initialize()
        {
            // initialize base map (for xServer internet)
            formsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            formsMap1.XMapCredentials = "xtok:" + token;

            // add a new Shape Layer
            var layer = new ShapeLayer("MyShapes");
            formsMap1.Layers.Add(layer);

            var startPoint = new Point(8.4, 49);
            var destPoint = new Point(8.4, 50);

            // set map view
            formsMap1.SetEnvelope(new MapRectangle(new[] { startPoint, destPoint }).Inflate(1.25));

            // create start marker
            var startMarker = new Truck
            {
                Color = Colors.Blue,
                Width = 50,
                ToolTip = "Start"
            };

            // set position and add to map
            ShapeCanvas.SetLocation(startMarker, startPoint);
            ShapeCanvas.SetZIndex(startMarker, 10);
            layer.Shapes.Add(startMarker);

            // create destination marker
            var destMarker = new Pyramid
            {
                Color = Colors.Green,
                Width = 50,
                Height = 50,
                ToolTip = "Destination",
            };

            // set position and add to map
            ShapeCanvas.SetLocation(destMarker, destPoint);
            ShapeCanvas.SetZIndex(destMarker, 10);
            layer.Shapes.Add(destMarker);

            // calculate route, non-blocking
            var route = await Task.Run(() => CalcRoute(startPoint.Y, startPoint.X, destPoint.Y, destPoint.X));

            // display route
            SetRoute(route, layer, Colors.Blue, "Route");
        }

        public Route CalcRoute(double lat1, double lon1, double lat2, double lon2)
        {
            var xroute = new XRouteWSClient();

            xroute.ClientCredentials.UserName.UserName = "xtok";
            xroute.ClientCredentials.UserName.Password = token;

            return xroute.calculateRoute(new[]
            {
                new WaypointDesc
                {
                    wrappedCoords =
                        new[] {new XRouteServiceReference.Point {point = new PlainPoint {x = lon1, y = lat1}}}
                },
                new WaypointDesc
                {
                    wrappedCoords =
                        new[] {new XRouteServiceReference.Point {point = new PlainPoint {x = lon2, y = lat2}}}
                }
            },
                null, null,
                new ResultListOptions {polygon = true},
                new CallerContext
                {
                    wrappedProperties = new[]
                    {
                        new CallerContextProperty {key = "CoordFormat", value = "OG_GEODECIMAL"},
                    }
                }
                );
        }

        public void SetRoute(Route route, ShapeLayer layer, Color color, string toolTip)
        {
            var pc = new PointCollection(from p in route.polygon.lineString.wrappedPoints select new System.Windows.Point(p.x, p.y));

            var poly = new MapPolyline
            {
                Points = pc,
                MapStrokeThickness = 40,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Triangle,
                Stroke = new SolidColorBrush(color),
                ScaleFactor = .2,
                ToolTip = toolTip
            };

            layer.Shapes.Add(poly);
        }
    }
}
