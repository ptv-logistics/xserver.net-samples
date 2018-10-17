// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System.Collections.Generic;
using System.Windows.Controls.DataVisualization.Charting;

namespace Ptv.XServer.Demo.UseCases.Shapes
{
    /// <summary> <para>Demonstrates the display of different shapes in the map.</para>
    /// <para>See the <conceptualLink target="101dba72-fb36-468b-aa99-4b9c5bbfb62f"/> topic for an example.</para> </summary>
    public class DifferentShapesUseCase : UseCase
    {
        #region private variables

        private ShapeLayer shapeLayer;
        private ShapeLayer shapeLayer2;
        #endregion

        /// <summary> Initializes a new instance of the <see cref="DifferentShapesUseCase"/> class. Adds elements to the map. </summary>
        protected override void Enable()
        {
            AddElements();
        }

        /// <summary> Removes the ellipse from the map. </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["DifferentShapes"]);
            wpfMap.Layers.Remove(wpfMap.Layers["PieCharts"]);
        }

        private bool useLabels;

        /// <summary>
        /// Activates or deactivates the rendering of the labels of all existing shapes.
        /// </summary>
        public bool UseLabels
        {
            get { return useLabels; }
            set
            {
                useLabels = value;
                foreach (var shape in shapeLayer.Shapes)
                    SetLabelVisible(shape, useLabels);
            }
        }

