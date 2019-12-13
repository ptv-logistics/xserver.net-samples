using System;
using System.Collections.Generic;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Tools;
using System.Windows;


namespace FormsMapCS
{
    /// <summary><para> This class represents a polyline on the map. The MapPolyline is responsible for adapting the visual
    /// to the current map viewport in terms of scaling and clipping. </para>
    /// <para> See the <conceptualLink target="06a654f3-afbd-4f00-9c8e-36997e2a3951"/> topic for an example. </para></summary>
    public class ArrowDashLine : MapPolylineBase
    {
        /// <summary> Initializes a new instance of the <see cref="MapPolyline"/> class. Initializes the
        /// <see cref="MapShape.ScaleFactor"/> to 0.5. </summary>
        public ArrowDashLine()
        {
            ScaleFactor = 0.0;
        }

        #region public methods

        protected void TransformShape()
        {
            if (GeoTransform == null) return;

            TransformedPoints.Clear();
            foreach (var point in Points)
                TransformedPoints.Add(GeoTransform(point));
        }

        protected virtual void ClipShape(MapView mapView, UpdateMode mode, bool lazyUpdate)
        {
            if (mode == UpdateMode.Refresh)
                TransformShape();

            if (!NeedsUpdate(lazyUpdate, mode))
                return;

            var mapRectangle = mapView.CurrentEnvelope;
            var size = new Size(mapView.ActualWidth, mapView.ActualHeight);
            var minX = mapRectangle.West;
            var minY = mapRectangle.South;
            var maxX = mapRectangle.East;
            var maxY = mapRectangle.North;
            var clippingRect = new Rect(minX, -maxY, maxX - minX, maxY - minY);

            double thickness = CurrentThickness(mapView.CurrentScale);

            clippingRect.X -= .5 * thickness;
            clippingRect.Y -= .5 * thickness;
            clippingRect.Width += thickness;
            clippingRect.Height += thickness;

            ICollection<PointCollection> tmpPoints = LineReductionClipping.ClipPolylineReducePoints<PointCollection, Point>(
                           size,
                           clippingRect,
                           TransformedPoints,
                           p => p,
                           (poly, pnt) => poly.Add(pnt));

            Data = BuildGeometry(mapView, tmpPoints);

            InvalidateVisual();
        }

        protected new bool NeedsUpdate(bool lazyUpdate, UpdateMode updateMode)
        {
            return lazyUpdate && updateMode == UpdateMode.EndTransition
                || !lazyUpdate && updateMode == UpdateMode.WhileTransition
                || updateMode == UpdateMode.Refresh;
        }

        /// <inheritdoc/>
        public override void UpdateShape(MapView mapView, UpdateMode mode, bool lazyUpdate)
        {
            ClipShape(mapView, mode, lazyUpdate);

            if (NeedsUpdate(lazyUpdate, mode))
                base.UpdateShape(mapView, mode, lazyUpdate);

            StrokeThickness = mapView.CurrentScale;
        }

        /// <summary> Builds the geometry of the polyline. </summary>
        /// <param name="mapView">Map view which will show the lines.</param>
        /// <param name="lines"> A collection of point collections to build the geometry for (multiple) polylines. </param>
        /// <returns> The geometry corresponding to the given point collections. </returns>
        protected Geometry BuildGeometry(MapView mapView, ICollection<PointCollection> lines)
        {
            var streamGeometry = new StreamGeometry();

            using (var streamGeometryContext = streamGeometry.Open())
            {
                foreach (var points in lines)
                {
                    double len = CurrentThickness(mapView.CurrentScale) * 2;
                    int pos = 0;
                    double rel = 0;
                    var pp1 = points[0];
                    var first = true;
                    while (GetNextArrPos(points, first? len /2 : len, ref pos, ref rel))
                    {
                        first = false;

                        var p1 = points[pos];
                        var p2 = points[pos + 1];

                        double vx = p2.X - p1.X;
                        double vy = p2.Y - p1.Y;

                        double lv = Math.Sqrt(vx * vx + vy * vy);

                        var p = new Point {X = p1.X + vx * rel / lv, Y = p1.Y + vy * rel / lv};

                        var vpp = new Point {X = 2 * p.X - pp1.X, Y = 2 * p.Y - pp1.Y};

                        DrawArrow(streamGeometryContext, p, vpp, len * .5, 0);
                        pp1 = p;
                    }
                }
            }

            return streamGeometry;
        }

        private static bool GetNextArrPos(IList<Point> points, double len, ref int pos, ref double rel)
        {
            while (pos < points.Count - 1)
            {
                var p1 = points[pos];
                var p2 = points[pos + 1];

                double vx = p2.X - p1.X;
                double vy = p2.Y - p1.Y;

                double lv = Math.Sqrt(vx * vx + vy * vy);

                double rel2 = rel + len;

                if (rel2 < lv)
                {
                    rel = rel2;
                    return true;
                }

                pos++;
                rel = 0;
                len = rel2 - lv;
            }

            return false;
        }

        private static void DrawArrow(StreamGeometryContext gc, Point from, Point to2, double Wings, double offset)
        {
            var to = new Point(from.X * (1.0 - offset) + to2.X * offset, from.Y * (1.0 - offset) + to2.Y * offset);

            double dx = to2.X - from.X;
            double dy = to2.Y - from.Y;

            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len == 0)
                return;

            dx = dx / len * Wings;
            dy = dy / len * Wings;

            var A = new Point(to.X - dx, to.Y - dy);

            var pts0 = new Point(to.X, to.Y);
            dx /= 2;
            dy /= 2;
            var pts1 = new Point(A.X - dy, A.Y + dx);
            var pts2 = new Point(A.X + dx / 2, A.Y + dy / 2);
            var pts3 = new Point(A.X + dy, A.Y - dx);

            gc.BeginFigure(pts0, true, true);
            gc.PolyLineTo(new[] { pts1, pts2, pts3 }, true, true);

        }
        #endregion
    }
}
