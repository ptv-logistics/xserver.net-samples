// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows;
using System.Windows.Input;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows.Controls;

namespace Ptv.XServer.Controls.Routing
{
    /// <summary>
    /// Argument of a Drag&amp;Drop event.
    /// </summary>
    public class DragDropEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Creates an argument object.
        /// </summary>
        /// <param name="state">Drag&amp;Drop state</param>
        /// <param name="ev">Event to create argument for</param>
        public DragDropEventArgs(InProcessDragDropState state, RoutedEvent ev) : base(ev)
        {
            Cancel = false;
            HitPosition = state.HitPosition;
            DragPosition = state.DragPosition;
            Position = state.Position;
            AnchorPosition = state.AnchorPosition;
        }

        /// <summary>
        /// Reads or writes the cancel flag. 
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Reads the hit position (relative to the element).
        /// </summary>
        public Point HitPosition { get; private set; }

        /// <summary>
        /// Reads the Drag position (screen coordinates).
        /// </summary>
        public Point DragPosition { get; private set; }

        /// <summary>
        /// Reads the current position (screen coordinates)
        /// </summary>
        public Point Position { get; private set; }

        /// <summary>
        /// Reads the anchor position of the dragged element (relative to the element)
        /// </summary>
        public Point AnchorPosition { get; private set; }
    }


    /// <summary>
    /// Drag&amp;Drop event delegate
    /// </summary>
    /// <param name="sender">Event source</param>
    /// <param name="args">Event arguments</param>
    public delegate void DragDropEventHandler(object sender, DragDropEventArgs args);

    /// <summary>
    /// Encapsulates a Drag&amp;Drop state.
    /// </summary>
    public class InProcessDragDropState
    {
        /// <summary>
        /// Backing field for Capturing property.
        /// </summary>
        private bool capturing;

        /// <summary>
        /// Drag mode flag
        /// </summary>
        public bool Dragging { get; set; }
        
        /// <summary>
        /// Z index backup field
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Dragged element
        /// </summary>
        public FrameworkElement Target { get; set; }

        /// <summary>
        /// Capturing mode flag
        /// </summary>
        public bool Capturing
        {
            get { return capturing; }

            set
            {
                // set / reset mouse capture mode depending on value
                capturing = value;
                if (capturing)
                    Mouse.Capture(Target);
                else if (Equals(Mouse.Captured, Target))
                    Mouse.Capture(null);
            }
        }

        /// <summary>
        /// Reads the hit position (relative to the element).
        /// </summary>
        public Point HitPosition { get; set; }

        /// <summary>
        /// Reads the Drag position (screen coordinates).
        /// </summary>
        public Point DragPosition { get; set; }

        /// <summary>
        /// Reads the current position (screen coordinates)
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Reads the anchor position of the dragged element (relative to the element)
        /// </summary>
        public Point AnchorPosition
        {
            get
            {
                double[] LocationAnchor_fx = { 0.5, 0.0, 1.0, 1.0, 0.0 };
                double[] LocationAnchor_fy = { 0.5, 0.0, 0.0, 1.0, 1.0 };

                var fidx = (int)ShapeCanvas.GetAnchor(Target);

                return new Point(Target.Width * LocationAnchor_fx[fidx], Target.Height * LocationAnchor_fy[fidx]);
            }
        }
    }

    /// <summary>
    /// "Attached application-local" Drag&amp;Drop behavior for FrameworkElements. 
    /// In contrast to conventional Drag&amp;Drop, event handlers are to be registered on the element being dragged.
    /// </summary>
    internal class InProcessDragDropBehavior : InProcessDragDropState
    {
        /// <summary> Definition of Drag event. </summary>
        public static readonly RoutedEvent Drag = EventManager.RegisterRoutedEvent("Drag", RoutingStrategy.Direct, typeof(DragDropEventHandler), typeof(FrameworkElement));

        /// <summary> Definition of Drag move event. </summary>
        public static readonly RoutedEvent DragMove = EventManager.RegisterRoutedEvent("DragMove", RoutingStrategy.Direct, typeof(DragDropEventHandler), typeof(FrameworkElement));

        /// <summary> Definition of Drop event. </summary>
        public static readonly RoutedEvent Drop = EventManager.RegisterRoutedEvent("Drop", RoutingStrategy.Direct, typeof(DragDropEventHandler), typeof(FrameworkElement));

        /// <summary> Registers a Drag handler with an element. </summary>
        /// <param name="d">Element to register the handler with.</param>
        /// <param name="handler">Handler</param>
        public static void AddDragHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.AddHandler(Drag, handler); }

        /// <summary> Unregisters a Drag handler. </summary>
        /// <param name="d">Element from which to unregister the handler.</param>
        /// <param name="handler">Handler</param>
        public static void RemoveDragHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.RemoveHandler(Drag, handler); }

        /// <summary> Registers a Drag move handler with an element. </summary>
        /// <param name="d">Element to register the handler with.</param>
        /// <param name="handler">Handler</param>
        public static void AddDragMoveHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.AddHandler(DragMove, handler); }

        /// <summary> Unregisters a Drag move handler. </summary>
        /// <param name="d">Element from which to unregister the handler.</param>
        /// <param name="handler">Handler</param>
        public static void RemoveDragMoveHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.RemoveHandler(DragMove, handler); }

        /// <summary> Registers a drop handler with an element. </summary>
        /// <param name="d">Element to register the handler with.</param>
        /// <param name="handler">Handler</param>
        public static void AddDropHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.AddHandler(Drop, handler); }

        /// <summary> Unregisters a drop handler. </summary>
        /// <param name="d">Element from which to unregister the handler.</param>
        /// <param name="handler">Handler</param>
        public static void RemoveDropHandler(DependencyObject d, DragDropEventHandler handler)
            { (d as FrameworkElement)?.RemoveHandler(Drop, handler); }

        /// <summary> Attached property, checks if Drag&amp;Drop is enabled for the specified element. </summary>
        /// <param name="e">Element to check.</param>
        /// <returns>True, if Drag&amp;Drop is enabled</returns>
        public static bool GetEnableDragDrop(FrameworkElement e)
            { return (bool)e.GetValue(EnableDragDrop); }

        /// <summary> Attached property, enables or disables Drag&amp;Drop for the specified element. </summary>
        /// <param name="e">Element to set the property for.</param>
        /// <param name="enable">True, if Drag&amp;Drop is to be enabled</param>
        public static void SetEnableDragDrop(FrameworkElement e, bool enable)
            { e.SetValue(EnableDragDrop, enable); }

        /// <summary> Definition of the attached "enable property". </summary>
        public static readonly DependencyProperty EnableDragDrop = DependencyProperty.RegisterAttached(
            "EnableDragDrop", typeof(bool), typeof(InProcessDragDropBehavior), new UIPropertyMetadata(false, OnEnableDragDrop));

        /// <summary> Handles EnableDragDrop changes. </summary>
        /// <param name="depObj">Object for which the value has changed.</param>
        /// <param name="e">Event arguments</param>
        private static void OnEnableDragDrop(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if (!(depObj is FrameworkElement elem)) return;

            if (e.NewValue is bool)
            {
                // enable drag and drop - attach necessary handlers
                elem.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
                elem.PreviewMouseMove += PreviewMouseMove;
                elem.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUp;
            }
            else
            {
                // disable drag and drop - detach necessary handlers
                elem.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
                elem.PreviewMouseMove -= PreviewMouseMove;
                elem.PreviewMouseLeftButtonUp -= PreviewMouseLeftButtonUp;

                // reset state
                elem.SetValue(DragDropState, null);
            }
        }

        /// <summary> Definition of a private attached property storing the Drag&amp;Drop state. </summary>
        private static readonly DependencyProperty DragDropState = DependencyProperty.RegisterAttached(
            "DragDropState", typeof(InProcessDragDropBehavior), typeof(InProcessDragDropBehavior), null);

        /// <summary> Handles mouse left button down event </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private static void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var state = new InProcessDragDropBehavior { Target = (sender as FrameworkElement) };

            if (state.Target?.Parent != null)
            {
                state.Target.SetValue(DragDropState, state);
                state.PreviewMouseLeftButtonDown(e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse left button down event
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void PreviewMouseLeftButtonDown(MouseEventArgs e)
        {
            HitPosition = e.GetPosition(Target);
            DragPosition = GetPosition(e);
            Position = DragPosition;

            Dragging = false;
            Capturing = true;

            e.Handled = true;
        }

        /// <summary>
        /// Fire a Drag&amp;Drop related routed event
        /// </summary>
        /// <param name="ev">Event args</param>
        /// <returns>True, if Drag&amp;Drop is to be continued. False, if it is to be cancelled.</returns>
        private bool Fire(RoutedEvent ev)
        {
            var args = new DragDropEventArgs(this, ev);
            Target.RaiseEvent(args);
            return !args.Cancel;
        }

        /// <summary>
        /// Handles mouse move event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private static void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (GetState(sender) != null)
                GetState(sender).PreviewMouseMove(e);
        }

        /// <summary>
        /// Handles mouse move event
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void PreviewMouseMove(MouseEventArgs e)
        {
            if ((!Capturing) || (e.LeftButton != MouseButtonState.Pressed)) return;

            if (Target.Parent == null)
                Reset();
            else
            {
                Position = GetPosition(e);

                if (Dragging)
                {
                    if (!Fire(DragMove))
                        Reset();
                }
                else
                {
                    var diff = Position - DragPosition;
                    var minDrag = new Vector(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance);

                    if (Math.Abs(diff.X) > minDrag.X || Math.Abs(diff.Y) > minDrag.Y)
                    {
                        Dragging = Fire(Drag);
                        ZIndex = Panel.GetZIndex(Target);

                        if (!Dragging)
                            Reset();
                        else
                        {
                            Panel.SetZIndex(Target, Int32.MaxValue);
                            PreviewMouseMove(e);
                        }
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Tries to get the state object for the specified object.
        /// </summary>
        /// <param name="sender">Object to get the state for.</param>
        /// <returns>State object. Null on any error.</returns>
        private static InProcessDragDropBehavior GetState(object sender)
        {
            return ((DependencyObject) sender)?.GetValue(DragDropState) as InProcessDragDropBehavior;
        }

        /// <summary>
        /// Handles mouse left button up event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private static void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetState(sender) != null)
                GetState(sender).PreviewMouseLeftButtonUp(e);
        }

        /// <summary>
        /// Handles mouse left button up event
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void PreviewMouseLeftButtonUp(MouseEventArgs e)
        {
            if (!Capturing) return;
            
            if (Target.Parent != null)
            {
                Position = GetPosition(e);

                if (Dragging)
                    if (Fire(DragMove))
                        Fire(Drop);
            }

            e.Handled = true;

            Reset();
        }

        /// <summary>
        /// Gets the corresponding screen coordinates out of the specified mouse event arguments.
        /// </summary>
        /// <param name="e">Event arguments</param>
        private Point GetPosition(MouseEventArgs e)
        {
            return Target.PointToScreen(e.GetPosition(Target));
        }

        /// <summary>
        /// Resets the Drag&amp;Drop operation.
        /// </summary>
        private void Reset()
        {
            Capturing = Dragging = false;
            Panel.SetZIndex(Target, ZIndex);
            Target.SetValue(DragDropState, null);
        }

        /// <summary>
        /// Attaches (= forces) a Drag&amp;Drop operation to the specified element.
        /// </summary>
        /// <param name="e">Element to Drag&amp;Drop</param>
        /// <param name="dragPosition">Drag position</param>
        /// <param name="position">Current position. Defaults to dragPosition, if null.</param>
        /// <param name="fireInitialBegin">Forces a initial Drag event to be fired, if set to true.</param>
        public static void Attach(FrameworkElement e, Point dragPosition, Point? position = null, bool fireInitialBegin = false)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                throw new InvalidOperationException();

            var state = new InProcessDragDropBehavior { Target = e };

            state.Attach(dragPosition, position, fireInitialBegin);
        }

        /// <summary>
        /// Attaches (= forces) a Drag&amp;Drop operation for the targeted element.
        /// </summary>
        /// <param name="dragPosition">Drag position</param>
        /// <param name="position">Current position. Defaults to dragPosition, if null.</param>
        /// <param name="fireInitialBegin">Forces a initial Drag event to be fired, if set to true.</param>
        private void Attach(Point dragPosition, Point? position = null, bool fireInitialBegin = false)
        {
            HitPosition = AnchorPosition;
            Position = DragPosition = dragPosition;

            Dragging = Capturing = true;

            ZIndex = Panel.GetZIndex(Target);
            Panel.SetZIndex(Target, Int32.MaxValue);

            Target.SetValue(DragDropState, this);

            if (fireInitialBegin && !Fire(Drag))
                Reset();
            else
            {
                Position = position ?? DragPosition;

                if (!Fire(DragMove))
                    Reset();
            }
        }
    }
}
