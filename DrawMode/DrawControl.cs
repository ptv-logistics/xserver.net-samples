using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;

namespace DrawMode
{
    public class DrawControl : WorldCanvas
    {
        private readonly MapView map; 
        private readonly ScaleTransform adjustTransform;

        public DrawControl(MapView mapView)
            : base(mapView, true)
        {     
            map = mapView;

            map.MouseLeftButtonDown += map_MouseDown;
            map.MouseMove += map_MouseMove;

            adjustTransform = new ScaleTransform(1, 1);
            AdjustTransform();

            SetZIndex(this, 999999999);

            Unloaded += DrawControl_Unloaded;
        }


        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsDrawing)
                return; 
            
            if (_Polygon == null)
                return;

             Point _CurrentMousePoint = e.GetPosition(this);
            _Polygon.Points[_Polygon.Points.Count - 1] = _CurrentMousePoint;
            e.Handled = true;

        }

        private void DrawControl_Unloaded(object sender, RoutedEventArgs e)
        {
            map.MouseDown -= map_MouseDown;
            map.MouseMove -= map_MouseMove;
        }

        /// <summary>
        /// Adjust the transformation for logarithmic scaling
        /// </summary>
        private void AdjustTransform()
        {
            adjustTransform.ScaleX = map.CurrentScale;
            adjustTransform.ScaleY = map.CurrentScale;
        }

        private bool _IsDrawing;
        private Polygon _Polygon;

        private void map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                _IsDrawing = false;
                _Polygon.Points.RemoveAt(_Polygon.Points.Count - 1);

                e.Handled = true;

                return;
            }

            Point _CurrentMousePoint = e.GetPosition(this);

            if (!_IsDrawing)
            {
                _Polygon = new Polygon { Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255))} ;

                Children.Add(_Polygon);
                _Polygon.Points.Add(_CurrentMousePoint);
 
                _IsDrawing = true;
            }

            _Polygon.Points.Add(_CurrentMousePoint);
             Children.Add(CreateSymbol(_CurrentMousePoint, false, _Polygon.Points.Count - 2));

            e.Handled = true;
        }

        private UIElement CreateSymbol(Point p, bool isCenterPoint, int index)
        {
            var ellipse = new Ellipse();

            if (!isCenterPoint)
            {
                ellipse.Fill = new SolidColorBrush(Colors.Yellow);

                ellipse.Height = 15;
                ellipse.Width = 15;
                ellipse.Tag = new PointInfo { Polygon = _Polygon, Index = index };
            }
            else
            {
                ellipse.Fill = new SolidColorBrush(Colors.DarkBlue);

                ellipse.Height = 10;
                ellipse.Width = 10;
            }

            ellipse.StrokeThickness = 1;
            var scaleTransform = new ScaleTransform(1, 1);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(adjustTransform);
            transformGroup.Children.Add(scaleTransform);
            ellipse.RenderTransform = transformGroup;
            ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.StrokeThickness = 2;
            ellipse.MouseEnter += ellipse_MouseEnter;
            ellipse.MouseLeave += ellipse_MouseLeave;
            ellipse.MouseDown += ellipse_MouseDown;
            ellipse.MouseUp += ellipse_MouseUp;
            ellipse.MouseMove += ellipse_MouseMove;

            SetTop(ellipse, p.Y - ellipse.Height / 2);
            SetLeft(ellipse, p.X - ellipse.Width / 2);
            SetZIndex(ellipse, 25);

            return ellipse;
        }

        private void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentEllipse = null;
        }

        private Ellipse currentEllipse;

        private void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_IsDrawing)
                return;

            if (e.LeftButton != MouseButtonState.Pressed) 
                return;

            currentEllipse = (Ellipse)sender;

            e.Handled = true;
        }

        private void ellipse_MouseMove(object sender, MouseEventArgs e)
        {  
            var uiElement = (Ellipse)sender;

            if (currentEllipse != uiElement || !(uiElement.Tag is PointInfo))
                return;

            var pointInfo = (PointInfo)(uiElement.Tag);
            Point tmpPoint = e.GetPosition(this);
            pointInfo.Polygon.Points[pointInfo.Index] = tmpPoint;
            SetTop(uiElement, tmpPoint.Y - uiElement.Height / 2);
            SetLeft(uiElement, tmpPoint.X - uiElement.Width / 2);

            e.Handled = true;
        }

        private void ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            var uiElement = (UIElement)sender;
            var scaleTransform = ((TransformGroup)uiElement.RenderTransform).Children[1] as ScaleTransform;

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(1));
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(1));       
        }

        private void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            var uiElement = (UIElement)sender;
            var scaleTransform = ((TransformGroup)uiElement.RenderTransform).Children[1] as ScaleTransform;

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(2));
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(2));
        }

        /// <summary>Helper to create the zoom double animation for scaling.</summary>
        /// <param name="toValue">Value to animate to.</param>
        /// <returns>Double animation.</returns>
        private static DoubleAnimation CreateZoomAnimation(double toValue)
        {
            var doubleAnimation = new DoubleAnimation(toValue,
                new Duration(TimeSpan.FromMilliseconds( /*checkBox1.IsChecked.Value*/true ? 300 : 0)))
            {
                AccelerationRatio = 0.1,
                DecelerationRatio = 0.9,
                FillBehavior = FillBehavior.HoldEnd
            };

            doubleAnimation.Freeze();
            return doubleAnimation;
        }

        public override void Update(UpdateMode updateMode)
        {
            AdjustTransform();
        }
    }

    public struct PointInfo
    {
        public Polygon Polygon { get; set; }

        public int Index { get; set; }
    }
}
