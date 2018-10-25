using System;
using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Petzold.Media2D;
using System.Windows.Media.Animation;

namespace MapArrowDemo
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

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            AddArrows(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Charles Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapShape
        /// </summary>
        /// <param name="layer"></param>
        public void AddArrows(ShapeLayer layer)
        {
            // ArrowLine with animated arrow properties.
            var arrowLine1 = new ArrowLine
            {
                Stroke = Brushes.Red,
                MapStrokeThickness = 30,
                ScaleFactor = .25,
                ArrowLength = 3,
                X1 = 8.100,
                Y1 = 49.400,
                X2 = 8.400,
                Y2 = 49.100
            };
            // the adaptive stroke thickness
            // the scale factor [0..1]. 0: Don't scale (thickness = pixels); 1: scale linear (thickness = mercator units)
            // the arrow length is relative to the arrow stroke thickness
            layer.Shapes.Add(arrowLine1);

            var doubleAnimation1 = new DoubleAnimation(10, 50, new Duration(new TimeSpan(0, 0, 5)))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            arrowLine1.BeginAnimation(ArrowLineBase.ArrowAngleProperty, doubleAnimation1);

            var doubleAnimation2 = new DoubleAnimation(1, 20, new Duration(new TimeSpan(0, 0, 5)))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            arrowLine1.BeginAnimation(ArrowLineBase.ArrowLengthProperty, doubleAnimation2);

            // ArrowLine with animated point properties.
            var arrowLine2 = new ArrowLine
            {
                ArrowEnds = ArrowEnds.Both,
                Stroke = Brushes.Blue,
                MapStrokeThickness = 30,
                ScaleFactor = .25
            };

            arrowLine1.ArrowLength = 3;
            arrowLine2.X1 = 8.100;
            arrowLine2.Y1 = 49.100;
            arrowLine2.X2 = 8.200;
            arrowLine2.Y2 = 49.400;
            layer.Shapes.Add(arrowLine2);

            AnimationTimeline doubleAnimation3 = new DoubleAnimation(8.100, 8.400, new Duration(new TimeSpan(0, 0, 5)));
            doubleAnimation3.AutoReverse = true;
            doubleAnimation3.RepeatBehavior = RepeatBehavior.Forever;
            arrowLine2.BeginAnimation(ArrowLine.X1Property, doubleAnimation3);

            AnimationTimeline doubleAnimation4 = new DoubleAnimation(49.400, 49.100, new Duration(new TimeSpan(0, 0, 5)));
            doubleAnimation4.AutoReverse = true;
            doubleAnimation4.RepeatBehavior = RepeatBehavior.Forever;
            arrowLine2.BeginAnimation(ArrowLine.Y2Property, doubleAnimation4);

            // ArrowPolyline rotated.            
            var arrowPolyline = new ArrowPolyline
            {
                ArrowEnds = ArrowEnds.Both,
                Stroke = Brushes.Green,
                MapStrokeThickness = 30,
                ScaleFactor = .25
            };
            arrowLine1.ArrowLength = 3;

            arrowPolyline.Points.Add(new Point(8.25, 49.25));
            arrowPolyline.Points.Add(new Point(8.125, 49.25));
            arrowPolyline.Points.Add(new Point(8.125, 49.125));
            arrowPolyline.Points.Add(new Point(8.25, 49.125));

            layer.Shapes.Add(arrowPolyline);

            // the rotation center of the canvas needs to be calculated for a rotate transform
            // only works after the shape has been added to the canvas.
            var rotateCenter = arrowPolyline.GeoTransform(new Point(8, 49));
            var rotateTransform = new RotateTransform(0, rotateCenter.X, rotateCenter.Y);
            arrowPolyline.RenderTransform = rotateTransform;
            AnimationTimeline animaDouble5 = new DoubleAnimation(0, 360, new Duration(new TimeSpan(0, 0, 10)));
            animaDouble5.RepeatBehavior = RepeatBehavior.Forever;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animaDouble5);
        }
    }
}
