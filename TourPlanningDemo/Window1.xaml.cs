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
    public partial class Window1
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

        public const string XMapCredentials = App.Token;

        private readonly ShapeLayer tourLayer;
        private readonly ShapeLayer orderLayer;
        private readonly ShapeLayer depotLayer;

        private Scenario scenario;
        private readonly Color unplannedColor = Colors.LightGray;
        private TourCalcWrapper tourCalcWrapper;

        private bool firstTime = true;
        public void SetScenario(Scenario usedScenario)
        {
            scenario = usedScenario;
            scenario.Profile = ProfileComboBox.Text;

            statusLabel.Content = "Scenario initialized";
            StartButton.IsEnabled = true;
            ScenarioComboBox.IsEnabled = true;

            tourLayer.Shapes.Clear();
            orderLayer.Shapes.Clear();
            depotLayer.Shapes.Clear();
            
            // add orders, oder by latitude so they overlap nicely on the map
            foreach (var order in from o in usedScenario.Orders orderby o.Latitude descending select o)
            {
                var cube = new Cube
                {
                    Color = unplannedColor,
                    Width = Math.Sqrt(order.Quantity) * 10,
                    Height = Math.Sqrt(order.Quantity) * 10,
                    ToolTip = order.Description
                };

                ShapeCanvas.SetLocation(cube, new Point(order.Longitude, order.Latitude));
                orderLayer.Shapes.Add(cube);
                cube.Tag = order;
            }

            // add depots, oder by latitude so they overlap nicely on the map
            foreach (var depot in from d in usedScenario.Depots orderby d.Latitude descending select d)
            {
                var pin = new Pyramid {
                    Width = 30,
                    Height = 30,
                    Color = depot.Color,
                    ToolTip = depot.Description 
                };
                ShapeCanvas.SetLocation(pin, new Point(depot.Longitude, depot.Latitude));
                depotLayer.Shapes.Add(pin);
            }

            if (!firstTime) return;
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

        public void SetPlannedTours(Scenario usedScenario)
        {
            CancelButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            ScenarioComboBox.IsEnabled = true;

            tourLayer.Shapes.Clear();
            foreach (var tour in usedScenario.Tours)
            {
                var pc = new PointCollection(from tp in tour.TourPoints select new Point(tp.Longitude, tp.Latitude));
                SetPlainLine(pc, tourLayer, tour.Vehicle.Depot.Color, tour.Vehicle.Id);
                SetAnimDash(pc, tourLayer);
            }

            foreach(var frameworkElement in orderLayer.Shapes)
            {
                var cube = (Cube) frameworkElement;
                var order = (Order)cube.Tag;
                cube.Color = order.Tour != null ? order.Tour.Vehicle.Depot.Color : unplannedColor;
            }
        }

        public void SetPlainLine(PointCollection pc, ShapeLayer layer, Color color, string toolTip)
        {
            var mapPolyline = new MapPolyline
            {
                Points = pc,
                MapStrokeThickness = 20,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Triangle,
                Stroke = new SolidColorBrush(color),
                ScaleFactor = .2,
                ToolTip = toolTip
            };
            mapPolyline.MouseEnter += (s, e) => mapPolyline.Stroke = new SolidColorBrush(Colors.Cyan);
            mapPolyline.MouseLeave += (s, e) => mapPolyline.Stroke = new SolidColorBrush(color);
            layer.Shapes.Add(mapPolyline);
        }

        public void SetAnimDash(PointCollection pc, ShapeLayer layer)
        {
            var animDashLine = new MapPolyline
            {
                MapStrokeThickness = 16,
                Points = pc,
                ScaleFactor = 0.2,
                Stroke = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Triangle,
                StrokeDashCap = PenLineCap.Triangle
            };

            var dc = new DoubleCollection { 2, 2 };
            animDashLine.IsHitTestVisible = false;
            animDashLine.StrokeDashArray = dc;

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

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (statusLabel != null)
                statusLabel.Content = "Initializing...";

            var center = new Point(6.130833, 49.611389); // LUX
            //var center = new System.Windows.Point(8.4, 49); // KA
            var scenarioSize = (ScenarioSize)Enum.Parse(typeof(ScenarioSize), (string)((ComboBoxItem)e.AddedItems[0]).Content);

            try
            {
                var s = await Task.Run(() => RandomScenarioBuilder.CreateScenario(scenarioSize, center));

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