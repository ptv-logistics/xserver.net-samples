using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TourPlanningDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            // infinite zoom makes the map more smooth at deep zoom levels
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = true;

            InitializeComponent();

            tourLayer = new ShapeLayer("Tours");
            Map.Layers.InsertBefore(tourLayer, "Labels"); // add before xmap labels

            orderLayer = new ShapeLayer("Orders");
            Map.Layers.Add(orderLayer);

            depotLayer = new ShapeLayer("Depots");
            Map.Layers.Add(depotLayer);
        }

        public const string XMapCredentials = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9:" + App.Token;

        ShapeLayer tourLayer;
        ShapeLayer orderLayer;
        ShapeLayer depotLayer;

        Scenario scenario;
        Color unplannedColor = Colors.LightGray;
        TourCalcWrapper tourCalcWrapper;

        bool firstTime = true;
        public void SetScenario(Scenario scenario)
        {
            this.scenario = scenario;

            statusLabel.Content = "Scenario initialized";
            StartButton.IsEnabled = true;
            ScenarioComboBox.IsEnabled = true;

            tourLayer.Shapes.Clear();
            orderLayer.Shapes.Clear();
            depotLayer.Shapes.Clear();
            
            // add orders, oder by latitude so they overlap nicely on the map
            foreach (var order in from o in scenario.Orders orderby o.Latitude descending select o)
            {
                var pin = new Ptv.XServer.Controls.Map.Symbols.Cube();
                pin.Color = unplannedColor;
                pin.Width = pin.Height = Math.Sqrt(order.Quantity) * 10;
                ShapeCanvas.SetLocation(pin, new Point(order.Longitude, order.Latitude));
                orderLayer.Shapes.Add(pin);
                pin.Tag = order;
            }

            // add depots, oder by latitude so they overlap nicely on the map
            foreach (var depot in from d in scenario.Depots orderby d.Latitude descending select d)
            {
                var pin = new Ptv.XServer.Controls.Map.Symbols.Pyramid();
                pin.Width = pin.Height = 30;
                pin.Color = depot.Color;
                ShapeCanvas.SetLocation(pin, new Point(depot.Longitude, depot.Latitude));
                depotLayer.Shapes.Add(pin);
            }

            if (firstTime)
            {
                firstTime = false;
                MessageBox.Show(this,
                    "This sample shows some best practices for PTV xTour in a " +
                    ".NET Windows client application. The main techniques demonstrated in this sample:\n" +
                    "* Using xTour at xServer internet\n" +
                    "* Visualizing the tour plan with the xServer .NET control\n" +
                    "* Mapping your application objects from and to xServer objects\n" +
                    "* Invoking PTV xServers from a windows application in a non-blocking way\n" +
                    "* Using the job API to control and display the tour calculation progress",
                    "What's this?");
            }
        }

        public void SetPlannedTours(Scenario scenario)
        {
            CancelButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            ScenarioComboBox.IsEnabled = true;

            tourLayer.Shapes.Clear();
            foreach (var tour in scenario.Tours)
            {
                var pc = new PointCollection(from tp in tour.TourPoints select new System.Windows.Point(tp.Longitude, tp.Latitude));
                SetPlainLine(pc, tourLayer, tour.Vehicle.Depot.Color, tour.Vehicle.Id);
                SetAnimDash(pc, tourLayer);
            }

            foreach(Cube cube in this.orderLayer.Shapes)
            {
                var order = (Order)cube.Tag;
                if (order.Tour != null)
                {
                    cube.Color = order.Tour.Vehicle.Depot.Color;
                }
                else
                {
                    cube.Color = unplannedColor;
                }
            }
        }

        public void SetPlainLine(PointCollection pc, ShapeLayer layer, Color color, string toolTip)
        {
            MapPolyline poly = new MapPolyline();
            poly.Points = pc;
            poly.MapStrokeThickness = 20;
            poly.StrokeLineJoin = PenLineJoin.Round;
            poly.StrokeStartLineCap = PenLineCap.Flat;
            poly.StrokeEndLineCap = PenLineCap.Triangle;
            poly.Stroke = new SolidColorBrush(color);
            poly.ScaleFactor = .2;
            poly.ToolTip = toolTip;
            poly.MouseEnter += (s, e) => poly.Stroke = new SolidColorBrush(Colors.Cyan);
            poly.MouseLeave += (s, e) => poly.Stroke = new SolidColorBrush(color);
            layer.Shapes.Add(poly);
        }

        public void SetAnimDash(PointCollection pc, ShapeLayer layer)
        {
            MapPolyline animDashLine = new MapPolyline()
            {
                MapStrokeThickness = 16,
                Points = pc,
                ScaleFactor = 0.2
            };

            animDashLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 255, 255, 255));
            animDashLine.StrokeLineJoin = PenLineJoin.Round;
            animDashLine.StrokeStartLineCap = PenLineCap.Flat;
            animDashLine.StrokeEndLineCap = PenLineCap.Triangle;
            animDashLine.StrokeDashCap = PenLineCap.Triangle;
            var dc = new DoubleCollection { 2, 2 };
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

        private void StartClick(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            ScenarioComboBox.IsEnabled = false;

            tourCalcWrapper = new TourCalcWrapper();
            tourCalcWrapper.Progress = () => { statusLabel.Content = tourCalcWrapper.ProgressMessage; progressBar1.Value = tourCalcWrapper.ProgressPercent; };
            tourCalcWrapper.Finished = () => { SetPlannedTours(tourCalcWrapper.scenario); };
            tourCalcWrapper.StartPlanScenario(scenario);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            tourCalcWrapper.Cancel();

            CancelButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            ScenarioComboBox.IsEnabled = true;
        }

        private async void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (statusLabel != null)
                statusLabel.Content = "Initializing...";

            var center = new System.Windows.Point(6.130833, 49.611389); // LUX
            //var center = new System.Windows.Point(8.4, 49); // KA
            var radius = 7.5; // radius in km
            var scenario = (ScenarioSize)System.Enum.Parse(typeof(ScenarioSize), ((string)((ComboBoxItem)e.AddedItems[0]).Content));

            try
            {
                var s = await Task.Run(() => RandomScenarioBuilder.CreateScenario(scenario, center, radius));

                Map.SetMapLocation(center, 10);
                SetScenario(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}