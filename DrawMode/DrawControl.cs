//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
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
        private MapView map; 
        private ScaleTransform adjustTransform;

        public DrawControl(MapView mapView)
            : base(mapView, true)
        {     
            this.map = mapView;

            map.MouseLeftButtonDown += map_MouseDown;
            map.MouseMove += map_MouseMove;

            adjustTransform = new ScaleTransform(1, 1);
            AdjustTransform();

            Canvas.SetZIndex(this, 999999999);

            this.Unloaded += new RoutedEventHandler(DrawControl_Unloaded);
        }


        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsDrawing)
                return; 
            
            if (_Polygon == null)
                return;

             Point _CurrentMousePoint = e.GetPosition(this);
            _Polygon.Points[_Polygon.Points.Count - 1] = _CurrentMousePoint;
            e.Handled = true;

        }

        void DrawControl_Unloaded(object sender, RoutedEventArgs e)
        {
            map.MouseDown -= new MouseButtonEventHandler(map_MouseDown);
            map.MouseMove -= new MouseEventHandler(map_MouseMove);
        }

        /// <summary>
        /// Adjust the transformation for logarithmic scaling
        /// </summary>
        void AdjustTransform()
        {
            adjustTransform.ScaleX = map.CurrentScale;
            adjustTransform.ScaleY = map.CurrentScale;
        }

        private bool _IsDrawing;
        private Polygon _Polygon;
        void map_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                _Polygon = new Polygon();
                _Polygon.Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

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
            Ellipse ellipse = new Ellipse();

            if (!isCenterPoint)
            {
                ellipse.Fill = new SolidColorBrush(Colors.Yellow); ;

                ellipse.Height = 15;
                ellipse.Width = 15;
                ellipse.Tag = new PointInfo { Polygon = _Polygon, Index = index };
            }
            else
            {
                ellipse.Fill = new SolidColorBrush(Colors.DarkBlue); ;

                ellipse.Height = 10;
                ellipse.Width = 10;
            }

            ellipse.StrokeThickness = 1;
            ScaleTransform sc = new ScaleTransform(1, 1);
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(adjustTransform);
            tg.Children.Add(sc);
            ellipse.RenderTransform = tg;
            ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.StrokeThickness = 2;
            ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            ellipse.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            ellipse.MouseDown += new MouseButtonEventHandler(ellipse_MouseDown);
            ellipse.MouseUp += new MouseButtonEventHandler(ellipse_MouseUp);
            ellipse.MouseMove += new MouseEventHandler(ellipse_MouseMove);

            Canvas.SetTop(ellipse, p.Y - ellipse.Height / 2);
            Canvas.SetLeft(ellipse, p.X - ellipse.Width / 2);
            Canvas.SetZIndex(ellipse, 25);

            return ellipse;
        }

        void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentEllipse = null;
        }

        Ellipse currentEllipse;

        void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._IsDrawing)
                return;

            if (e.LeftButton != MouseButtonState.Pressed) 
                return;

            currentEllipse = ((Ellipse)sender);

            e.Handled = true;
        }

        void ellipse_MouseMove(object sender, MouseEventArgs e)
        {  
            Ellipse uiElement = ((Ellipse)sender);

            if (currentEllipse != uiElement)
                return;

            if(uiElement.Tag is PointInfo)
            {

                PointInfo p = (PointInfo)(uiElement.Tag);
                Point tmpP = e.GetPosition(this);
                p.Polygon.Points[p.Index] = tmpP;
                Canvas.SetTop(uiElement, tmpP.Y - uiElement.Height / 2);
                Canvas.SetLeft(uiElement, tmpP.X - uiElement.Width / 2);

                e.Handled = true;
            }
        }

        void ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = ((UIElement)sender);
            ScaleTransform sc = ((TransformGroup)uiElement.RenderTransform).Children[1] as ScaleTransform;

            sc.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(1));
            sc.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(1));       
        }

        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = ((UIElement)sender);
            ScaleTransform sc = ((TransformGroup)uiElement.RenderTransform).Children[1] as ScaleTransform;

            sc.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(2));
            sc.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(2));
        }

        /// <summary>Helper to create the zoom double animation for scaling.</summary>
        /// <param name="toValue">Value to animate to.</param>
        /// <returns>Double animation.</returns>
        private DoubleAnimation CreateZoomAnimation(double toValue)
        {
            var da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(/*checkBox1.IsChecked.Value*/true ? 300 : 0)));
            da.AccelerationRatio = 0.1;
            da.DecelerationRatio = 0.9;
            da.FillBehavior = FillBehavior.HoldEnd;

            da.Freeze();
            return da;
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
