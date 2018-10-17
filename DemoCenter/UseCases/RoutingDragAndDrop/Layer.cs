// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;

namespace Ptv.XServer.Controls.Routing
{
    /// <summary>
    /// A layer that encapsulates a route that can be defined and 
    /// modified via mouse & context menu operations.
    /// </summary>
    public class RouteLayer : ShapeLayer
    {
        /// <summary>
        /// list of stops
        /// </summary>
        private List<Stop> stops;

        /// <summary>
        /// list of routes. There are "stops.Count - 1" routes.
        /// </summary>
        private List<Route> routes;

        /// <summary>
        /// Delay for updating routes on Drag&amp;Drop operations.
        /// </summary>
        public const int MOVE_UPDATE_DELAY = 200;

        /// <summary>
        /// Style of a stop
        /// </summary>
        public static readonly ShapeStyle STOP_STYLE = new ShapeStyle { Color = Colors.LightGreen.SetA(192), Size = 40 };

        /// <summary>
        /// Style of a via way point
        /// </summary>
        public static readonly ShapeStyle VIA_STYLE = new ShapeStyle { Color = Colors.White.SetA(192), Size = 12 };

        /// <summary>
        /// Style of a temporary via way point (= draggable symbol on a route).
        /// </summary>
        public static readonly ShapeStyle TMPVIA_STYLE = new ShapeStyle { Color = Colors.LightGray.SetA(128), Size = 10 };

        /// <summary>
        /// Route style.
        /// </summary>
        public static readonly ShapeStyle ROUTE = new ShapeStyle { Color = Colors.Blue.SetA(128), Size = 15 };

        /// <summary>
        /// Style used for routing errors.
        /// </summary>
        public static readonly ShapeStyle ROUTE_ERROR = new ShapeStyle { Color = Colors.Red.SetA(128), Size = 15 };

        /// <summary>
        /// Style used for previewing routes.
        /// </summary>
        public static readonly ShapeStyle ROUTE_PREVIEW = new ShapeStyle { Color = Colors.LightGray.SetA(128), Size = 15 };
                
        /// <summary>
        /// Create the route layer.
        /// </summary>
        /// <param name="map">Map control to attach to.</param>
        public RouteLayer(WpfMap map) : base("Route")
        {
            Map = map;
            WayPoint.WayPointMouseRightButtonDown += ShowWayPointContextMenu;
            Reset();
        }

        /// <summary>
        /// The method inserts an additional event handler for showing the context menu,
        /// when this layer is added to a map view.
        /// </summary>
        /// <param name="mapView">Mapview to which this layer is added.</param>
        public override void AddToMapView(MapView mapView)
        {
            base.AddToMapView(mapView);
            Map.MouseRightButtonDown += ShowContextMenu;
            Map.PreviewMouseMove += Map_PreviewMouseMove;
        }

        /// <summary>
        /// The method removes the previously inserted event handler for showing the context menu,
        /// when this layer is removed from a map view.
        /// </summary>
        /// <param name="mapView">Mapview to which this layer is added.</param>
        public override void RemoveFromMapView(MapView mapView)
        {
            Map.PreviewMouseMove -= Map_PreviewMouseMove;
            Map.MouseRightButtonDown -= ShowContextMenu;
            base.RemoveFromMapView(mapView);
        }

        /// <summary>
        /// Given a point p, determines the nearest point on the route.
        /// </summary>
        /// <param name="maxDistance">Maximum distance</param>
        /// <param name="nearestRoute">The nearest route</param>
        /// <param name="nearestPoint">Input point and nearest point on the nearest route</param>
        /// <returns>True, if a match could be made that is withing a distance of maxDistance. False otherwise.</returns>
        private bool GetNearestRoutePoint(int maxDistance, ref Route nearestRoute, ref Point nearestPoint)
        {
            int index;
            double len = 0;
            Point p = nearestPoint;
            Point q = ToWorld(p), r;

            foreach (var route in routes.Where(rte => rte.HasRoutePolyline))
                if (route.Points.GetNearestPoint(q, out r, out index))
                    if (nearestRoute == null || (r - q).LengthSquared < len)
                    {
                        len = (r - q).LengthSquared;
                        nearestRoute = route;
                        nearestPoint = r;
                    }

            return nearestRoute != null && (ToMap(nearestPoint) - p).Length <= maxDistance;
        }

        /// <summary>
        /// Handles mouse move event. 
        /// Displays a draggable, temporary via way point if a route is in range.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void Map_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Route route = null;
            var p = e.GetPosition(Map);

            if (GetNearestRoutePoint(48, ref route, ref p))
                TemporaryVia.Show(this, route, p);
            else
                TemporaryVia.Hide();
        }

