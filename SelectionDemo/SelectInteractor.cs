//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;

namespace SilverMap.UseCases.SharpMap
{
    /// <summary>
    /// Demonstrates map-interaction (selection)
    /// </summary>
    public class SelectInteractor : WorldCanvas
    {
        public SelectInteractor(MapView map, ObservableCollection<FrameworkElement> shapes) : base(map)
        {
            this.map = map;
            this.shapes = shapes;

            map.MouseLeftButtonUp += new MouseButtonEventHandler(map_MouseLeftButtonUp);
            map.MouseLeftButtonDown += new MouseButtonEventHandler(map_MouseLeftButtonDown);
            map.MouseMove += new MouseEventHandler(map_MouseMove);

            Canvas.SetZIndex(this, 999999999);
            SelectedElements.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SelectedElements_CollectionChanged);
        }

        void SelectedElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FrameworkElement shape in e.NewItems)
                {
                    var p1 = (new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape)));

                    var r = new Ellipse();
                    r.Tag = shape;
                    Canvas.SetLeft(r, p1.X);
                    Canvas.SetTop(r, p1.Y);
                    r.Width = shape.ActualWidth;
                    r.Height = shape.ActualHeight;
                    r.RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale);
                    r.RenderTransformOrigin = new Point(.5, .5);
                    r.Fill = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                    this.Children.Add(r);
                }
            }

            if (e.OldItems != null)
            {
                var l = new List<object>();
                foreach (var c in this.Children)
                    l.Add(c);


                foreach (FrameworkElement selection in l)
                {
                    if (e.OldItems.Contains(selection.Tag))
                        this.Children.Remove(selection);

                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                var l = new List<object>();
                foreach (var c in this.Children)
                    l.Add(c);


                foreach (FrameworkElement selection in l)
                {
                    this.Children.Remove(selection);
                }
            }
        }

        private ObservableCollection<FrameworkElement> shapes;
        private MapView map;

        private Polygon dragPolygon;

        public ObservableCollection<FrameworkElement> SelectedElements = new ObservableCollection<FrameworkElement>();

        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectMode == SelectMode.Polygon)
            {
                polyPoints.Add(this.CanvasToPtvMercator(e.GetPosition(this)));

                dragPolygon.Points.Clear();
                foreach (var point in polyPoints)
                {
                    dragPolygon.Points.Add(this.PtvMercatorToCanvas(point));
                }
                e.Handled = true;
            }
        }

        Point g1;
        SelectMode selectMode = SelectMode.None;
        List<Point> polyPoints;

        void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dragPolygon != null)
                this.Children.Remove(dragPolygon);

            g1 = CanvasToPtvMercator(e.GetPosition(this));

            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                selectMode = SelectMode.Polygon;
//                mapControl.PanAndZoom.IsActive = false;
                polyPoints = new List<Point> { g1 };
                dragPolygon = new Polygon
                {
                    Fill = new SolidColorBrush(Color.FromArgb(0x3e, 0xa0, 0x00, 0x00)),
                    Stroke = new SolidColorBrush(Color.FromArgb(0x55, 0xff, 0x00, 0x00))
                };

                this.Children.Add(dragPolygon);
                e.Handled = true;
            }
            else
                selectMode = SelectMode.Click;
        }

        void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectMode == SelectMode.Click)
            {
                foreach(var shape in shapes)
                {
                    Point g2 = e.GetPosition(shape);

                    var x = shape.InputHitTest(g2);
                    if (x == shape)
                    {
                        if (SelectedElements.Contains(shape))
                            SelectedElements.Remove(shape);
                        else
                            SelectedElements.Add(shape);
                    }
                }
            }
            else if (selectMode == SelectMode.Polygon)
            {
                Point g2 = this.CanvasToPtvMercator(e.GetPosition(map));
//                mapControl.PanAndZoom.IsActive = true;
                polyPoints.Add(polyPoints[0]);

                foreach (var shape in shapes)
                {
                    var pp1 =(new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape)));
                    var x = dragPolygon.InputHitTest(pp1);

                    if (x == dragPolygon)
                    {
                        if(!SelectedElements.Contains(shape))
                            SelectedElements.Add(shape);
                    }
                }
                
                this.Children.Remove(dragPolygon);
                dragPolygon = null;
            }

            selectMode = SelectMode.None;
        }

        public void Remove()
        {     
            map.MouseLeftButtonUp -= new MouseButtonEventHandler(map_MouseLeftButtonUp);
            map.MouseLeftButtonDown -= new MouseButtonEventHandler(map_MouseLeftButtonDown);
            map.MouseMove -= new MouseEventHandler(map_MouseMove);

            this.Remove();
        }

        public override void Update(UpdateMode updateMode)
        {
            foreach(var x in this.Children)
            {
                if(x is Ellipse)
                {
                    var e = x as Ellipse;
                    e.RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale);
                }
            }
        }
    }

    public enum SetMode
    {
        Set,
        Add,
        Xor
    }

    public enum SelectMode
    {
        None,
        Click,
        Polygon
    }
}
