using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Windows.Media.Animation;
using ToursAndStops.XRouteServiceReference;
using System.Threading.Tasks;

namespace ToursAndStops
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            Map.Loaded += Map_Loaded;
        }

        private async void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var stations = Station.TestStations;

            var myLayer = new MultiCanvasShapeLayer("MyLayer") {LazyUpdate = false /* test the new lazy feature */};
            Map.Layers.Insert(Map.Layers.IndexOf(Map.Layers["Labels"]), myLayer); // add it below labels


            AddBalloon(myLayer, stations[0].Latitude, stations[0].Longitude, Colors.DarkGreen,
                stations[0].ShortDescription, stations[0].Description);
            AddBalloon(myLayer, stations[1].Latitude, stations[1].Longitude, Colors.DarkBlue,
                stations[1].ShortDescription, stations[1].Description);
            AddBalloon(myLayer, stations[2].Latitude, stations[2].Longitude, Colors.DarkRed,
                stations[2].ShortDescription, stations[2].Description);

            Map.SetEnvelope(new MapRectangle(7.10052, 13.74316, 50.73117, 53.54897).Inflate(1.3));

            // calculate 3 sample routes async (= parallel)
            var tasks = new List<Task<Route>>
            {
                Task.Run(() => CalcRoute(stations[0].Latitude, stations[0].Longitude, stations[1].Latitude,
                    stations[1].Longitude)),
                Task.Run(() => CalcRoute(stations[1].Latitude, stations[1].Longitude, stations[2].Latitude,
                    stations[2].Longitude)),
                Task.Run(() => CalcRoute(stations[2].Latitude, stations[2].Longitude, stations[0].Latitude,
                    stations[0].Longitude))
            };

            await Task.WhenAll(tasks);

            foreach (var r in tasks.Select((t, i) => new {i, t.Result}))
            {
                switch (r.i)
                {
                    case 0:
                        var pc = new PointCollection(from p in r.Result.polygon.lineString.wrappedPoints
                            select new System.Windows.Point(p.x, p.y));
                        SetPlainLine(pc, myLayer, Colors.Blue, "Plain Line");
                        break;
                    case 1:
                        pc = new PointCollection(from p in r.Result.polygon.lineString.wrappedPoints
                            select new System.Windows.Point(p.x, p.y));
                        SetPlainLine(pc, myLayer, Colors.Red, "Animated Dash");
                        SetAnimDash(pc, myLayer);
                        break;
                    case 2:
                        pc = new PointCollection(from p in r.Result.polygon.lineString.wrappedPoints
                            select new System.Windows.Point(p.x, p.y));
                        SetPlainLine(pc, myLayer, Colors.Green, "Arrow Dash");
                        SetArrowDash(pc, myLayer);
                        break;
                }
            }
        }

        public Route CalcRoute(double lat1, double lon1, double lat2, double lon2)
        {
            var xRoute = new XRouteWSClient();
            if (xRoute.ClientCredentials != null)
            {
                xRoute.ClientCredentials.UserName.UserName = "xtok";
                xRoute.ClientCredentials.UserName.Password = "Insert your xToken here";
            }

            return xRoute.calculateRoute(new[]
                {
                    new WaypointDesc
                    {
                        wrappedCoords = new[]
                        {
                            new XRouteServiceReference.Point {point = new PlainPoint {x = lon1, y = lat1}}
                        }
                    },
                    new WaypointDesc
                    {
                        wrappedCoords = new[]
                        {
                            new XRouteServiceReference.Point {point = new PlainPoint {x = lon2, y = lat2}}
                        }
                    }
                },
                null, null,
                new ResultListOptions {polygon = true},
                new CallerContext
                {
                    wrappedProperties = new[]
                    {
                        new CallerContextProperty {key = "CoordFormat", value = "OG_GEODECIMAL"}
                    }
                }
            );
        }

        public void AddBalloon(MultiCanvasShapeLayer layer, double lat, double lon, Color color, string text,
            string tooltip)
        {
            // create and initialize balloon
            var balloon = new Balloon
            {
                Color = color,
                Text = text,
                ToolTip = tooltip
            };

            // set geo location
            ShapeCanvas.SetLocation(balloon, new System.Windows.Point(lon, lat));

            // optional use adaptive (zoom-dependent scaling)
            ShapeCanvas.SetScale(balloon, 2.5);
            ShapeCanvas.SetScaleFactor(balloon, 0.1);

            // don't need a z_index, use TopShapes instead.
            // Canvas.SetZIndex(balloon, 1);

            // add to map
            layer.TopShapes.Add(balloon);
        }

        public void SetPlainLine(PointCollection pc, MultiCanvasShapeLayer layer, Color color, string toolTip)
        {
            var mapPolyline = new MapPolyline
            {
                Points = pc,
                MapStrokeThickness = 60,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Triangle,
                Stroke = new SolidColorBrush(color),
                ScaleFactor = .2,
                ToolTip = toolTip
            };
            layer.Shapes.Add(mapPolyline);
        }

        public void SetAnimDash(PointCollection pc, MultiCanvasShapeLayer layer)
        {
            var animDashLine = new MapPolyline
            {
                MapStrokeThickness = 40,
                Points = pc,
                ScaleFactor = 0.2,
                Stroke = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Triangle,
                StrokeDashCap = PenLineCap.Triangle,
                IsHitTestVisible = false,
                StrokeDashArray = new DoubleCollection {2, 2}
            };

            var animation = new DoubleAnimation
            {
                From = 4,
                To = 0,
                FillBehavior = FillBehavior.HoldEnd,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var strokeStoryboard = new Storyboard();
            strokeStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Line.StrokeDashOffset)"));
            Storyboard.SetTarget(animation, animDashLine);
            strokeStoryboard.Begin();
            layer.Shapes.Add(animDashLine);
        }

        public void SetArrowDash(PointCollection pc, MultiCanvasShapeLayer layer)
        {
            var arrowDashLine = new ArrowDashLine
            {
                Points = pc,
                MapStrokeThickness = 20,
                Stroke = new SolidColorBrush(Colors.Black),
                ScaleFactor = .1,
                Fill = new SolidColorBrush(Colors.White),
                IsHitTestVisible = false
            };

            layer.Shapes.Add(arrowDashLine);
        }
    }

    public class Station
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }


        public static Station[] TestStations
        {
            get
            {
                return new[]  {
                new Station{Latitude = 50.73117, Longitude = 7.10052, Description = "Bonn", ShortDescription = "1"},
                new Station{Latitude = 53.54897, Longitude = 9.99337, Description = "Hamburg", ShortDescription = "2"},
                new Station{Latitude = 51.05347, Longitude = 13.74316, Description = "Dresden", ShortDescription = "3"}
                };
            }
        }
    }
}
