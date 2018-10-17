// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Ptv.XServer.Demo.UseCases
{
    /// <summary>
    /// Helper class concentrating the style and layout of the routing polyline.
    /// It demonstrates the display of directions using an animated offset stroke.
    /// Additionally, the insertion and removal of the polyline shapes into a shape layer is encapsulated.
    /// </summary>
    public class RoutePolyline : IDisposable
    {
        /// <summary> Storyboard for the animation of the route direction. </summary>
        private readonly Storyboard strokeStoryboard = new Storyboard();

        /// <summary>Layer to which the polyline shapes are added.</summary>
        private readonly ShapeLayer shapeLayer;

        /// <summary>Creates a new polyline shape adapted to the needs of a routing polyline.</summary>
        /// <param name="layer">Layer which will contain the generated polyline shapes.</param>
        public RoutePolyline(ShapeLayer layer)
        {
            shapeLayer = layer;

            MapPolyline = new MapPolyline
            {
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                MapStrokeThickness = 25,
                ScaleFactor = .1,               
            };

            AnimatedPolyline = new MapPolyline
            {
                Stroke = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                StrokeLineJoin = MapPolyline.StrokeLineJoin,
                StrokeStartLineCap = MapPolyline.StrokeStartLineCap,
                StrokeEndLineCap = MapPolyline.StrokeEndLineCap,
                StrokeDashCap = PenLineCap.Triangle,
                StrokeDashArray = new DoubleCollection { 2, 2 },

                IsHitTestVisible = false,
                MapStrokeThickness = MapPolyline.MapStrokeThickness - 5,
                ScaleFactor = MapPolyline.ScaleFactor,
            };

            var animation = new DoubleAnimation
            {
                From = 4,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(1500),
                FillBehavior = FillBehavior.HoldEnd,
                RepeatBehavior = RepeatBehavior.Forever
            };

            strokeStoryboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Line.StrokeDashOffset)"));
            Storyboard.SetTarget(animation, AnimatedPolyline);
            strokeStoryboard.Begin();

            Color = Colors.Black;
        }

        /// <inheritdoc/>  
        public void Dispose()
        {
            shapeLayer.Shapes.Remove(MapPolyline);
            shapeLayer.Shapes.Remove(AnimatedPolyline);
        }

        /// <summary>Gets the polyline shape representing the base polyline.</summary>
        public MapPolyline MapPolyline { get; private set; }

        /// <summary>Gets the polyline shape needed for animation effects.</summary>
        public MapPolyline AnimatedPolyline { get; private set; }

        /// <summary>Gets and sets the geometry of the polyline. </summary>
        public IEnumerable<Point> Points
        {
            get { return MapPolyline.Points; }
            set
            {
                MapPolyline.Points = new PointCollection();
                AnimatedPolyline.Points = MapPolyline.Points;

                if ((value == null) || !value.Any())
                {
                    shapeLayer.Shapes.Remove(MapPolyline);
                    shapeLayer.Shapes.Remove(AnimatedPolyline);
                }
                else
                {
                    foreach (Point p in value)
                        MapPolyline.Points.Add(p);

                    shapeLayer.Shapes.Add(MapPolyline);
                    shapeLayer.Shapes.Add(AnimatedPolyline);

                    //MapPolyline.MouseEnter += MapMouseEnter;
                    //MapPolyline.MouseLeave += MapMouseLeave;
                    //restartAnimation = true;
                }
            }
        }

        private Color color;

        /// <summary>
        /// Gets and sets the color of the polyline shape.
        /// </summary>
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                MapPolyline.Stroke = new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B));
            }
        }

        /// <summary>Gets or sets the width of the polyline.</summary>
        public double Width
        {
            get => MapPolyline.MapStrokeThickness;
            set => MapPolyline.MapStrokeThickness = AnimatedPolyline.MapStrokeThickness = value;
        }

        /// <summary>Gets and sets the tooltip text along the polyline shape.</summary>
        public string ToolTip
        {
            get => MapPolyline.ToolTip as string;
            set => MapPolyline.ToolTip = string.IsNullOrEmpty(value) ? null : value;
        }

        /* The following mouse events are not used, because they activate the animation
         * when the mouse is over the polyline. If a temporary via point is used in the 
         * drag&drop routing use case, it will hide the mouse event handlers; therefore,
         * the animation is activated every time.
          
        /// <summary>
        /// Event handler for the marching ants animation of the route.
        /// </summary>
        /// <param name="sender"> The sender of the MouseEnter event. </param>
        /// <param name="e"> The event parameters. </param>
        private void MapMouseEnter(object sender, object e)
        {
            if (restartAnimation)
            {
                strokeStoryboard.Begin();
                restartAnimation = false;
            }
            else
                strokeStoryboard.Resume();
        }

        /// <summary>
        /// Event handler for the marching ants animation of the route.
        /// </summary>
        /// <param name="sender"> The sender of the MouseLeave event. </param>
        /// <param name="e"> The event parameters. </param>
        private void MapMouseLeave(object sender, object e)
        {
            strokeStoryboard.Pause();
        }
         
        */
    }
}
