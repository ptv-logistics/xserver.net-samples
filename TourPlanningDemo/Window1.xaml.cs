using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Linq;
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
            InitializeComponent();

            tourLayer = new ShapeLayer("Tours");
            Map.Layers.InsertBefore(tourLayer, "Labels"); // add before xmap labels

            orderLayer = new ShapeLayer("Orders");
            Map.Layers.Add(orderLayer);

            depotLayer = new ShapeLayer("Depots");
            Map.Layers.Add(depotLayer);
        }

        public const string XMapCredentials = "xtok:" + App.Token;

        ShapeLayer tourLayer;
        ShapeLayer orderLayer;
        ShapeLayer depotLayer;
        Scenario scenario;
        Color unplannedColor = Color.FromRgb(255, 64, 64);
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

            foreach (var order in scenario.Orders)
            {
                var pin = new Ptv.XServer.Controls.Map.Symbols.Cube();
                pin.Color = unplannedColor;
                pin.Width = pin.Height = Math.Sqrt(order.Quantity) * 10;
                ShapeCanvas.SetLocation(pin, new Point(order.Longitude, order.Latitude));
                orderLayer.Shapes.Add(pin);
                pin.Tag = order;
            }

            foreach (var depot in scenario.Depots)
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
                    "* Using the job API to control and display the tour calculation progress", "Info");
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
                    ShapeCanvas.SetZIndex(cube, 1); // bring to front
                }
                else
                {
                    cube.Color = unplannedColor;
                    ShapeCanvas.SetZIndex(cube, 0); // bring to back
                }
            }
        }

        public void SetPlainLine(PointCollection pc, ShapeLayer layer, Color color, string toolTip)
        {
            MapPolyline poly = new MapPolyline();
            poly.Points = pc;
            poly.MapStrokeThickness = 30;
            poly.StrokeLineJoin = PenLineJoin.Round;
            poly.StrokeStartLineCap = PenLineCap.Flat;
            poly.StrokeEndLineCap = PenLineCap.Triangle;
            poly.Stroke = new SolidColorBrush(color);
            poly.ScaleFactor = .2;
            poly.ToolTip = toolTip;
            layer.Shapes.Add(poly);
        }

        public void SetAnimDash(PointCollection pc, ShapeLayer layer)
        {
            MapPolyline animDashLine = new MapPolyline()
            {
                MapStrokeThickness = 20,
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

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(statusLabel != null)
                statusLabel.Content = "Initializing...";

//            var center = new System.Windows.Point(6.130833, 49.611389); // LUX
            var center = new System.Windows.Point(8.4, 49); // KA
            var radius = .2; // radius in degrees of latitude
            var scenario = (ScenarioSize)System.Enum.Parse(typeof(ScenarioSize), ((string)((ComboBoxItem)e.AddedItems[0]).Content));

            Tools.AsyncUIHelper(() => ScenarioBuilder.CreateRandomScenario(scenario, center, radius),
                         (s) => { Map.SetMapLocation(center, 10); SetScenario(s); },
                        (ex) => { MessageBox.Show(ex.Message); });

        }
    }
}