        #region helper methods
        /// <summary> Adds elements to the map. </summary>
        private void AddElements()
        {
            #region doc:create ShapeLayer
            // create a shape layer
            shapeLayer = new ShapeLayer("DifferentShapes")
            {
                SpatialReferenceId = "EPSG:31467" // set SR to Gauss-Kruger zone 3 (default is WGS84)
            };

            shapeLayer2 = new ShapeLayer("PieCharts");
            #endregion //doc:create ShapeLayer

            #region doc:add layer to map
            // add layers to map
            wpfMap.Layers.Add(shapeLayer);
            wpfMap.Layers.InsertBefore(shapeLayer2, "DifferentShapes");
            #endregion //doc:add layer to map

            #region doc:location variable
            // the location in Gauss-Kruger coordinates
            var location = new Point(3565913, 5935734);
            #endregion //doc:location variable
            
            #region doc:animate shape
            // create a WPF element
            var triangleUp = new TriangleUp {
                Color = Colors.Blue, Stroke = Colors.Red, 
                HorizontalAlignment = HorizontalAlignment.Left,
                ToolTip = "TriangleUp", Width = 32, Height = 32 };

            // set geo location of the element
            triangleUp.SetValue(ShapeCanvas.LocationProperty, location);

            // add some click interaction
            triangleUp.MouseDoubleClick += (o, e) => MessageBox.Show("Hello!");

            var myPointAnimation = new PointAnimation
            {
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                From = location,
                To = new Point(location.X + 960, location.Y + 960)
            };

            // add to layer
            AddToLayer(triangleUp, myPointAnimation);


            #endregion //doc:animate shape

            #region doc:add MapPolygon
            var poly = new MapPolygon
            {
                Points = new PointCollection(new[]
                {
                    (Point) (location - new Point(-100, -100)), (Point) (location - new Point(100, -100)),
                    (Point) (location - new Point(100, 100)), (Point) (location - new Point(-100, 100))
                }),
                Fill = new SolidColorBrush(Colors.Red),
                Opacity = .5,
                MapStrokeThickness = 5,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            Panel.SetZIndex(poly, -1);
            AddToLayer(poly);
            #endregion //doc:add MapPolygon

            #region doc:handle MapPolygon events
            poly.MouseEnter += (o, e) => poly.Fill = new SolidColorBrush(Colors.Green);
            poly.MouseLeave += (o, e) => poly.Fill = new SolidColorBrush(Colors.Red);
            #endregion //doc:handle MapPolygon events

            #region doc:create shapes
            // Create elements
            var triangleDown = CreateElement(Symbols.TriangleDown, Colors.Black, Colors.Red) as TriangleDown;
            var star = CreateElement(Symbols.Star, Colors.Yellow, Colors.Green) as Star;
            var pentagon = CreateElement(Symbols.Pentagon, Colors.Red, Colors.Blue) as Pentagon;
            var hexagon = CreateElement(Symbols.Hexagon, Colors.Orange, Colors.Navy) as Hexagon;
            var diamond = CreateElement(Symbols.Diamond, Colors.DeepPink, Colors.Navy) as Diamond;
            var pyramid = CreateElement(Symbols.Pyramid, Colors.Yellow, Colors.Black) as Pyramid;
            var ball = CreateElement(Symbols.Ball, Colors.Yellow, Colors.Green) as Ball;
            ball.Width = ball.Height = 100; // Varying the default size
            var pin = CreateElement(Symbols.Pin, Colors.Green, Colors.Black) as Pin;
            var cube = CreateElement(Symbols.Cube, Colors.Blue, Colors.Red) as Cube;
            cube.Width = cube.Height = 15; // Varying the default size
            var truck = CreateElement(Symbols.Truck, Colors.Red, Colors.Black) as Truck;
            #endregion //doc:create shapes

            #region doc:set position
            // set position
            triangleDown.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 320, location.Y));
            Panel.SetZIndex(triangleDown, -1);
            star.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 640, location.Y));
            Panel.SetZIndex(star, -1);
            pentagon.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 960, location.Y));
            Panel.SetZIndex(pentagon, -1);
            hexagon.SetValue(ShapeCanvas.LocationProperty, new Point(location.X, location.Y + 320));
            Panel.SetZIndex(hexagon, -1);
            diamond.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 320, location.Y + 320));
            Panel.SetZIndex(diamond, -1);
            pyramid.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 640, location.Y + 320));
            Panel.SetZIndex(pyramid, -1);
            ball.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 960, location.Y + 320));
            Panel.SetZIndex(ball, -1);
            pin.SetValue(ShapeCanvas.LocationProperty, new Point(location.X, location.Y + 640));
            ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
            Panel.SetZIndex(pin, -1);
            cube.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 320, location.Y + 640));
            Panel.SetZIndex(cube, -1);
            truck.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 960, location.Y + 640));
            Panel.SetZIndex(truck, -1);
            #endregion //doc:set position

            AddToLayer(triangleDown);
            AddToLayer(star);
            AddToLayer(pentagon);
            AddToLayer(hexagon);
            AddToLayer(diamond);
            AddToLayer(pyramid);
            AddToLayer(ball);
            AddToLayer(pin);
            AddToLayer(cube);
            AddToLayer(truck);

            
            #region doc:sample anchor
            var pinRightBottom = CreateElement(Symbols.Pin, Colors.Green, Colors.Black) as Pin;
            if (pinRightBottom != null)
            {
                pinRightBottom.SetValue(ShapeCanvas.LocationProperty, new Point(location.X - 320, location.Y));
                pinRightBottom.ToolTip = "Pin with anchor right bottom";
                ShapeCanvas.SetAnchor(pinRightBottom, LocationAnchor.RightBottom);
                Panel.SetZIndex(pinRightBottom, -1);
                AddToLayer(pinRightBottom);
            }

            var ballRightBottom = CreateElement(Symbols.Ball, Colors.Red, Colors.Black) as Ball;
            if (ballRightBottom != null)
            {
                ballRightBottom.Width = 10;
                ballRightBottom.Height = 10;
                ballRightBottom.ToolTip = "Ball with same coordinates like pin";
                ballRightBottom.SetValue(ShapeCanvas.LocationProperty, new Point(location.X - 320, location.Y));
                Panel.SetZIndex(ballRightBottom, -1);
                AddToLayer(ballRightBottom);
            }

            var pinCenter = CreateElement(Symbols.Pin, Colors.Green, Colors.Black) as Pin;
            if (pinCenter != null)
            {
                pinCenter.SetValue(ShapeCanvas.LocationProperty, new Point(location.X - 320, location.Y + 150));
                pinCenter.ToolTip = "Pin with default anchor";
                Panel.SetZIndex(pinCenter, -1);
                AddToLayer(pinCenter);
            }

            var ballCenter = CreateElement(Symbols.Ball, Colors.Red, Colors.Black) as Ball;
            if (ballCenter != null)
            {
                ballCenter.Width = 10;
                ballCenter.Height = 10;
                ballCenter.ToolTip = "Ball with same coordinates like pin";
                ballCenter.SetValue(ShapeCanvas.LocationProperty, new Point(location.X - 320, location.Y + 150));
                Panel.SetZIndex(ballCenter, -1);
                AddToLayer(ballCenter);
            }

            #endregion // doc:sample anchor


            #region doc:sample ScaleProperty
            var starStandard = CreateElement(Symbols.Star, Colors.Green, Colors.Black) as Star;
            if (starStandard != null)
            {
                starStandard.SetValue(ShapeCanvas.LocationProperty, new Point(location.X - 100, location.Y + 960));
                starStandard.Width = starStandard.Height = 40;
                starStandard.ToolTip = "Star with size 40 and scale 1 (standard display)";
                starStandard.SetValue(ShapeCanvas.ScaleProperty, 1.0);
                Panel.SetZIndex(starStandard, -1);
                AddToLayer(starStandard);
            }

            var starBig = CreateElement(Symbols.Star, Colors.Green, Colors.Black) as Star;
            if (starBig != null)
            {
                starBig.SetValue(ShapeCanvas.LocationProperty, new Point(location.X, location.Y + 960));
                starBig.Width = starBig.Height = 40;
                starBig.ToolTip = "Star with size 40 and scale 3 (object bigger and border thicker)";
                starBig.SetValue(ShapeCanvas.ScaleProperty, 3.0);
                Panel.SetZIndex(starBig, -1);
                AddToLayer(starBig);
            }

            #endregion // doc:sample ScaleProperty

            
            #region doc:sample ScaleFactor
            var starConstant = CreateElement(Symbols.Star, Colors.Red, Colors.Black) as Star;
            if (starConstant != null)
            {
                starConstant.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 320, location.Y + 960));
                starConstant.Width = starConstant.Height = 80;
                starConstant.ToolTip = "Star with size 80 and scale factor 0 (constant object size, standard display)";
                ShapeCanvas.SetScaleFactor(starConstant, 0);
                Panel.SetZIndex(starConstant, -1);
                AddToLayer(starConstant);
            }

            var starHalfEnlarged = CreateElement(Symbols.Star, Colors.Yellow, Colors.Black) as Star;
            if (starHalfEnlarged != null)
            {
                starHalfEnlarged.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 370, location.Y + 960));
                starHalfEnlarged.Width = starHalfEnlarged.Height = 80;
                starHalfEnlarged.ToolTip =
                    "Star with size 80 and scale factor 0.5 (object is half enlarged if the map zooms)";
                ShapeCanvas.SetScaleFactor(starHalfEnlarged, 0.5);
                Panel.SetZIndex(starHalfEnlarged, -1);
                AddToLayer(starHalfEnlarged);
            }

            var starEnlarged = CreateElement(Symbols.Star, Colors.Green, Colors.Black) as Star;
            if (starEnlarged != null)
            {
                starEnlarged.SetValue(ShapeCanvas.LocationProperty, new Point(location.X + 420, location.Y + 960));
                starEnlarged.Width = starEnlarged.Height = 80;
                starEnlarged.ToolTip = "Star with size 80 and scale factor 1 (object is enlarged if the map zooms)";
                ShapeCanvas.SetScaleFactor(starEnlarged, 1);
                Panel.SetZIndex(starEnlarged, -1);
                AddToLayer(starEnlarged);
            }

            #endregion // doc:sample ScaleFactor

            // our demo data
            var stores = new List<Store>{
            new Store
            {
                Name = "HH-South",
                Latitude = 53.55,
                Longitude = 10.02,
                Sales = new List<Sale>{
                    new Sale{Type = "Food", Amount= 30}, 
                    new Sale { Type = "Non Food", Amount = 70 }}              
            },
            new Store
            {
                Name = "HH-North",
                Latitude = 53.56,
                Longitude = 10.02,
                Sales = new List<Sale>{
                    new Sale{Type = "Food", Amount = 40},
                    new Sale { Type = "Non Food", Amount = 50 },
                    new Sale { Type = "Pet Food", Amount = 10 }}                
            }};

            foreach (var store in stores)
            {
                // initialize a pie chart for each element
                var chartView = new Chart();
                chartView.BeginInit();

                chartView.Width = 300;
                chartView.Height = 250;
                chartView.Background = new SolidColorBrush(Color.FromArgb(192, 255, 255, 255));
                
                var pieSeries = new PieSeries();
                chartView.Title = store.Name;
                pieSeries.IndependentValuePath = "Type";
                pieSeries.DependentValuePath = "Amount";
                pieSeries.ItemsSource = store.Sales;
                pieSeries.IsSelectionEnabled = true;
                chartView.Series.Add(pieSeries);

                chartView.EndInit();

                // Add to map 
                ShapeCanvas.SetLocation(chartView, new Point(store.Longitude, store.Latitude));
                ShapeCanvas.SetAnchor(chartView, LocationAnchor.Center);
                ShapeCanvas.SetScaleFactor(chartView, .1); // adopt the element to the scale factor
                shapeLayer2.Shapes.Add(chartView);
            }
            
            #region doc:center map to location
            // center map to location
            wpfMap.SetMapLocation(new Point(location.X + 1000, location.Y + 650), 14.7, "EPSG:31467");
            #endregion //doc:center map to location
        }

        public class Sale
        {
            public string Type { get; set; }
            public int Amount { get; set; }
        }

        public class Store
        {
            public string Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public List<Sale> Sales { get; set; }
        }

        #region doc:CreateElement method
        /// <summary> Enumeration containing all possible symbol types for the elements. </summary>
        private enum Symbols
        {
            /// <summary> Symbol type representing a triangle with an upward tip. </summary>
            TriangleUp,
            /// <summary> Symbol type representing a triangle with an downward tip. </summary>
            TriangleDown,
            /// <summary> Symbol type representing a star. </summary>
            Star,
            /// <summary> Symbol type representing a pentagon. </summary>
            Pentagon,
            /// <summary> Symbol type representing a hexagon. </summary>
            Hexagon,
            /// <summary> Symbol type representing a diamond. </summary>
            Diamond,
            /// <summary> Symbol type representing a pyramid. </summary>
            Pyramid,
            /// <summary> Symbol type representing a ball. </summary>
            Ball,
            /// <summary> Symbol type representing a pin. </summary>
            Pin,
            /// <summary> Symbol type representing a cube. </summary>
            Cube,
            /// <summary> Symbol type representing a truck. </summary>
            Truck
        }

        /// <summary> Creates an element and sets the corresponding properties. </summary>
        /// <param name="symbol"> Symbol type of the element. </param>
        /// <param name="color"> Color of the element. </param>
        /// <param name="stroke"> Stroke of the element. </param>
        /// <returns> The created element. </returns>
        private static FrameworkElement CreateElement(Symbols symbol, Color color, Color stroke)
        {
            FrameworkElement frameworkElement = null;
            switch(symbol)
            {
                case Symbols.Ball:         frameworkElement = new Ball { Color = color, Stroke = stroke, ToolTip = "Ball" }; break;
                case Symbols.Cube:         frameworkElement = new Cube { Color = color, Stroke = stroke, ToolTip = "Cube" }; break;
                case Symbols.Diamond:      frameworkElement = new Diamond { Color = color, Stroke = stroke, ToolTip = "Diamond" }; break;
                case Symbols.Hexagon:      frameworkElement = new Hexagon { Color = color, Stroke = stroke, ToolTip = "Hexagon" }; break;
                case Symbols.Pentagon:     frameworkElement = new Pentagon { Color = color, Stroke = stroke, ToolTip = "Pentagon" }; break;
                case Symbols.Pin:          frameworkElement = new Pin { Color = color, ToolTip = "Pin" }; break;
                case Symbols.Pyramid:      frameworkElement = new Pyramid { Color = color, Stroke = stroke, ToolTip = "Pyramid" }; break;
                case Symbols.Star:         frameworkElement = new Star { Color = color, Stroke = stroke, ToolTip = "Star" }; break;
                case Symbols.TriangleDown: frameworkElement = new TriangleDown { Color = color, Stroke = stroke, ToolTip = "TriangleDown" }; break;
                case Symbols.TriangleUp:   frameworkElement = new TriangleUp { Color = color, Stroke = stroke, ToolTip = "TriangleUp" }; break;
                case Symbols.Truck:        frameworkElement = new Truck { Color = color, ToolTip = "Truck" }; break;
            }

            if (frameworkElement == null)
                return null;

            frameworkElement.Height = 32;
            frameworkElement.Width = 32;

            return frameworkElement;
        }
        #endregion //doc:CreateElement method


        /// <summary>
        /// Besides the individual customized symbols, which can be any objects of type FrameworkElement,
        /// additionally labels are added into the shape layer.
        /// </summary>
        /// <param name="frameworkElement">New shape element to insert into the shape layer.</param>
        /// <param name="animation">Optionally an animation can be specified applied to the new shape.</param>
        private void AddToLayer(FrameworkElement frameworkElement, Timeline animation = null)
        {
            #region doc:add to layer

            // Because of a different positioning of labels for objects of type MapPolylineBase compared
            // to other object types, two different implementations are available
            if (frameworkElement is MapPolylineBase)
            {
                shapeLayer.Shapes.Add(frameworkElement);
                // Create a label at the position specified in the Location property of the MapPolyline object.
                var border = CreateLabel("Hello");
                ShapeCanvas.SetLocation(border, ShapeCanvas.GetLocation(frameworkElement));
                shapeLayer.Shapes.Add(border);
            }
            else
            {
                // Arrange symbol and text label in a stack panel
                var stackPanel = new StackPanel();

                stackPanel.Children.Add(frameworkElement);
                stackPanel.Children.Add(CreateLabel("Hello"));

                // The following properties of the new object are transferred to the StackPanel for
                // correct behavior.
                ShapeCanvas.SetLocation(stackPanel, ShapeCanvas.GetLocation(frameworkElement));
                ShapeCanvas.SetAnchor(stackPanel, ShapeCanvas.GetAnchor(frameworkElement));
                ShapeCanvas.SetScale(stackPanel, ShapeCanvas.GetScale(frameworkElement));
                ShapeCanvas.SetScaleFactor(stackPanel, ShapeCanvas.GetScaleFactor(frameworkElement));

                shapeLayer.Shapes.Add(stackPanel);

                // Add the option animation
                if (animation == null) return;

                // Set the animation to target the Center property of the stack panel object
                Storyboard.SetTarget(animation, stackPanel);
                Storyboard.SetTargetProperty(animation, new PropertyPath(ShapeCanvas.LocationProperty));

                // Create a storyboard to apply the animation.
                var sb = new Storyboard();
                sb.Children.Add(animation);
                sb.Begin();
            }
            #endregion //doc:add to layer
        }

        private static FrameworkElement CreateLabel(string text)
        {
          #region doc:create label border
          return new Border
            {
                Visibility = Visibility.Collapsed, // According the default value of UseLabel property, the labels are not visible.
                Background = Brushes.White, 
                BorderBrush = Brushes.Black, 
                BorderThickness = new Thickness(1), 
                Margin = new Thickness(5), 
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock { Text = text, Background = Brushes.White, Foreground = Brushes.Black, Margin = new Thickness(5, 0, 5, 2) }
            };
          #endregion
        }

        private static void SetLabelVisible(FrameworkElement frameworkElement, bool visible)
        {
            if (frameworkElement is Border)
                frameworkElement.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            else if (frameworkElement is StackPanel panel)
                foreach (var f in panel.Children)
                    SetLabelVisible((f as FrameworkElement), visible);
        }
        #endregion
    }
}


