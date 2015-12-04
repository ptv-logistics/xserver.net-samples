//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using XServerNetExt.XRouteServiceReference;

namespace Ptv.XServer.Controls.Routing
{
    /// <summary>
    /// Provides basic styling information.
    /// </summary>
    public class ShapeStyle
    {
        /// <summary>
        /// Color 
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        public int Size { get; set; }
    }
    
    /// <summary>
    /// The routing way point base class.
    /// </summary>
    public abstract class WayPoint : Canvas, IDisposable
    {
        /// <summary>
        /// assigned route layer
        /// </summary>
        protected RouteLayer layer;

        /// <summary>
        /// Provides a global event for right mouse button clicks on way points.
        /// </summary>
        public static MouseButtonEventHandler WayPointMouseRightButtonDown;

        /// <summary>
        /// Creates the way point
        /// </summary>
        /// <param name="layer">Assigned route layer</param>
        /// <param name="p">Position of the way point (world coordinates)</param>
        /// <param name="style">Style of the way point</param>
        protected WayPoint(RouteLayer layer, System.Windows.Point p, ShapeStyle style)
        {
            this.layer = layer;

            Width = Height = style.Size;

            // set initial position and show polyline 
            // do not use Point property to avoid LocationChanged event in initialization
            ShapeCanvas.SetLocation(this, p);
            
            // drag and drop handlers
            InProcessDragDropBehavior.AddDragHandler(this, Drag);
            InProcessDragDropBehavior.AddDragMoveHandler(this, DragMove);
            InProcessDragDropBehavior.SetEnableDragDrop(this, true);

            // handler for context menu
            MouseRightButtonDown += WayPoint_MouseRightButtonDown;
        }

        /// <summary>
        /// Reads or writes the z index of the way point.
        /// </summary>
        public virtual int ZIndex
        {
            get { return GetZIndex(this); }
            set { SetZIndex(this, value); }
        }
        
        /// <summary>
        /// Reads of writes the way point location.
        /// </summary>
        public virtual System.Windows.Point Point
        {
            get { return ShapeCanvas.GetLocation(this); }
            set
            {
                System.Windows.Point previousPoint = ShapeCanvas.GetLocation(this);
                if (previousPoint == value)
                    return;

                ShapeCanvas.SetLocation(this, value); 
                OnLocationChanged();
            }
        }

        /// <summary>
        /// Handles location changes
        /// </summary>
        public virtual void OnLocationChanged()
        {
            // pass on to layer
            layer.WayPointMoved(this);
        }

        /// <summary>
        /// Disposes the way point.
        /// </summary>
        public virtual void Dispose()
        {
            WayPointMouseRightButtonDown -= WayPoint_MouseRightButtonDown;

            InProcessDragDropBehavior.SetEnableDragDrop(this, false);
            InProcessDragDropBehavior.RemoveDragHandler(this, Drag);
            InProcessDragDropBehavior.RemoveDragMoveHandler(this, DragMove);

            layer.Shapes.Remove(this);
        }

        /// <summary>
        /// Handles right mouse button clicks.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void WayPoint_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WayPointMouseRightButtonDown == null) return;

            WayPointMouseRightButtonDown(this, e);
            e.Handled = true;
        }

        /// <summary>
        /// Provides the way point description to routing. 
        /// </summary>
        public abstract WaypointDesc Waypoint
        {
            get;
        }

        /// <summary>
        /// Drag handler. Element is being dragged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void DragMove(Object sender, DragDropEventArgs args)
        {
            var o = args.AnchorPosition - args.HitPosition;
            var p = layer.Map.PointFromScreen(args.Position);
            
            Point = layer.ToWorld(p + o);
        }

        /// <summary>
        /// Drag handler. Triggered when way point was dragged by used.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event arguments</param>
        protected virtual void Drag(Object sender, DragDropEventArgs args)
        {
            // nothing to do here
        }

