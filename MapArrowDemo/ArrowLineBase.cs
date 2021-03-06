//----------------------------------------------
// ArrowLineBase.cs (c) 2007 by Charles Petzold
//----------------------------------------------

using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.Layers.Shapes;

namespace Petzold.Media2D
{
    /// <summary>Provides a base class for ArrowLine and ArrowPolyline. </summary>
    public abstract class ArrowLineBase : MapShape
    {
        protected PathGeometry pathGeometry;
        protected PathFigure pathFigureLine;
        protected PolyLineSegment polyLineSegmentLine;

        private readonly PathFigure pathFigureHead1;
        private readonly PathFigure pathFigureHead2;

        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(3.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowEnds",
                typeof(ArrowEnds), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(ArrowEnds.End,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }
        }

        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty =
            DependencyProperty.Register("IsArrowClosed",
                typeof(bool), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(false,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }

        /// <summary>
        /// Initializes a new instance of ArrowLineBase.
        /// </summary>
        protected ArrowLineBase()
        {
            pathGeometry = new PathGeometry();

            pathFigureLine = new PathFigure();
            polyLineSegmentLine = new PolyLineSegment();
            pathFigureLine.Segments.Add(polyLineSegmentLine);

            pathFigureHead1 = new PathFigure();
            var polylineSegmentHead1 = new PolyLineSegment();
            pathFigureHead1.Segments.Add(polylineSegmentHead1);

            pathFigureHead2 = new PathFigure();
            var polylineSegmentHead2 = new PolyLineSegment();
            pathFigureHead2.Segments.Add(polylineSegmentHead2);
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                int count = polyLineSegmentLine.Points.Count;

                if (count > 0)
                {
                    // Draw the arrow at the start of the line.
                    if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                    {
                        Point pt1 = pathFigureLine.StartPoint;
                        Point pt2 = polyLineSegmentLine.Points[0];
                        pathGeometry.Figures.Add(CalculateArrow(pathFigureHead1, pt2, pt1));
                    }

                    // Draw the arrow at the end of the line.
                    if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                    {
                        Point pt1 = count == 1 ? pathFigureLine.StartPoint :
                                                 polyLineSegmentLine.Points[count - 2];
                        Point pt2 = polyLineSegmentLine.Points[count - 1];
                        pathGeometry.Figures.Add(CalculateArrow(pathFigureHead2, pt1, pt2));
                    }
                }
                return pathGeometry;
            }
        }

        private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            var matrix = new Matrix();
            Vector vector = pt1 - pt2;
            vector.Normalize();
            vector *= ArrowLength * StrokeThickness;

            var polyLineSegment = pathfig.Segments[0] as PolyLineSegment;
            polyLineSegment.Points.Clear();
            matrix.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = pt2 + vector * matrix;
            polyLineSegment.Points.Add(pt2);

            matrix.Rotate(-ArrowAngle);
            polyLineSegment.Points.Add(pt2 + vector * matrix);
            pathfig.IsClosed = IsArrowClosed;

            return pathfig;
        }
    }
}
