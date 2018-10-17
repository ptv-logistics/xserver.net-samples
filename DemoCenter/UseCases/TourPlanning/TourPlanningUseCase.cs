// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Ptv.XServer.Demo.UseCases.TourPlanning
{
    /// <summary>
    /// This class concentrates all relevant activities and resources for planning a tour via xTour.
    /// </summary>
    public class TourPlanningUseCase : UseCase
    {
        private ShapeLayer tourLayer;
        private ShapeLayer orderLayer;
        private ShapeLayer depotLayer;
        private readonly Color unplannedColor = Colors.LightGray;
        private TourCalculationWrapper tourCalculationWrapper;

        /// <summary>
        /// Tries to create all relevant layers and other resources needed for this use case.
        /// </summary>
        protected override void Enable()
        {
            #region doc:CreateLayers
            tourLayer = new ShapeLayer("Tours");
            wpfMap.Layers.InsertBefore(tourLayer, "Labels"); // add before xmap labels

            orderLayer = new ShapeLayer("Orders");
            wpfMap.Layers.Add(orderLayer);

            depotLayer = new ShapeLayer("Depots");
            wpfMap.Layers.Add(depotLayer);
            #endregion doc:CreateLayers
        }

        /// <summary>
        /// Removes the tour layer from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(tourLayer);
            wpfMap.Layers.Remove(orderLayer);
            wpfMap.Layers.Remove(depotLayer);

            SendProgressInfo("Deactivated");
        }

        private Scenario scenario;

        /// <summary>
        /// Configure the tour planning with a scenario by specifying its size via <paramref name="scenarioSize"/>.
        /// </summary>
        /// <param name="scenarioSize">Specifies the amount of objects, which will be created randomly.</param>
        public async void SetScenario(ScenarioSize scenarioSize)
        {
            SendProgressInfo("Initializing...");

            var center = new Point(6.130833, 49.611389); // LUX
            //var center = new System.Windows.Point(8.4, 49); // KA
            var radius = 7.5; // radius in km

            try
            {
                var s = await Task.Run(() => RandomScenarioBuilder.CreateScenario(scenarioSize, center, radius));

                wpfMap.SetMapLocation(center, 10);
                SetScenario(s);
                Finished?.Invoke();
                SendProgressInfo("Ready");
                Initialized();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetScenario(Scenario newScenario)
        {
            scenario = newScenario;

            #region doc:ConfigureLayers

            tourLayer.Shapes.Clear();
            orderLayer.Shapes.Clear();
            depotLayer.Shapes.Clear();

            foreach (var order in scenario.Orders)
            {
                var pin = new Cube { Color = unplannedColor };
                pin.Width = pin.Height = Math.Sqrt(order.Quantity) * 10;
                ShapeCanvas.SetLocation(pin, new Point(order.Longitude, order.Latitude));
                orderLayer.Shapes.Add(pin);
                pin.Tag = order;
            }

            foreach (var depot in scenario.Depots)
            {
                var pyramid = new Pyramid();
                pyramid.Width = pyramid.Height = 30;
                pyramid.Color = depot.Color;
                ShapeCanvas.SetLocation(pyramid, new Point(depot.Longitude, depot.Latitude));
                depotLayer.Shapes.Add(pyramid);
            }

            #endregion // doc:ConfigureLayers
        }

        private void RenderPlannedTours()
        {
            tourLayer.Shapes.Clear();
            foreach (var tour in scenario.Tours)
            {
                var pc = new PointCollection(from tp in tour.TourPoints select new Point(tp.Longitude, tp.Latitude));
                SetPlainLine(pc, tourLayer, tour.Vehicle.Depot.Color, tour.Vehicle.Id);
                SetAnimDash(pc, tourLayer);
            }

            foreach (var frameworkElement in orderLayer.Shapes)
            {
                var cube = (Cube) frameworkElement;
                var order = (Order)cube.Tag;
                cube.Color = order.Tour?.Vehicle.Depot.Color ?? unplannedColor;
            }

            Finished?.Invoke();
        }

        public void SetPlainLine(PointCollection pc, ShapeLayer layer, Color color, string toolTip)
        {
            MapPolyline poly = new MapPolyline
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
            poly.MouseEnter += (s, e) => poly.Stroke = new SolidColorBrush(Colors.Cyan);
            poly.MouseLeave += (s, e) => poly.Stroke = new SolidColorBrush(color);
            layer.Shapes.Add(poly);
        }

        public void SetAnimDash(PointCollection pc, ShapeLayer layer)
        {
            MapPolyline animDashLine = new MapPolyline
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

            DoubleAnimation animation = new DoubleAnimation
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
        private void SendProgressInfo(string message, int percentage = 0)
        {
            Progress?.Invoke(message, percentage);
        }

        /// <summary>Callback delegate for getting informed about changes in the progress of the tour planning.</summary>
        public Action<string, int> Progress;
        /// <summary>Callback delegate for getting informed about the termination of the tour planning. </summary>
        public Action Finished;
        /// <summary>Callback delegate for getting informed when a new scenario is set up. </summary>
        public Action Initialized;

        /// <summary> Triggers the asynchronous call of the tour planning.</summary>
        public void StartPlanning()
        {
            #region doc:StartTourPlanning
            tourCalculationWrapper = new TourCalculationWrapper();
            tourCalculationWrapper.Progress = () => SendProgressInfo(tourCalculationWrapper.ProgressMessage, tourCalculationWrapper.ProgressPercent);
            tourCalculationWrapper.Finished = RenderPlannedTours;
            tourCalculationWrapper.StartPlanScenario(scenario);
            #endregion // doc:StartTourPlanning
        }

        /// <summary> Stops the asynchronous called tour planning.</summary>
        public void StopPlanning()
        {
            tourCalculationWrapper.Cancel();
        }
    }
}
