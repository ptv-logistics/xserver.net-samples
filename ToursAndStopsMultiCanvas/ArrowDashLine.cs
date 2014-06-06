using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Tools;
using System.Windows;

// Note: This class would be much easier to implement if BuildGeometry was internal! So it is just a replicaion and modification of the default
// MapPolyline implementation
namespace ToursAndStops
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

        /// <inheritdoc/>
        protected void TransformShape()
        {
            TransformedPoints.Clear();

            for (int i = 0; i < Points.Count; i++)
            {
                var mercatorPoint = GeoTransform(Points[i]);

                TransformedPoints.Add(mercatorPoint);
            }
        }

        protected virtual void ClipShape(MapView mapView, UpdateMode mode, bool lazyUpdate)
        {
            if (mode == UpdateMode.Refresh)
                TransformShape();

            if (!NeedsUpdate(lazyUpdate, mode))
                return;

            MapRectangle rect = mapView.CurrentEnvelope;
            Size sz = new Size(mapView.ActualWidth, mapView.ActualHeight);
            var minX = rect.West;
            var minY = rect.South;
            var maxX = rect.East;
            var maxY = rect.North;
            Rect clippingRect = new Rect(minX, -maxY, maxX - minX, maxY - minY);

            double thickness = this.CurrentThickness(mapView.CurrentScale);

            clippingRect.X -= .5 * thickness;
            clippingRect.Y -= .5 * thickness;
            clippingRect.Width += thickness;
            clippingRect.Height += thickness;

            ICollection<PointCollection> tmpPoints = LineReductionClipping.ClipPolylineReducePoints<PointCollection, System.Windows.Point>(
                           sz,
                           clippingRect,
                           TransformedPoints,
                           p => p,
                           (poly, pnt) => poly.Add(pnt));

            this.Data = BuildGeometry(mapView, tmpPoints);

            this.InvalidateVisual();
        }

        protected bool NeedsUpdate(bool lazyUpdate, UpdateMode updateMode)
        {
            return
                (lazyUpdate && updateMode == UpdateMode.EndTransition)
                || (!lazyUpdate && updateMode == UpdateMode.WhileTransition)
                || updateMode == UpdateMode.Refresh;
        }

        /// <inheritdoc/>
        public override void UpdateShape(MapView mapView, UpdateMode mode, bool lazyUpdate)
        {
            ClipShape(mapView, mode, lazyUpdate);

            if (NeedsUpdate(lazyUpdate, mode))
                base.UpdateShape(mapView, mode, lazyUpdate);

            this.StrokeThickness = mapView.CurrentScale;
        }

        /// <summary> Builds the geometry of the polyline. </summary>
        /// <param name="lines"> A collection of point collections to build the geometry for (multiple polylines). </param>
        /// <returns> The geometry corresponding to the given point collections. </returns>
        protected Geometry BuildGeometry(MapView mapView, ICollection<PointCollection> lines)
        {
            StreamGeometry geom = new StreamGeometry();

            using (StreamGeometryContext gc = geom.Open())
            {
                foreach (PointCollection points in lines)
                {
                    double len = CurrentThickness(mapView.CurrentScale) * 2;
                    int pos = 0;
                    double rel = 0;
                    Point pp1 = points[0];
                    bool first = true;
                    while (GetNextArrPos(points, first? len /2 : len, ref pos, ref rel))
                    {
                        first = false;

                        Point p1 = points[pos];
                        Point p2 = points[pos + 1];

                        double vx = p2.X - p1.X;
                        double vy = p2.Y - p1.Y;

                        double lv = Math.Sqrt(vx * vx + vy * vy);

                        Point p = new Point();
                        p.X = p1.X + (vx * rel) / lv;
                        p.Y = p1.Y + (vy * rel) / lv;

                        Point vpp = new Point(); ;
                        vpp.X = 2 * p.X - pp1.X;
                        vpp.Y = 2 * p.Y - pp1.Y;

                        DrawArrow(gc, p, vpp, len * .5, 0);
                        pp1 = p;
                    }
                }
            }

            return geom;
        }



        bool GetNextArrPos(IList<Point> points, double len, ref int pos, ref double rel)
        {
            while (pos < points.Count - 1)
            {
                Point p1 = points[pos];
                Point p2 = points[pos + 1];

                double vx = p2.X - p1.X;
                double vy = p2.Y - p1.Y;

                double lv = Math.Sqrt(vx * vx + vy * vy);

                double rel2 = rel + len;

                if (rel2 < lv)
                {
                    rel = rel2;
                    return true;
                }
                else
                {
                    pos++;
                    rel = 0;
                    len = rel2 - lv;
                }
            }

            return false;
        }

        void DrawArrow(StreamGeometryContext gc, Point from, Point to2, double Wings, double offset)
        {
            Point to = new Point(from.X * (1.0 - offset) + to2.X * offset, from.Y * (1.0 - offset) + to2.Y * offset);

            double dx = to2.X - from.X;
            double dy = to2.Y - from.Y;

            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len == 0)
                return;

            dx = (dx / len) * (Wings);
            dy = (dy / len) * (Wings);

            Point A = new Point(to.X - dx, to.Y - dy);

            var pts0 = new Point(to.X, to.Y);
            dx /= 2;
            dy /= 2;
            var pts1 = new Point(A.X - dy, A.Y + dx);
            var pts2 = new Point(A.X + dx / 2, A.Y + dy / 2);
            var pts3 = new Point(A.X + dy, A.Y - dx);

            gc.BeginFigure(pts0, true, true);
            gc.PolyLineTo(new Point[] { pts1, pts2, pts3 }, true, true);

        }
        #endregion
    }
}
