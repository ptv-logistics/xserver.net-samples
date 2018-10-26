using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
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

            map.MouseLeftButtonUp += map_MouseLeftButtonUp;
            map.MouseLeftButtonDown += map_MouseLeftButtonDown;
            map.MouseMove += map_MouseMove;

            SetZIndex(this, 999999999);
            SelectedElements.CollectionChanged += SelectedElements_CollectionChanged;
        }

        private void SelectedElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FrameworkElement shape in e.NewItems)
                {
                    var p1 = new Point(GetLeft(shape), GetTop(shape));

                    var r = new Ellipse
                    {
                        Tag = shape,
                        Width = shape.ActualWidth,
                        Height = shape.ActualHeight,
                        RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale),
                        RenderTransformOrigin = new Point(.5, .5),
                        Fill = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0))
                    };
                    SetLeft(r, p1.X);
                    SetTop(r, p1.Y);
                    Children.Add(r);
                }
            }

            if (e.OldItems != null)
            {
                var l = new List<object>();
                foreach (var c in Children)
                    l.Add(c);


                foreach (FrameworkElement selection in l)
                {
                    if (e.OldItems.Contains(selection.Tag))
                        Children.Remove(selection);

                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                var l = new List<object>();
                foreach (var c in Children)
                    l.Add(c);


                foreach (FrameworkElement selection in l)
                {
                    Children.Remove(selection);
                }
            }
        }

        private readonly ObservableCollection<FrameworkElement> shapes;
        private readonly MapView map;

        private Polygon dragPolygon;

        public ObservableCollection<FrameworkElement> SelectedElements = new ObservableCollection<FrameworkElement>();

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            switch (selectMode)
            {
                case SelectMode.Polygon:
                {
                    polyPoints.Add(CanvasToPtvMercator(e.GetPosition(this)));

                    dragPolygon.Points.Clear();
                    foreach (var point in polyPoints)
                    {
                        dragPolygon.Points.Add(PtvMercatorToCanvas(point));
                    }
                    e.Handled = true;
                    break;
                }
                case SelectMode.Rectangle:
                {
                    dragPolygon.Points.Clear();
                    foreach (var dummy in polyPoints)
                    {
                        var g2 = CanvasToPtvMercator(e.GetPosition(this));

                        dragPolygon.Points.Add(PtvMercatorToCanvas(new Point(g1.X, g1.Y)));
                        dragPolygon.Points.Add(PtvMercatorToCanvas(new Point(g2.X, g1.Y)));
                        dragPolygon.Points.Add(PtvMercatorToCanvas(new Point(g2.X, g2.Y)));
                        dragPolygon.Points.Add(PtvMercatorToCanvas(new Point(g1.X, g2.Y)));
                    }
                    e.Handled = true;
                    break;
                }
            }
        }

        private Point g1;
        private SelectMode selectMode = SelectMode.None;
        private List<Point> polyPoints;

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dragPolygon != null)
                Children.Remove(dragPolygon);

            g1 = CanvasToPtvMercator(e.GetPosition(this));

            if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) != 0)
            {
                selectMode = (Keyboard.Modifiers & ModifierKeys.Alt) != 0 ? SelectMode.Polygon : SelectMode.Rectangle;
//                mapControl.PanAndZoom.IsActive = false;
                polyPoints = new List<Point> { g1 };
                dragPolygon = new Polygon
                {
                    Fill = new SolidColorBrush(Color.FromArgb(0x3e, 0xa0, 0x00, 0x00)),
                    Stroke = new SolidColorBrush(Color.FromArgb(0x55, 0xff, 0x00, 0x00))
                };

                Children.Add(dragPolygon);
                e.Handled = true;
            }
            else
                selectMode = SelectMode.Click;
        }

        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (selectMode)
            {
                case SelectMode.Click:
                {
                    foreach(var shape in shapes)
                    {
                        var x = shape.InputHitTest(e.GetPosition(shape));
                        if (x == shape)
                        {
                            if (SelectedElements.Contains(shape))
                                SelectedElements.Remove(shape);
                            else
                                SelectedElements.Add(shape);
                        }
                    }

                    break;
                }
                case SelectMode.Polygon:
                case SelectMode.Rectangle:
                {
//                mapControl.PanAndZoom.IsActive = true;
                    polyPoints.Add(polyPoints[0]);

                    foreach (var shape in shapes)
                    {
                        var pp1 = new Point(GetLeft(shape), GetTop(shape));
                        var x = dragPolygon.InputHitTest(pp1);

                        if (x == dragPolygon)
                        {
                            if(!SelectedElements.Contains(shape))
                                SelectedElements.Add(shape);
                        }
                    }
                
                    Children.Remove(dragPolygon);
                    dragPolygon = null;
                    break;
                }
            }

            selectMode = SelectMode.None;
        }

        public override void Update(UpdateMode updateMode)
        {
            foreach(var x in Children)
            {
                if (!(x is Ellipse)) continue;

                var e = x as Ellipse;
                e.RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale);
            }
        }
    }

    public enum SelectMode
    {
        None,
        Click,
        Polygon,
        Rectangle
    }
}
