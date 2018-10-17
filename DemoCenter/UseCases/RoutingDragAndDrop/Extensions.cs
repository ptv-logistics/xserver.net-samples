// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Ptv.Components.Projections;
using GeoAPI.Geometries;

namespace Ptv.XServer.Controls.Routing
{
    /// <summary>
    /// Provides some useful extension methods.
    /// </summary>
    internal static class RouteLayerExtensions
    {
        /// <summary>
        /// Coordinate transformation: WGS84 > PTV Mercator
        /// </summary>
        private static readonly ICoordinateTransformation Wgs84ToPtvMercatorTransform =
            Components.Projections.Direct.CoordinateTransformation.Get("EPSG:4326", "EPSG:76131");

        /// <summary>
        /// Coordinate transformation: WGS84 > PTV SmartUnits
        /// </summary>
        static readonly ICoordinateTransformation Wgs84ToPtvSmartUnitsTransform =
            Components.Projections.Direct.CoordinateTransformation.Get("EPSG:4326", "PTV_SMARTUNITS");

        /// <summary>
        /// Coordinate transformation: PTV Mercator > WGS84
        /// </summary>
        static readonly ICoordinateTransformation PtvMercatorToWgs84Transform =
            Components.Projections.Direct.CoordinateTransformation.Get("EPSG:76131", "EPSG:4326");

        /// <summary>
        /// Coordinate transformation: PTV SmartUnits > WGS84
        /// </summary>
        static readonly ICoordinateTransformation PtvSmartUnitsToWgs84Transform =
            Components.Projections.Direct.CoordinateTransformation.Get("PTV_SMARTUNITS", "EPSG:4326");

        /// <summary>
        /// Determines the closest point on a polyline given a point p.
        /// </summary>
        /// <param name="points">Polyline</param>
        /// <param name="p">Coordinates to find the closest point for.</param>
        /// <param name="q">Resulting point</param>
        /// <param name="index">Index of the resuling point</param>
        /// <returns></returns>
        public static bool GetNearestPoint(this IEnumerable<Point> points, Point p, out Point q, out int index)
        {
            index = -1;
            q = new Point();

            double len = 0;
            var e = points?.GetEnumerator();

            if (e == null) return false;

            for (int i = 0; e.MoveNext(); ++i)
                if (i == 0 || (e.Current - p).Length < len)
                {
                    len = ((q = e.Current) - p).Length;
                    index = i;
                }

            return index != -1;
        }

        /// <summary>
        /// Transforms a point into a way point description for xRoute
        /// </summary>
        /// <param name="p">Point to create the way point from</param>
        /// <param name="viaType">Via type to be set, if any</param>
        /// <returns>Newly created way point</returns>
        public static Demo.XrouteService.WaypointDesc ToWaypoint(this Point p, Demo.XrouteService.ViaTypeEnum? viaType = null)
        {
            p = Wgs84ToPtvSmartUnitsTransform.Transform(p);

            var waypointDesc = new Demo.XrouteService.WaypointDesc
            {
                linkType = Demo.XrouteService.LinkType.AUTO_LINKING,
                wrappedCoords = new[] { new Demo.XrouteService.Point { point = new Demo.XrouteService.PlainPoint { x = p.X, y = p.Y } } }
            };

            if (viaType.HasValue)
                waypointDesc.viaType = new Demo.XrouteService.ViaType { viaType = viaType.Value };

            return waypointDesc;
        }


        /// <summary>
        /// Adds a menu item to a context menu.
        /// </summary>
        /// <param name="m">Context menu to add the item to</param>
        /// <param name="text">Text of the menu item</param>
        /// <param name="action">Action to execute if the item was clicked</param>
        /// <returns>Newly create menu item</returns>
        public static System.Windows.Controls.MenuItem Add(this System.Windows.Controls.ContextMenu m, string text, Action action = null)
        {
            var i = new System.Windows.Controls.MenuItem {Header = text};

            i.Click += (o, a) =>
            {
                action?.Invoke();
            };

            m.Items.Add(i);

            return i;
        }

        /// <summary>
        /// Modifies the alpha value of a color
        /// </summary>
        /// <param name="color">Base color to modify</param>
        /// <param name="alpha">Alpha value to set</param>
        /// <returns>Color with modified alpha value</returns>
        public static Color SetA(this Color color, byte alpha)
        {
            color.A = alpha;
            return color;
        }

        /// <summary>
        /// Transforms coordinates from WSG84 to PTV Mercator.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <returns>Transformed point</returns>
        public static Point Wgs84ToPtvMercator(this Point p)
        {
            return Wgs84ToPtvMercatorTransform.Transform(p);
        }

        /// <summary>
        /// Transforms coordinates from PTV Mercator to WSG84;
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <returns>Transformed point</returns>
        public static Point PtvMercatorToWgs84(this Point p)
        {
            return PtvMercatorToWgs84Transform.Transform(p);
        }

        /// <summary>
        /// Transforms xRoute WKB coordinates from PTV SmartUnits to WSG84.
        /// </summary>
        /// <param name="c">Extension object</param>
        /// <returns>Transformed point</returns>
        public static Point PtvSmartUnitsToWgs84(this ICoordinate c)
        {
            return PtvSmartUnitsToWgs84Transform.Transform(new Point(c.X, c.Y));
        }

        /// <summary>
        /// Transforms xRoute coordinates from PTV SmartUnits to WSG84.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <returns>Transformed point</returns>
        public static Point PtvSmartUnitsToWgs84(this Demo.XrouteService.PlainPoint p)
        {
            return PtvSmartUnitsToWgs84Transform.Transform(new Point(p.x, p.y));
        }
        
        /// <summary>
        /// Inserts an item in an array. Resizes the array.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="a">Array to insert an item in</param>
        /// <param name="idx">Index where to insert the item</param>
        /// <param name="t">Item to insert</param>
        /// <returns>Resized and updated array</returns>
        public static T[] Insert<T>(this T[] a, int idx, T t)
        {
            var l = a.ToList();
            l.Insert(idx, t);
            return l.ToArray();
        }

        /// <summary>
        /// Removes an item from an array. Resizes the array.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="a">Array to insert an item in</param>
        /// <param name="idx">Index of the item to remove</param>
        /// <returns>Resized and updated array</returns>
        public static T[] RemoveAt<T>(this T[] a, int idx)
        {
            var l = a.ToList();
            l.RemoveAt(idx);
            return l.ToArray();
        }
    }
}
