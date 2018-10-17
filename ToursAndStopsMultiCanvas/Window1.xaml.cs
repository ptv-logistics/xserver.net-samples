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
using Ptv.XServer.Controls.Map;
using System.Printing;
using System.IO;
using System.Windows.Media.Animation;
using ToursAndStops.XRouteServiceReference;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ToursAndStops
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        async void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var stations = Station.TestStations;

            var myLayer = new MultiCanvasShapeLayer("MyLayer") {LazyUpdate = false /* test the new lazy feature */};
            Map.Layers.Insert(Map.Layers.IndexOf(Map.Layers["Labels"]), myLayer); // add it below labels


            AddBallon(myLayer, stations[0].Latitude, stations[0].Longitude, Colors.DarkGreen,
                stations[0].ShortDescription, stations[0].Description);
            AddBallon(myLayer, stations[1].Latitude, stations[1].Longitude, Colors.DarkBlue,
                stations[1].ShortDescription, stations[1].Description);
            AddBallon(myLayer, stations[2].Latitude, stations[2].Longitude, Colors.DarkRed,
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

            // await caulated
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
            var xroute = new XRouteWSClient();
            xroute.ClientCredentials.UserName.UserName = "xtok";
            xroute.ClientCredentials.UserName.Password = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9";
            return xroute.calculateRoute(new[]
                {
                    new WaypointDesc
                    {
                        wrappedCoords = new[]
                        {
                            new ToursAndStops.XRouteServiceReference.Point {point = new PlainPoint {x = lon1, y = lat1}}
                        }
                    },
                    new WaypointDesc
                    {
                        wrappedCoords = new[]
                        {
                            new ToursAndStops.XRouteServiceReference.Point {point = new PlainPoint {x = lon2, y = lat2}}
                        }
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

        public void AddBallon(MultiCanvasShapeLayer layer, double lat, double lon, Color color, string text,
            string tooltip)
        {
            // create and initialize ballon
            var ballon = new Balloon
            {
                Color = color,
                Text = text,
                ToolTip = tooltip
            };

            // set geo locatoin
            ShapeCanvas.SetLocation(ballon, new System.Windows.Point(lon, lat));

            // optional use adaptive (zoom-dependant scaling)
            ShapeCanvas.SetScale(ballon, 2.5);
            ShapeCanvas.SetScaleFactor(ballon, 0.1);

            // don't neet a z_index, use TopShapes instead.
            // Canvas.SetZIndex(ballon, 1);

            // add to map
            layer.TopShapes.Add(ballon);
        }

        public void SetPlainLine(PointCollection pc, MultiCanvasShapeLayer layer, Color color, string toolTip)
        {
            MapPolyline poly = new MapPolyline();
            poly.Points = pc;
            poly.MapStrokeThickness = 60;
            poly.StrokeLineJoin = PenLineJoin.Round;
            poly.StrokeStartLineCap = PenLineCap.Flat;
            poly.StrokeEndLineCap = PenLineCap.Triangle;
            poly.Stroke = new SolidColorBrush(color);
            poly.ScaleFactor = .2;
            poly.ToolTip = toolTip;
            layer.Shapes.Add(poly);
        }

        public void SetAnimDash(PointCollection pc, MultiCanvasShapeLayer layer)
        {
            MapPolyline animDashLine = new MapPolyline()
            {
                MapStrokeThickness = 40,
                Points = pc,
                ScaleFactor = 0.2
            };

            animDashLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 255, 255, 255));
            animDashLine.StrokeLineJoin = PenLineJoin.Round;
            animDashLine.StrokeStartLineCap = PenLineCap.Flat;
            animDashLine.StrokeEndLineCap = PenLineCap.Triangle;
            animDashLine.StrokeDashCap = PenLineCap.Triangle;
            var dc = new DoubleCollection {2, 2};
            animDashLine.IsHitTestVisible = false;
            animDashLine.StrokeDashArray = dc;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 4,
                To = 0,
                FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd,
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
            ArrowDashLine arrowDash = new ArrowDashLine();
            arrowDash.Points = pc;
            arrowDash.MapStrokeThickness = 20;
            arrowDash.Stroke = new SolidColorBrush(Colors.Black);
            arrowDash.ScaleFactor = .1;
            arrowDash.Fill = new SolidColorBrush(Colors.White);
            arrowDash.IsHitTestVisible = false;

            layer.Shapes.Add(arrowDash);
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
                return new Station[]  {
                new Station{Latitude = 50.73117, Longitude = 7.10052, Description = "Bonn", ShortDescription = "1"},
                new Station{Latitude = 53.54897, Longitude = 9.99337, Description = "Hamburg", ShortDescription = "2"},
                new Station{Latitude = 51.05347, Longitude = 13.74316, Description = "Dresden", ShortDescription = "3"},
                };
            }
        }
    }
}
