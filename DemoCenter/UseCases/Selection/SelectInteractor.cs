// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;

namespace Ptv.XServer.Demo.UseCases.Selection
{
    /// <summary>
    /// Demonstrates map-interaction (selection)
    /// </summary>
    public class SelectInteractor : WorldCanvas
    {
        public SelectInteractor(MapView map, ObservableCollection<FrameworkElement> shapes) : base(map)
        {
            #region doc:SelectionPreparation

            this.map = map;
            this.shapes = shapes;

            map.MouseLeftButtonUp += map_MouseLeftButtonUp;
            map.MouseLeftButtonDown += map_MouseLeftButtonDown;
            map.MouseMove += map_MouseMove;

            SetZIndex(this, 999999999);
            SelectedElements.CollectionChanged += SelectedElements_CollectionChanged;

            #endregion // doc:SelectionPreparation
        }

        void SelectedElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FrameworkElement shape in e.NewItems)
                {
                    var p1 = (new Point(GetLeft(shape), GetTop(shape)));

                    var r = new Ellipse {Tag = shape};
                    SetLeft(r, p1.X);
                    SetTop(r, p1.Y);
                    r.Width = r.Height = shape.ActualWidth * 2;
                    r.RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale);
                    r.RenderTransformOrigin = new Point(.5, .5);
                    r.Fill = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                    Children.Add(r);
                }
            }

            if (e.OldItems != null)
            {
                var l = Children.Cast<object>().ToList();

                foreach (var selection in l.Cast<FrameworkElement>().Where(selection => e.OldItems.Contains(selection.Tag)))
                    Children.Remove(selection);
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                var l = Children.Cast<object>().ToList();

                foreach (var selection in l.Cast<FrameworkElement>().Where(selection => selection.Tag != null))
                    Children.Remove(selection);
            }
        }

        private readonly ObservableCollection<FrameworkElement> shapes;
        private readonly MapView map;

        private Polygon dragPolygon;

        public ObservableCollection<FrameworkElement> SelectedElements = new ObservableCollection<FrameworkElement>();

        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectMode != SelectMode.Polygon) return;

            polyPoints.Add(CanvasToPtvMercator(e.GetPosition(this)));

            dragPolygon.Points.Clear();
            polyPoints.ForEach(point => dragPolygon.Points.Add(PtvMercatorToCanvas(point)));

            e.Handled = true;
        }

        Point g1;
        SelectMode selectMode = SelectMode.None;
        List<Point> polyPoints;

        void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dragPolygon != null)
                Children.Remove(dragPolygon);

            g1 = CanvasToPtvMercator(e.GetPosition(this));

            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                selectMode = SelectMode.Polygon;
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

        void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                SelectedElements.Clear();

            switch (selectMode)
            {
                case SelectMode.Click:
                    foreach (var shape in from shape in shapes let g2 = e.GetPosition(shape) let x = shape.InputHitTest(g2) where x == shape select shape)
                    {
                        if (SelectedElements.Contains(shape))
                            SelectedElements.Remove(shape);
                        else
                            SelectedElements.Add(shape);
                    }
                    break;
                case SelectMode.Polygon:
                {
                    CanvasToPtvMercator(e.GetPosition(map));
                    // mapControl.PanAndZoom.IsActive = true;
                    polyPoints.Add(polyPoints[0]);

                    foreach (var shape in from shape in shapes let pp1 = (new Point(GetLeft(shape), GetTop(shape))) let x = dragPolygon.InputHitTest(pp1) where x == dragPolygon where !SelectedElements.Contains(shape) select shape)
                        SelectedElements.Add(shape);
                
                    Children.Remove(dragPolygon);
                    dragPolygon = null;
                }
                    break;
            }

            selectMode = SelectMode.None;
        }

        public void Remove()
        {
            map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
            map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
            map.MouseMove -= map_MouseMove;

            map.GeoCanvas.Children.Remove(this);
        }

        public override void Update(UpdateMode updateMode)
        {
            foreach (var e in Children.OfType<Ellipse>())
                e.RenderTransform = new ScaleTransform(map.CurrentScale, map.CurrentScale);
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
