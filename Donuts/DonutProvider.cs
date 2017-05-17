using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using Ptv.XServer.Demo.MapMarket;
using System;
using System.Collections.Generic;

namespace Donuts
{
    /// <summary>
    /// The donut provider creates an in-memory set of computed shapes and stores them as OCG binaries
    /// inside a quad-tree. This is just a template to use your own computed shapes.
    /// </summary>
    public class DonutProvider : IGeoProvider
    {
        // use a quad tree to efficiently store the objects
        private Quadtree quadTree;

        public DonutProvider()
        {
            UpdateShapes();
        }

        // re-create ou shapes
        public void UpdateShapes()
        {
            // re-initialize the quadtree
            quadTree = new Quadtree();

            // we just generate some random objects
            var rand = new Random();

            for (var dix = 0; dix < 10000; dix++)
            {
                // our donut parameters
                var lat = rand.NextDouble() * 4 + 47.0; // latitude
                var lon = rand.NextDouble() * 6 + 5.0; // logitude
                var rot = rand.NextDouble() * Math.PI; // rotation angle
                var radiusX = rand.NextDouble() * 2000.0 + 1000.0; // x-radius in m
                var radiusY = rand.NextDouble() * 2000.0 + 1000.0; // y-radius in m
                var buffer = 1000.0; // buffer size in m

                // the donut shapes are calulcated in a mercator (= conrformal) projection
                // This means we can assiciate units with meters and angles are correct
                // see http://bl.ocks.org/oliverheilig/29e494c33ef58c6d5839
                var mercP = Ptv.XServer.Controls.Map.Tools.GeoTransform.WGSToPtvMercator(new System.Windows.Point(lon, lat));

                // in our conformal projection we have to adopt the size depending on the latitude
                var f = 1.0 / Math.Cos((lat / 360) * 2 * Math.PI);
                radiusX *= f;
                radiusY *= f;
                buffer *= f;

                // the step size for the approximation
                var numVertices = 100;
                var darc = 2 * Math.PI / numVertices;

                // create shell
                var shell = new List<ICoordinate>();
                for (var i = 0; i < numVertices; i++)
                {
                    var arc = darc * i;

                    var xPos = mercP.X - (radiusX * Math.Sin(arc)) * Math.Sin(rot * Math.PI) + (radiusY * Math.Cos(arc)) * Math.Cos(rot * Math.PI);
                    var yPos = mercP.Y + (radiusY * Math.Cos(arc)) * Math.Sin(rot * Math.PI) + (radiusX * Math.Sin(arc)) * Math.Cos(rot * Math.PI);

                    var wgsPoint = Ptv.XServer.Controls.Map.Tools.GeoTransform.PtvMercatorToWGS(new System.Windows.Point(xPos, yPos));

                    shell.Add(new Coordinate(wgsPoint.X, wgsPoint.Y));
                }
                shell.Add(shell[0]); // close ring

                // create hole
                var hole = new List<ICoordinate>();
                for (var i = 0; i < numVertices; i++)
                {
                    var arc = darc * i;

                    var xPos = mercP.X - ((radiusX - buffer) * Math.Sin(arc)) * Math.Sin(rot * Math.PI) + ((radiusY - buffer) * Math.Cos(arc)) * Math.Cos(rot * Math.PI);
                    var yPos = mercP.Y + ((radiusY - buffer) * Math.Cos(arc)) * Math.Sin(rot * Math.PI) + ((radiusX - buffer) * Math.Sin(arc)) * Math.Cos(rot * Math.PI);

                    var wgsPoint = Ptv.XServer.Controls.Map.Tools.GeoTransform.PtvMercatorToWGS(new System.Windows.Point(xPos, yPos));

                   hole.Add(new Coordinate(wgsPoint.X, wgsPoint.Y));
                }
                hole.Add(hole[0]); // close ring

                // store the geomety as well known binary
                var p = Geometry.DefaultFactory.CreatePolygon(
                    Geometry.DefaultFactory.CreateLinearRing(shell.ToArray()),
                    new ILinearRing[] { Geometry.DefaultFactory.CreateLinearRing(hole.ToArray()) });
                var wkbw = new GisSharpBlog.NetTopologySuite.IO.WKBWriter();
                var wkb = wkbw.Write(p);

                // GeoItem is the container element that holds a shape
                var gi = new GeoItem
                {
                    Id = dix,
                    Wkb = wkb,
                    XMin = p.EnvelopeInternal.MinX,
                    YMin = p.EnvelopeInternal.MinY,
                    XMax = p.EnvelopeInternal.MaxX,
                    YMax = p.EnvelopeInternal.MaxY,
                };

                // insert into our quadtree
                quadTree.Insert(p.EnvelopeInternal, gi);
            }
        }

        // implements the query method
        public IEnumerable<GeoItem> QueryBBox(double xmin, double ymin, double xmax, double ymax, string[] attributes)
        {
            var p1 = new Point(xmin, ymin);
            var p2 = new Point(xmax, ymax);
            var e = new Envelope(p1.X, p2.X, p1.Y, p2.Y);

            foreach (GeoItem gi in quadTree.Query(e))
            {
                if (e.Intersects(new Envelope(gi.XMin, gi.YMin, gi.XMax, gi.YMax)))
                    yield return gi;       
            }
        }
        public IEnumerable<GeoItem> QueryPoint(double x, double y, string[] attributes)
        {
            var point = Geometry.DefaultFactory.CreatePoint(new Coordinate(x, y));
            foreach (var gi in QueryBBox(x, y, x, y, attributes))
            {
                var geometry = new GisSharpBlog.NetTopologySuite.IO.WKBReader().Read(gi.Wkb);
                if (geometry.Contains(point))
                    yield return gi;
            }
        }
    }
}
