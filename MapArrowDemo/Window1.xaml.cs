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
using Petzold.Media2D;
using System.Windows.Media.Animation;

namespace MapArrowDemo
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

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            AddArrows(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Chales Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapSape
        /// </summary>
        /// <param name="layer"></param>
        public void AddArrows(ShapeLayer layer)
        {
            // ArrowLine with animated arrow properties.
            ArrowLine aline1 = new ArrowLine();
            aline1.Stroke = Brushes.Red;
            aline1.MapStrokeThickness = 30; // the adaptive stroke thickness
            aline1.ScaleFactor = .25; // the scale factor [0..1]. 0: Don't scale (thickness = pixels); 1: scale linear (thickness = mercator units)
            aline1.ArrowLength = 3; // the arrow length is relative to the arrow stroke thickness
            aline1.X1 = 8.100;
            aline1.Y1 = 49.400;
            aline1.X2 = 8.400;
            aline1.Y2 = 49.100;
            layer.Shapes.Add(aline1);

            DoubleAnimation animaDouble1 = new DoubleAnimation(10, 50, new Duration(new TimeSpan(0, 0, 5)));
            animaDouble1.AutoReverse = true;
            animaDouble1.RepeatBehavior = RepeatBehavior.Forever;
            aline1.BeginAnimation(ArrowLine.ArrowAngleProperty, animaDouble1);

            DoubleAnimation animaDouble2 = new DoubleAnimation(1, 20, new Duration(new TimeSpan(0, 0, 5)));
            animaDouble2.AutoReverse = true;
            animaDouble2.RepeatBehavior = RepeatBehavior.Forever;
            aline1.BeginAnimation(ArrowLine.ArrowLengthProperty, animaDouble2);

            // ArrowLine with animated point properties.
            ArrowLine aline2 = new ArrowLine();
            aline2.ArrowEnds = ArrowEnds.Both;

            aline2.Stroke = Brushes.Blue;
            aline2.MapStrokeThickness = 30;
            aline2.ScaleFactor = .25;
            aline1.ArrowLength = 3;
            aline2.X1 = 8.100;
            aline2.Y1 = 49.100;
            aline2.X2 = 8.200;
            aline2.Y2 = 49.400;
            layer.Shapes.Add(aline2);

            AnimationTimeline animaDouble3 = new DoubleAnimation(8.100, 8.400, new Duration(new TimeSpan(0, 0, 5)));
            animaDouble3.AutoReverse = true;
            animaDouble3.RepeatBehavior = RepeatBehavior.Forever;
            aline2.BeginAnimation(ArrowLine.X1Property, animaDouble3);

            AnimationTimeline animaDouble4 = new DoubleAnimation(49.400, 49.100, new Duration(new TimeSpan(0, 0, 5)));
            animaDouble4.AutoReverse = true;
            animaDouble4.RepeatBehavior = RepeatBehavior.Forever;
            aline2.BeginAnimation(ArrowLine.Y2Property, animaDouble4);

            // ArrowPolyline rotated.            
            ArrowPolyline apoly = new ArrowPolyline();
            apoly.ArrowEnds = ArrowEnds.Both;
            apoly.Stroke = Brushes.Green;
            apoly.MapStrokeThickness = 30;
            apoly.ScaleFactor = .25;
            aline1.ArrowLength = 3;

            apoly.Points.Add(new Point(8.25, 49.25));
            apoly.Points.Add(new Point(8.125, 49.25));
            apoly.Points.Add(new Point(8.125, 49.125));
            apoly.Points.Add(new Point(8.25, 49.125));

            layer.Shapes.Add(apoly);

            // the rotation center of the canvas needs to be calculated for a rotate transform
            // only works after the shape has been added to the canvas.
            var rotateCenter = apoly.GeoTransform(new Point(8, 49));
            RotateTransform xform = new RotateTransform(0, rotateCenter.X, rotateCenter.Y);
            apoly.RenderTransform = xform;
            AnimationTimeline animaDouble5 = new DoubleAnimation(0, 360, new Duration(new TimeSpan(0, 0, 10)));
            animaDouble5.RepeatBehavior = RepeatBehavior.Forever;
            xform.BeginAnimation(RotateTransform.AngleProperty, animaDouble5);
        }
    }
}