        /// <summary>
        /// Displays a context menu on a route way point (via or stop).
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void ShowWayPointContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is WayPoint) || (sender is TemporaryVia)) return;

            var menu = new ContextMenu();

            var p = e.GetPosition(Map);

            if (sender is Via via)
                menu.Add("Remove via way point", () => via.Route.Remove(via));
            if (sender is Stop stop)
                menu.Add("Remove stop " + stop.Label, () => Remove(stop));

            menu.PlacementRectangle = new Rect(p, p);
            menu.IsOpen = true;

            e.Handled = true;
        }

        /// <summary>
        /// Displays a context menu on the map.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            var menu = new ContextMenu();

            var p = e.GetPosition(Map);
            var q = ToWorld(p);

            menu.Add("Route from here", () => First = new Stop(this, q, STOP_STYLE));
            menu.Add("Route to here", () => Last = new Stop(this, q, STOP_STYLE));

            if (Valid)
            {
                menu.Items.Add(new Separator());
                menu.Add("Add destination", () => Append(new Stop(this, q, STOP_STYLE)));
            }

            menu.Items.Add(new Separator());
            menu.Add("Reset and start new calculation", Reset);

            menu.PlacementRectangle = new Rect(p, p);
            menu.IsOpen = true;

            e.Handled = true;
        }

        /// <summary>
        /// Accesses the corresponding map control.
        /// </summary>
        public WpfMap Map
        {
            get;
            private set;
        }

        /// <summary>
        /// Transforms a given point p (relative to the map) to WGS84.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <returns>WGS84 coordinates</returns>
        public Point ToWorld(Point p)
        {
            var e = Map.GetEnvelope("EPSG:76131");

            p.X /= Map.ActualWidth;
            p.Y /= Map.ActualHeight;

            return new Point(e.West + (e.East - e.West) * p.X, e.North + (e.South - e.North) * p.Y).PtvMercatorToWgs84();
        }

        /// <summary>
        /// Transforms given WGS84 coordinates to a point, relative to the map.
        /// </summary>
        /// <param name="p">Point to transform</param>
        /// <returns>WGS84 coordinates</returns>
        public Point ToMap(Point p)
        {
            var e = Map.GetEnvelope("EPSG:76131");

            p = p.Wgs84ToPtvMercator();

            return new Point(Map.ActualWidth *  (p.X - e.West) / (e.East - e.West), Map.ActualHeight * (p.Y - e.North) / (e.South - e.North));
        }

        /// <summary>
        /// Resets routing.
        /// </summary>
        public void Reset()
        {
            Shapes.Clear();

            stops = new List<Stop> { null, null };
            routes = new List<Route> { new Route(this) };
        }

        /// <summary>
        /// Triggers the necessary updates when a way point was moved.
        /// </summary>
        /// <param name="wayPoint">Way point that was moved.</param>
        public void WayPointMoved(WayPoint wayPoint)
        {
            if (wayPoint is Via via)
                via.Route.UpdateRouteForMovingWayPoint(via, MOVE_UPDATE_DELAY);
            else
            {
                var stop = wayPoint as Stop;
                int idx = stops.IndexOf(stop);

                UpdateRoute(stop, idx - 1, MOVE_UPDATE_DELAY);
                UpdateRoute(stop, idx, MOVE_UPDATE_DELAY);
            }
        }

        /// <summary>
        /// Sets a stop at a specific index in the stop list. 
        /// </summary>
        /// <param name="index">Stop index</param>
        /// <param name="stop">Stop to set</param>
        /// <remarks>Does not resize the list of stop; just sets the element at the specified index 
        /// disposing an existing stop. Triggers the necessary routes updates. If the specified stop 
        /// is null, calls RemoveStop to remove the stop.
        /// </remarks>
        public void SetStopAt(int index, Stop stop)
        {
            if (stop == null)
                RemoveStop(index);
            else
            {
                if (stops[index] != null)
                    stops[index].Dispose();

                stops[index] = stop;
                stops[index].Label = "" + (char)(65 + index);
                stops[index].ZIndex = 100000 + index;

                UpdateRoute(stop, index - 1, 5);
                UpdateRoute(stop, index, 5);
            }
        }

        /// <summary>
        /// Gets the start way point of a specific route.
        /// </summary>
        /// <param name="p">Route to get the start way point for.</param>
        /// <returns>Start way point</returns>
        public Stop GetStart(Route p)
        {
            return stops[routes.IndexOf(p)];
        }

        /// <summary>
        /// Gets the end way point of a specific route.
        /// </summary>
        /// <param name="p">Route to get the end way point for.</param>
        /// <returns>End way point</returns>
        public Stop GetEnd(Route p)
        {
            return stops[routes.IndexOf(p)+1];
        }

        /// <summary>
        /// Updates a specific route as a result of a stop that has been moved. Passes 
        /// the stop that changed to the route so that it can optimize the route 
        /// calculation (update only the portion that changed).
        /// </summary>
        /// <param name="movedWayPoint">Way point that changed</param>
        /// <param name="idx">Route index</param>
        /// <param name="delay">Optional; update delay</param>
        private void UpdateRoute(WayPoint movedWayPoint, int idx, int? delay = null)
        {
            if (idx >= 0 && idx < routes.Count)
                routes[idx].UpdateRouteForMovingWayPoint(movedWayPoint, delay);

            for (int i = 0; i < stops.Count; ++i)
            {
                if (stops[i] == null)
                    continue;

                if (i == 0)
                    stops[i].Pin.Color = Colors.LightGreen;
                else if (i == stops.Count - 1)
                    stops[i].Pin.Color = Colors.LightCoral;
                else
                    stops[i].Pin.Color = Colors.LightBlue;
            }
        }

        /// <summary>
        /// Convenience; reads or writes the very first stop. By using SetStopAt to 
        /// set the stop, the necessary route updates will be triggered.
        /// </summary>
        public Stop First
        {
            get { return stops.First(); }
            set { SetStopAt(0, value); }
        }

        /// <summary>
        /// Convenience; reads or writes the very last stop. By using SetStopAt to 
        /// set the stop, the necessary route updates will be triggered.
        /// </summary>
        public Stop Last
        {
            get { return stops.Last(); }
            set { SetStopAt(stops.Count - 1, value); }
        }

        /// <summary>
        /// Appends a new stop to the stop list. By using the Last property 
        /// to set this stop, the necessary route updates will be triggered.
        /// </summary>
        /// <param name="stop"></param>
        public void Append(Stop stop)
        {
            if (!Valid)
                throw new InvalidOperationException();

            routes.Add(new Route(this));
            routes[routes.Count - 1].ZIndex = 10*routes.Count;

            stops.Add(null);

            Last = stop;
        }

        /// <summary>
        /// Removes a specific stop.
        /// </summary>
        /// <param name="stop">Stop to remove</param>
        public void Remove(Stop stop)
        {
            int idx = stops.IndexOf(stop);

            if (idx < 0 || idx >= stops.Count)
                throw new ArgumentException("cannot identify stop");

            RemoveStop(idx);
        }

        /// <summary>
        /// Removes a stop at a specific index.
        /// </summary>
        /// <param name="idx">Index of the stop to remove. Must be valid.</param>
        private void RemoveStop(int idx)
        {
            stops[idx].Dispose();

            if (stops.Count > 2)
            {
                stops.RemoveAt(idx);

                for (int i = idx; i < stops.Count; ++i)
                {
                    stops[i].Label = "" + (char)(65 + i);
                    stops[i].ZIndex = 100000 + i;
                }

                if (idx > 0 && idx < stops.Count)
                    routes[idx - 1].MergeWith(routes[idx]);
                else
                    idx = Math.Min(idx, routes.Count);

                routes[idx].Dispose();
                routes.RemoveAt(idx);

                for (int i = idx; i < routes.Count; ++i)
                    routes[i].ZIndex = 10 * i;
            }
            else
            {
                stops[idx] = null;

                routes[0].Dispose();
                routes[0] = new Route(this);
            }
        }

        /// <summary>
        /// Returns the number of stops.
        /// </summary>
        public int StopCount => stops.Count;

        /// <summary>
        /// Checks if there are enough stops to calculate a route.
        /// </summary>
        public bool Valid => StopCount >= 2 && First != null && Last != null;

        /// <summary>
        /// Builds a tool tip text containing the overall information generated out of all routes. 
        /// </summary>
        /// <returns>Tool tip text. Returns an empty string if there is any route with an error.</returns>
        public String GetToolTip()
        {
            if (routes.Count(r => r.HasRoutePolyline) != routes.Count)
                return "";

            var info = new Demo.XrouteService.RouteInfo();

            foreach (var r in routes)
            {
                info.time += r.RouteInfo.time;
                info.distance += r.RouteInfo.distance;
            }

            return Route.FormatRouteToolTip(First.Label, Last.Label, info);
        }

        /// <summary>
        /// Forces a tool tip update on each route.
        /// </summary>
        public void UpdateRouteToolTips()
        {
            routes.ForEach(route => route.UpdateToolTip());
        }
    }
}