        /// <summary>
        /// Initiates a Drag&amp;Drop operation on this way point.
        /// </summary>
        /// <param name="dragPosition"></param>
        /// <param name="position"></param>
        public void BeginDrag(System.Windows.Point dragPosition, System.Windows.Point? position = null)
        {
            InProcessDragDropBehavior.Attach(this, dragPosition, position);
        }
    }

    /// <summary>
    /// A route stop.
    /// </summary>
    public class Stop : WayPoint
    {
        /// <summary>
        /// Creates a stop.
        /// </summary>
        /// <param name="layer">Assigned route layer</param>
        /// <param name="p">Position of the way point (world coordinates)</param>
        /// <param name="style">Style of the way point</param>
        public Stop(RouteLayer layer, System.Windows.Point p, ShapeStyle style)
            : base(layer, p, style)
        {
            // add a pin
            Pin = new Pin {Color = style.Color, Width = style.Size, Height = style.Size};
            Children.Add(Pin);

            // overlay pin with a text block
            Children.Add(new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = style.Size * 0.3125,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(800),
                RenderTransform = new RotateTransform(-45),
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 0.125 * style.Size,
                    Direction = 45,
                    Color = Colors.White,
                    Opacity = 0.5,
                    BlurRadius = 0.125 * style.Size
                }
            });

            // position subordinate elements
            SetLeft(Children[1], 0.25 * style.Size);
            SetTop(Children[1], 0.4375 * style.Size);

            ShapeCanvas.SetAnchor(this, LocationAnchor.RightBottom);
            layer.Shapes.Add(this);
        }

        /// <summary>
        /// Reads or writes the label of the stop.
        /// </summary>
        public String Label
        {
            get { return (Children[1] as TextBlock).Text; }

            set 
            { 
                if (value == null || value.Length != 1) 
                    throw new ArgumentException("Label is invalid or exceeds maximum length");  

                (Children[1] as TextBlock).Text = value; 
            }
        }

        /// <summary>
        /// Getting the pin shape object, which represents this way point.
        /// </summary>
        public Pin Pin { get; private set; }

        private MapPolyline linkPolyline;

        /// <summary>
        /// Reads or writes the location, to which this Stop is linked to the road network.
        /// </summary>
        public virtual System.Windows.Point LinkPoint
        {
            set
            {
                if (linkPolyline != null)
                    layer.Shapes.Remove(linkPolyline);

                var points = new[] { Point, value };
                linkPolyline = new MapPolyline
                {
                    MapStrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection(new[] {2.0, 2.0}),
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Points = new PointCollection(points),
                    ToolTip = null
                };
                layer.Shapes.Add(linkPolyline);
            }
        }

        /// <inheritdoc/>
        public override WaypointDesc Waypoint
        {
            get { return Point.ToWaypoint(); }
        }
    }

    /// <summary>
    /// A via way point.
    /// </summary>
    public class Via : WayPoint
    {
        /// <summary>
        /// Creates a via way point.
        /// </summary>
        /// <param name="layer">Assigned route layer</param>
        /// <param name="p">Position of the way point (world coordinates)</param>
        /// <param name="style">Style of the way point</param>
        public Via(RouteLayer layer, System.Windows.Point p, ShapeStyle style)
            : base(layer, p, style)
        {
            Children.Add(new Ball
            {
                Color = style.Color,
                Stroke = Colors.Black,
                Width = style.Size,
                Height = style.Size
            });

            // must set a tool tip for the ToolTipOpening event to be raised.
            ToolTip = "<null>";
            base.ToolTipOpening += ToolTipOpening;

            ShapeCanvas.SetAnchor(this, LocationAnchor.Center);
            layer.Shapes.Add(this);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.ToolTipOpening -= ToolTipOpening;
            base.Dispose();
        }

        /// <summary>
        /// Handles the event that occurs when a tool tip is about to be displayed.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguements</param>
        protected new void ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            // overwrite tool tip with that one of the route
            ToolTip = Route.ToolTip;
        }

        /// <summary>
        /// Gets or set the route to which this way point is assigned to.
        /// </summary>
        public Route Route { get; set; }

        /// <summary>
        /// Gets or sets the last known polygon index.
        /// </summary>
        public int PolyIndex { get; set; }

        /// <inheritdoc/>
        public override WaypointDesc Waypoint
        {
            get { return Point.ToWaypoint(ViaTypeEnum.VIA); }
        }
    }

    /// <summary>
    /// A temporary way point for defining a via way point by means of Drag&amp;Drop.
    /// </summary>
    public class TemporaryVia : Via
    {
        /// <summary>
        /// Via way point instance.
        /// </summary>
        private static TemporaryVia instance;

        /// <summary>
        /// Creates the temporary via way point.
        /// </summary>
        /// <param name="layer">Assigned Routing layer</param>
        /// <param name="p">Position of the via way point (word coordinates)</param>
        /// <param name="style">Style of the way point</param>
        private TemporaryVia(RouteLayer layer, System.Windows.Point p, ShapeStyle style)
            : base(layer, p, style)
        {
        }

        /// <summary>
        /// Handles location changes
        /// </summary>
        public override void OnLocationChanged()
        {
            // override default, do nothing
        }

        /// <summary>
        /// Drag handler. Triggered when a way point was dragged by user.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event arguments</param>
        protected override void Drag(Object sender, DragDropEventArgs args)
        {
            // cancel drag and drop operation, replace a temporary way point with a 
            // real via way point and attach drag and drop operation to that way point

            Route.BeginDragVia(RouteLayer.VIA_STYLE, args.DragPosition, args.Position);

            args.Cancel = true;
            Hide();
        }

        /// <summary>
        /// Displays the temporary via way point
        /// </summary>
        /// <param name="layer">Assigned route layer</param>
        /// <param name="route">Assigned route</param>
        /// <param name="p">Position of the via way point (word coordinates)</param>
        public static void Show(RouteLayer layer, Route route, System.Windows.Point p)
        {
            if (instance == null)
                instance = new TemporaryVia(layer, p, RouteLayer.TMPVIA_STYLE) { ZIndex = 999 };

            instance.Route = route;
            instance.Point = p;
        }

        /// <summary>
        /// Hides the temporary via way point.
        /// </summary>
        public static void Hide()
        {
            if (instance != null)
                instance.Dispose();

            instance = null;
        }
    }
}
