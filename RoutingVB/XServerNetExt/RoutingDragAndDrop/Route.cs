//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.ServiceModel;
using System.Threading;
using System.Diagnostics;
using GisSharpBlog.NetTopologySuite.IO;
using Point = System.Windows.Point;
using XServerNetExt.XRouteServiceReference;
using Ptv.XServer.Demo.UseCases;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Controls.Routing
{
    /// <summary>
    /// Represents a route, defined by two stops and optional via way points.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// The route layer this route belongs to.
        /// </summary>
        private readonly RouteLayer layer;

        /// <summary>
        /// Reference counter for pending update requests.
        /// </summary>
        private int updateRef;

        /// <summary>
        /// The hash of an update request. The hash is used to determine if 
        /// subsequent update requests refer to the same section of the route.
        /// </summary>
        private ulong? updateHash;

        /// <summary>
        /// Flag indicating if a route calculation succeeded.
        /// </summary>
        private bool calculationSucceeded;

        /// <summary>
        /// The id of the route that was processed last. This is used to skip
        /// responses that come in too late.
        /// </summary>
        private int latestIdCalculated;

        /// <summary>
        /// The client to send requests to xRoute.
        /// </summary>
        private XServerNetExt.XRouteServiceReference.XRouteWSClient xroute;

        /// <summary>
        /// A timer used for delaying update requests.
        /// </summary>
        private readonly DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Preview polyline shape.
        /// </summary>
        private readonly RoutePolylineWithDragAndDrop previewPolyline;

        /// <summary>
        /// Final route polyline shape.
        /// </summary>
        private readonly RoutePolylineWithDragAndDrop finalPolyline;

        /// <summary>
        /// The via way points of the route.
        /// </summary>
        private readonly List<Via> Via = new List<Via>();

        /// <summary>
        /// A snapshot of the last route that was successfully calculated. Used on updates 
        /// to request only the portions of the route that was changed.
        /// </summary>
        private RouteSnapshot snapshot = new RouteSnapshot();

        /// <summary>
        /// The base z index of the route.
        /// </summary>
        private int zIndex;

        /// <summary>
        /// A reader for reading the WKBs returned from xRoute.
        /// </summary>
        private readonly WKBReader wkbReader = new WKBReader();

        /// <summary>
        /// lock object for the route client
        /// </summary>
        private readonly Object routeClientLock = new Object();

        /// <summary>
        /// Creates the route.
        /// </summary>
        /// <param name="layer">The route layer the route belongs to.</param>
        public Route(RouteLayer layer, XRouteWSClient client )
        {
            this.layer = layer;
            timer.Tick += Timer_Elapsed;
            finalPolyline = new RoutePolylineWithDragAndDrop(layer, this);
            previewPolyline = new RoutePolylineWithDragAndDrop(layer);
            xroute = client;
        }

        /// <summary>
        /// Gets an enumerable for enumerating the route way points, including the route stops.
        /// </summary>
        private IEnumerable<WayPoint> WayPoints
        {
            get
            {
                yield return Start;

                foreach (var v in Via)
                    yield return v;

                yield return End;
            }
        }

        /// <summary>
        /// Removes a via way point from the route.
        /// </summary>
        /// <param name="wayPoint"></param>
        public void Remove(WayPoint wayPoint)
        {
            // get way point index
            int idx = WayPoints.ToList().IndexOf(wayPoint);

            // remove from via list
            Via.RemoveAt(idx-1);

            // try to update snapshot also, if that is possible
            if (snapshot.Valid(Via.Count + 3))
                snapshot.Indices = snapshot.Indices.RemoveAt(idx);
            else
                snapshot = new RouteSnapshot();
           
            // dispose way point - removes the way point from the map
            wayPoint.Dispose();

            // update z indices of via way points
            UpdateZIndex(ZIndex, false, true);

            // update route, try a delta update for the portion that changed
            UpdateRoute(GetRequestData(idx - 1, idx), null);
        }

        /// <summary>
        /// Merges this route with another route. 
        /// This is usually triggered if a stop is removed.
        /// </summary>
        /// <param name="other"></param>
        public void MergeWith(Route other)
        {
            // invalidate snapshot, force full calculation
            //
            // TODO: this could be optimized. If both routes have a 
            // valid snapshot we could create combined snapshot. The update 
            // section would then be:
            //
            // from = Via.Count (<- the count before other via way points have been inserted)
            // to = from + 1;
            snapshot = new RouteSnapshot();
            
            // transfer other via way points
            Via.AddRange(other.Via);

            // set new owner
            other.Via.ForEach(via => via.Route = this);

            // update the z index of the via way points
            UpdateZIndex(ZIndex, false, true);

            // clear via way points of other route
            other.Via.Clear();
            
            // try to display sort of a preview and trigger update
            UpdateRoutePolylines(Points.Concat(other.Points), "", RouteLayer.ROUTE);
            UpdateRoute(GetRequestData(), null);
        }

        /// <summary>
        /// Reads or writes the base z index.
        /// </summary>
        public int ZIndex
        {
            get { return zIndex; }
            set { UpdateZIndex(value); }
        }

        /// <summary>
        /// Updates the base z index of the route.
        /// </summary>
        /// <param name="newZIndex">New z index</param>
        /// <param name="bForceShape">If set to true, updates the route shapes even if the z index didn't change.</param>
        /// <param name="bForceVia">If set to true, updates the via shapes even if the z index didn't change.</param>
        private void UpdateZIndex(int newZIndex, bool bForceShape = false, bool bForceVia = false)
        {
            if (zIndex != newZIndex || bForceShape)
            {
                // update the route shapes

                var shapes = new[] {  finalPolyline.MapPolyline, previewPolyline.MapPolyline };

                for (int i=0; i<shapes.Length; ++i)
                    Panel.SetZIndex(shapes[i], newZIndex + i);
            }

            if (zIndex != newZIndex || bForceVia)
            {
                // update the via way points

                for (int i = 0; i < Via.Count; ++i)
                    Via[i].ZIndex = 1000 + 1000 * newZIndex + i;
            }

            // finally set new value
            zIndex = newZIndex;
        }

        /// <summary>
        /// Indicates if a route can be calculated - that is, if start and end of the route is defined.
        /// </summary>
        private bool CanCalculate
        {
            get
            {
                return Start != null && End != null;
            }
        }

        /// <summary>
        /// Formats a tool tip for a route.
        /// </summary>
        /// <param name="from">Label of start way point</param>
        /// <param name="to">Label of end way point</param>
        /// <param name="info">Route information</param>
        /// <returns></returns>
        public static String FormatRouteToolTip(String from, String to, RouteInfo info)
        {
            return String.Format("{0} > {1}: {2:0.00}km, {3}", from, to, (double)info.distance / 1000.0, new TimeSpan(0, 0, info.time));
        }

        /// <summary>
        /// Updates the tool tip of the route.
        /// </summary>
        public void UpdateToolTip()
        {
            // if we have a route we can update the tool tip
            if (!HasRoutePolyline) return;

            // begin with tool tip of this route
            finalPolyline.ToolTip = FormatRouteToolTip(Start.Label, End.Label, snapshot.Info);

            // if there a more than two stop, querry the layer for an 'overall tool tip'
            string overall_ToolTip = layer.StopCount > 2 ? layer.GetToolTip() : "";

            // if there is an 'overall tool tip' append it
            if (!String.IsNullOrEmpty(overall_ToolTip))
                finalPolyline.ToolTip += "\n" + overall_ToolTip;
        }

        /// <summary>
        /// Displays a preview for a route. This is triggered by updates 
        /// on the route to provide some sort of early user feedback.
        /// </summary>
        void PreviewRoute()
        {
            if (!CanCalculate)
                // no stops > no preview
                previewPolyline.Reset();
            else
            {
                // Remove an eventually existing RoutePolylineWithDragAndDrop
                finalPolyline.Reset();
                // create a preview out of the route's way points
                previewPolyline.Update(WayPoints.Select(s => s.Point), null, RouteLayer.ROUTE_PREVIEW);

                // force z index update
                UpdateZIndex(ZIndex, true);
            }
        }

        /// <summary>
        /// Checks if the route has a polyline shape - that is, if the route calculation 
        /// successfully finished and there are no pending updates.
        /// </summary>
        public bool HasRoutePolyline
        {
            get
            {
                return updateRef == 0 && calculationSucceeded && Points.GetEnumerator().MoveNext();
            }
        }

        /// <summary>
        /// Reads the start way point.
        /// </summary>
        public Stop Start
        {
            get { return layer.GetStart(this); }
        }

        /// <summary>
        /// Reads the end way point.
        /// </summary>
        public Stop End
        {
            get { return layer.GetEnd(this); }
        }

        /// <summary>
        /// Called when an update begins.
        /// </summary>
        /// <param name="data"></param>
        private void BeginUpdate(RequestData data)
        {
            // reset calculation flag
            calculationSucceeded = false;

            // increment update counter. Store hash on initial update.
            if ((Interlocked.Increment(ref updateRef) == 1) && (data != null))
                updateHash = data.Hash;
        }

        /// <summary>
        /// Called when an update ends.
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="bFromTimer"></param>
        private void EndUpdate(RouteSnapshot resp = null, bool bFromTimer = false)
        {
            // decrement update counter. If there are no pending updates ...
            if (Interlocked.Decrement(ref updateRef) != 0) return;

            // reset update hash
            updateHash = null;

            // if end update has been triggered by the update timer (no matter in what way)
            // other updates will start soon. In this case do no update snapshot and tooltip.

            if (bFromTimer) return;

            snapshot = resp ?? new RouteSnapshot();
            layer.UpdateRouteToolTips();
        }

        /// <summary>
        /// Stops the update timer. 
        /// </summary>
        /// <returns>The request data object associated with the timer, if any.</returns>
        RequestData StopTimer()
        {
            // must read timer as it is set to null below
            var data = timer.Tag as RequestData;

            if (!timer.IsEnabled) return data;

            // stop & reset timer
            timer.Tag = null;
            timer.Stop();

            // trigger end update
            EndUpdate(null, true);

            // return data object
            return data;
        }

        /// <summary>
        /// Start the update timer.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="data"></param>
        void StartTimer(int interval, RequestData data)
        {
            // initialize & start timer
            timer.Interval = TimeSpan.FromMilliseconds(interval);
            timer.Tag = data;
            timer.Start();
        }

        /// <summary>
        /// Handles the Elapsed event of the update timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Elapsed(object sender, EventArgs e)
        {
            // forced stop - returns the request data object associated with the timer
            var data = StopTimer();

            // trigger the update, finally
            UpdateRoute(data, null);
        }

        /// <summary>
        /// Gets the request data including all way points.
        /// </summary>
        /// <returns>Request data object</returns>
        private RequestData GetRequestData()
        {
            return GetRequestData(0, Via.Count + 1);
        }

        /// <summary>
        /// Gets optimized request data for the specified portion of the route that changed.
        /// </summary>
        /// <param name="changesFrom">Index of the way point marking the begin of the route section that changed.</param>
        /// <param name="changesTo">Index of the way point marking the end of the route section that changed.</param>
        /// <returns>Request data object</returns>
        private RequestData GetRequestData(int changesFrom, int changesTo)
        {
            // be sure the indices are in range
            changesFrom = Math.Max(changesFrom, 0);
            changesTo = Math.Min(changesTo, Via.Count + 1);

            // create request data object
            var data = new RequestData(changesFrom, changesTo,
                CanCalculate ? WayPoints.ToArray() : null, updateHash, snapshot);

            // see if snapshot must be invalidated
            if (!snapshot.Valid(Via.Count + 2) || (updateHash.HasValue && updateHash.Value != data.Hash))
                snapshot = new RouteSnapshot();

            // done
            return data;
        }

        /// <summary>
        /// Triggers a (optimized) route update in case a way point has been moved.
        /// </summary>
        /// <param name="movingWayPoint">Way point to move</param>
        /// <param name="delay">Timespan in [ms] to delay the update request.</param>
        public void UpdateRouteForMovingWayPoint(WayPoint movingWayPoint, int? delay = null)
        {
            if (movingWayPoint == null)
                UpdateRoute(GetRequestData(), delay);
            else
            {
                int idx = WayPoints.ToList().IndexOf(movingWayPoint);
                UpdateRoute(GetRequestData(idx - 1, idx + 1), delay);
            }
        }

        /// <summary>
        /// Triggers a route update for the specified request data.
        /// </summary>
        /// <param name="data">Request data object.</param>
        /// <param name="delay">Timespan in [ms] to delay the update request.</param>
        private void UpdateRoute(RequestData data, int? delay)
        {
            // begin update, provide some user feedback and 
            // stop any update timer that might be active

            BeginUpdate(data);
            PreviewRoute();
            StopTimer();

            if (delay.HasValue)
                // start update timer if delay was specified
                StartTimer(delay.Value, data);
            else
            {
                if (data == null || !data.Valid)
                {
                    // clear route if request is invalid
                    UpdateRoutePolylines(null, null, null);
                    EndUpdate();
                }
                else
                {
                    // fire request in a separate thread as this operation takes rather long
                    var threadStart = new ParameterizedThreadStart(SendRequest);
                    new Thread(threadStart).Start(data);
                }
            }
        }

        /// <summary>
        /// Factory for the  of the xServer generated client class granting access to the xRoute server.
        /// If xServers are used on an Azure environment, authentication data has to be integrated when
        /// requests are made.
        /// </summary>
        /// <param name="xUrl">The xServer base url. </param>
        /// <returns></returns>
        public static XRouteWSClient CreateXRouteClient(string xUrl, string user, string password)
        {
            string completeXServerUrl = XServerUrl.Complete(xUrl, "XRoute");

            var binding = new BasicHttpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 0, 30),
                SendTimeout = new TimeSpan(0, 0, 30),
                OpenTimeout = new TimeSpan(0, 0, 30),
                CloseTimeout = new TimeSpan(0, 0, 30),
                MaxReceivedMessageSize = int.MaxValue
            };

            var endpoint = new EndpointAddress(completeXServerUrl);
            var client = new XRouteWSClient(binding, endpoint);

            if (!XServerUrl.IsXServerInternet(completeXServerUrl)) return client;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            client.ClientCredentials.UserName.UserName = user;
            client.ClientCredentials.UserName.Password = password;

            // increase message size
            (client.Endpoint.Binding as BasicHttpBinding).MaxReceivedMessageSize = 4 << 20;
            (client.Endpoint.Binding as BasicHttpBinding).ReaderQuotas.MaxBytesPerRead = 4 << 20;

            // set timeouts
            client.Endpoint.Binding.SendTimeout = TimeSpan.FromMilliseconds(5000);
            client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMilliseconds(10000);

            return client;
        }

        /// <summary>
        /// Sends a route request to xRoute
        /// </summary>
        /// <param name="param">The parameter, must be a request data object</param>
        public void SendRequest(object param)
        {
            // associated data object
            var data = param as RequestData;

            lock (routeClientLock)
            {
                // abort any previous request that might be active
                //if (xroute != null)
                //    xroute.Abort();

                // finally trigger an asynchronous route calculation with the specified request data
                xroute.BegincalculateRoute(data.RequestWaypoints, null, null,
                    new ResultListOptions { polygon = true }, new CallerContext
                    {
                        wrappedProperties = new[] {
                        new CallerContextProperty { key = "CoordFormat", value = "PTV_SMARTUNITS" },
                        new CallerContextProperty { key = "ResponseGeometry", value = "WKB" }
                    }}, RouteCalculated, data);
            }
        }

        /// <summary>
        /// Updates the route polylines.
        /// </summary>
        /// <param name="points">Points of the route polyline</param>
        /// <param name="toolTip">Tool tip</param>
        /// <param name="style">Display style of the route</param>
        public void UpdateRoutePolylines(IEnumerable<Point> points, String toolTip, ShapeStyle style)
        {
            finalPolyline.Update(points, toolTip, style);
            previewPolyline.Reset();
        }

        /// <summary>
        /// Handles the route calculation results.
        /// </summary>
        /// <param name="result">Route calculation results.</param>
        public void RouteCalculated(IAsyncResult result)
        {
            layer.Map.Dispatcher.Invoke((Action)(() =>
            {
                // get request data and initialize a snapshot
                var req = (RequestData)result.AsyncState;
                var routeSnapshot = new RouteSnapshot();

                try
                {
                    // skip this response if there are newer responses 
                    // that have already been processed.
                    if (req.Id < latestIdCalculated)
                        return;

                    // store request id
                    latestIdCalculated = req.Id;

                    // get route result
                    var route = xroute.EndcalculateRoute(result);

                    // create snapshot out of route result
                    routeSnapshot = RouteSnapshot.FromXRoute(
                        route.wrappedStations.Select(w => w.polyIdx).ToArray(),
                        wkbReader.Read(route.polygon.wkb).Coordinates.Select(p => p.PtvSmartUnitsToWgs84()).ToArray(),
                        route.info
                    );

                    // merge snapshot with request data. If only a part of the route has 
                    // been updated, this step creates the 'full route information'
                    routeSnapshot.MergeWithRequest(req);

                    var toolTip = FormatRouteToolTip(req.Labels.First(), req.Labels.Last(), routeSnapshot.Info);
                    UpdateRoutePolylines(routeSnapshot.Points, toolTip, RouteLayer.ROUTE);

                    // if there are no pending updates also set calculationSucceeded flag
                    // and store the polygon indices of the via way points
                    if (updateRef == 1)
                    {
                        calculationSucceeded = routeSnapshot.Valid(Via.Count + 2);
                        if (calculationSucceeded)
                        {
                            for (int i = 0; i < routeSnapshot.Indices.Length; ++i)
                                if ((i == 0) || (i == routeSnapshot.Indices.Length - 1))
                                {
                                    if (req.OriginalWayPoints[i] is Stop)
                                        (req.OriginalWayPoints[i] as Stop).LinkPoint = routeSnapshot.Points[Math.Min(routeSnapshot.Points.Count() - 1, routeSnapshot.Indices[i])];
                                }
                                else
                                    Via[i - 1].PolyIndex = routeSnapshot.Indices[i];
                        }
                    }
                }
                catch (Exception e)
                {
                    // Response processing failed. If there are no pending updates we create an error polyline here.
                    // We also invalidate the route snapshot and set the exception message as the tool tip.

                    if (updateRef == 1)
                    {
                        Func<WaypointDesc, Point> WaypointToPoint = 
                            wp => wp.wrappedCoords[0].point.PtvSmartUnitsToWgs84();

                        UpdateRoutePolylines(req.Waypoints.Select(WaypointToPoint), e.Message, RouteLayer.ROUTE_ERROR);

                        routeSnapshot = new RouteSnapshot();
                    }
                }

                // another update done
                EndUpdate(routeSnapshot);

            }), DispatcherPriority.Send, null);
        }


        /// <summary>
        /// Determines the correct insert position of the specified via way point.
        /// </summary>
        /// <param name="via">The via way point to find an insert index for.</param>
        /// <returns>The index where to insert the way point. Valid in every case.</returns>
        private int GetViaIndex(Via via)
        {
            // easy, if there are no way points yet.
            if (Via.Count == 0)
                return 0;

            Point p;
            int polyIdx, idx = 0;

            // get the nearest point on the route. Also set its 
            // index as the polygon index of the via.
            Points.GetNearestPoint(via.Point, out p, out polyIdx);
            via.PolyIndex = polyIdx;

            // Use the polygon index to find the correct insert position.
            foreach (var v in Via)
                if (polyIdx > v.PolyIndex)
                    idx++;
                else
                    break;

            // return insert position
            return idx;
        }

        /// <summary>
        /// Inserts a new via way point and attaches a Drag&amp;Drop operation to it.
        /// </summary>
        /// <param name="style">Display style of the way point</param>
        /// <param name="initialPosition">Initial position</param>
        /// <param name="position">Current position</param>
        public void BeginDragVia(ShapeStyle style, Point initialPosition, Point? position = null)
        {
            // the via location in world coordinates
            var pointOnMap = layer.ToWorld(layer.Map.PointFromScreen(initialPosition));

            // insert the via way point then attach the drag and drop operation to it.
            InsertVia(pointOnMap, style).BeginDrag(initialPosition, position);
        }

        /// <summary>
        /// Inserts a via way point at the specified position.
        /// </summary>
        /// <param name="p">Location of the via way point</param>
        /// <param name="style">Display style of the via way point</param>
        /// <returns>The newly created via way point</returns>
        private Via InsertVia(Point p, ShapeStyle style)
        {
            // create the way point and set owner
            var via = new Via(layer, p, style) { Route = this };

            // find the index where to insert the via way point and insert it there
            Via.Insert(GetViaIndex(via), via);

            // try to update the snapshot if that is possible
            if (snapshot.Valid(Via.Count + 1))
                snapshot.Indices = snapshot.Indices.Insert(Via.IndexOf(via) + 1, via.PolyIndex);
            else
                snapshot = new RouteSnapshot();

            // force via way point z index update
            UpdateZIndex(ZIndex, false, true);

            // return the newly created via way point
            return via;
        }

        /// <summary>
        /// Disposes the route - removes it along with the via way points from the map.
        /// </summary>
        public void Dispose()
        {
            finalPolyline.Reset();
            previewPolyline.Reset();

            Via.ForEach(via => via.Dispose());
        }

        /// <summary>
        /// Reads the points of the latest route available.
        /// </summary>
        public IEnumerable<Point> Points { get { return finalPolyline.Points ?? new PointCollection(); } }

        /// <summary>
        /// Reads the polyline's tool tip.
        /// </summary>
        public String ToolTip { get { return finalPolyline.ToolTip; } }

        /// <summary>
        /// Reads the route information of the latest route available.
        /// </summary>
        public RouteInfo RouteInfo { get { return snapshot.Info; } }
    }

    /// <summary>
    /// Encapsulates a route polyline. 
    /// </summary>
    /// <remarks>
    /// This class is mainly used to encapsulate a basic MapPolyline and to 
    /// provide some automation for adding / removing the polyline to / from a 
    /// shape layer. 
    /// </remarks>
    public class RoutePolylineWithDragAndDrop : RoutePolyline
    {
        /// <summary>
        /// The route the polyline belongs to.
        /// </summary>
        private readonly Route route;

        /// <summary>
        /// Create a route polyline.
        /// </summary>
        /// <param name="layer">The layer the route and the polyline belongs to.</param>
        /// <param name="route">The route the polyline belongs to</param>
        public RoutePolylineWithDragAndDrop(RouteLayer layer, Route route = null) : base(layer)
        {
            this.route = route;
        }

        /// <summary>
        /// Polyline has been dragged.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event arguments</param>
        private void Drag(Object sender, DragDropEventArgs args)
        {
            // cancel the polyline drag and drop, inject a via way point 
            // and continue to dragging&dropping this way point instead
            route.BeginDragVia(RouteLayer.VIA_STYLE, args.DragPosition, args.Position);
            args.Cancel = true;
        }

        private bool PointsExists()
        {
            return (MapPolyline.Points == null) || !MapPolyline.Points.Any();
        }

        /// <summary>
        /// This method updates the route polyline, especially by providing a new geometry.
        /// The DragAndDrop handling is corrected.
        /// </summary>
        /// <param name="points">Points defining the new geometry for the polyline.</param>
        /// <param name="toolTip">Tool tip of the hole polyline.</param>
        /// <param name="style">Display style of the polyline.</param>
        public void Update(IEnumerable<Point> points, String toolTip, ShapeStyle style)
        {
            if ((route != null) && !PointsExists())
            {
                // remove drag and drop behavior
                InProcessDragDropBehavior.RemoveDragHandler(MapPolyline, Drag);
                InProcessDragDropBehavior.SetEnableDragDrop(MapPolyline, true);
                InProcessDragDropBehavior.RemoveDragHandler(AnimatedPolyline, Drag);
                InProcessDragDropBehavior.SetEnableDragDrop(AnimatedPolyline, true);
            }
            Dispose();

            Points = points;
            ToolTip = toolTip;
            Color = (style == null) ? Colors.Black : style.Color;

            if ((route == null) || PointsExists()) return;

            // add drag and drop behavior
            InProcessDragDropBehavior.SetEnableDragDrop(MapPolyline, true);
            InProcessDragDropBehavior.AddDragHandler(MapPolyline, Drag);

            InProcessDragDropBehavior.SetEnableDragDrop(AnimatedPolyline, true);
            InProcessDragDropBehavior.AddDragHandler(AnimatedPolyline, Drag);
        }

        /// <summary>
        /// Previously set properties, especially the adding of the polyline into the shape layer, are reset. 
        /// </summary>
        public void Reset()
        {
            Update(null, null, null);
        }
    }

    /// <summary>
    /// Encapsulates the snapshot taken after a route calculation.
    /// </summary>
    public class RouteSnapshot
    {
        /// <summary>
        /// The polyline indices of the way points.
        /// </summary>
        /// 
        public int[] Indices { get; set; }

        /// <summary>
        /// The route polyline
        /// </summary>
        public Point[] Points { get; set; }

        /// <summary>
        /// The route information
        /// </summary>
        public RouteInfo Info { get; set; }

        /// <summary>
        /// Creates a snapshot, invalid by default.
        /// </summary>
        public RouteSnapshot()
        {
        }

        /// <summary>
        /// Creates a snapshot out of the given route information.
        /// </summary>
        /// <param name="indices">Polyline indices</param>
        /// <param name="points">Route polyline</param>
        /// <param name="info">Route information</param>
        private RouteSnapshot(IEnumerable<int> indices, IEnumerable<Point> points, RouteInfo info)
        {
            Indices = indices.ToArray();
            Points = points.ToArray();
            Info = info;
        }

        /// <summary>
        /// Checks if the snapshot with the exception of its route information is valid. An update 
        /// which calculates only a portion of a route stores snapshots of route parts that remain 
        /// stable. This parts will have invalid route information objects by default and need 
        /// special treatment.
        /// </summary>
        /// <param name="nWayPoints">Number of way points. Null if not to be checked.</param>
        /// <returns>True if snapshot is valid, false otherwise.</returns>
        private bool ValidExceptRouteInfo(int? nWayPoints = null)
        {
            // check indices and points only
            return Indices != null && Points != null && Indices.Length > 1 && 
                (nWayPoints == null || Indices.Length == nWayPoints.Value);
        }

        /// <summary>
        /// Checks if the snapshot is valid.
        /// </summary>
        /// <param name="nWayPoints">Number of way points. Null if not to be checked.</param>
        /// <returns>True if snapshot is valid, false otherwise.</returns>
        public bool Valid(int? nWayPoints = null)
        {
            // use ValidExceptRouteInfo for the check and check route information also
            return ValidExceptRouteInfo(nWayPoints) && Info != null;
        }

        /// <summary>
        /// Extracts a portion of the snapshot.
        /// </summary>
        /// <param name="from">Starting index</param>
        /// <returns>Snapshot portion</returns>
        public RouteSnapshot Right(int from)
        {
            if (!Valid() || from >= Indices.Length - 1)
                return new RouteSnapshot();

            return new RouteSnapshot(
                Indices.Skip(from).Select(i => i - Indices[from]), 
                Points.Skip(Indices[from]),
                null
            );
        }

        /// <summary>
        /// Extracts a portion of the snapshot.
        /// </summary>
        /// <param name="to">Ending index</param>
        /// <returns>Snapshot portion</returns>
        public RouteSnapshot Left(int to)
        {
            if (!Valid() || to < 1)
                return new RouteSnapshot();

            return new RouteSnapshot(
                Indices.Take(to + 1),
                Points.Take(Indices[to]),
                null
            );
        }

        /// <summary>
        /// Create a snapshot out of xRoute response data.
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="points"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static RouteSnapshot FromXRoute(int[] indices, System.Windows.Point[] points, RouteInfo info)
        {
            // indices must be adopted to let the last index point right behind the route
            if (indices[indices.Length - 1] == points.Length - 1)
                indices[indices.Length - 1]++;

            // assert last index is correct
            Debug.Assert(indices[indices.Length - 1] == points.Length);

            // create snapshot
            return new RouteSnapshot(indices, points, info);
        }
        
        /// <summary>
        /// Concatenates two snapshots.
        /// </summary>
        /// <param name="other">Snapshot to concat.</param>
        /// <returns>Concatenated snapshots.</returns>
        private RouteSnapshot Concat(RouteSnapshot other)
        {
            if (!ValidExceptRouteInfo())
                return other;

            if (!other.ValidExceptRouteInfo())
                return this;

            return new RouteSnapshot(
                Indices.Concat(other.Indices.Skip(1).Select(i => i + Points.Length)),
                Points.Concat(other.Points),
                Info ?? other.Info
            );
        }

        /// <summary>
        /// Merges a snapshot, usually created out of a xRoute response, 
        /// with those route parts that have not been updated.
        /// </summary>
        /// <param name="req"></param>
        public void MergeWithRequest(RequestData req)
        {
            // concatenate
            var tmp = req.Left.Concat(this).Concat(req.Right);

            // store concatenated results
            Indices = tmp.Indices;
            Points = tmp.Points;
            Info = tmp.Info;
        }
    }

    /// <summary>
    /// Encapsulates the data needed to enable the Route class to 
    /// request a route from xRoute server.
    /// </summary>
    public class RequestData
    {
        /// <summary>
        /// static id counter.
        /// </summary>
        private static int NextId = 1;

        /// <summary>
        /// Creates and initializes a RequestData object.
        /// </summary>
        /// <param name="from">Index of the way point marking the begin of the route section that is going to change.</param>
        /// <param name="to">Index of the way point marking the end of the route section that is going to change.</param>
        /// <param name="wayPoints">The route way points</param>
        /// <param name="updateHash">The hash of previous update operations</param>
        /// <param name="snapshot">The snapshot of the current route</param>
        public RequestData(int from, int to, WayPoint[] wayPoints, ulong? updateHash, RouteSnapshot snapshot)
        {
            // unique, consecutive id of the data object
            Id = NextId++;

            // make update hash out of the way point indices
            Hash = ((ulong)from << 32) | (ulong)to;

            // initialize snapshots objects
            Left = new RouteSnapshot();
            Right = new RouteSnapshot();

            if (wayPoints == null) return;

            // if there are way points ...
            OriginalWayPoints = wayPoints;

            // get and store the full list of way points and its labels
            RequestWaypoints = Waypoints = wayPoints.Select(s => s.Waypoint).ToArray();
            Labels = wayPoints.Select(s => (s is Stop) ? (s as Stop).Label : "").ToArray();

            // if the specified snapshot is valid and the hash matches the hash of previous
            // updates then we only need to update a portion of the route. 
            //
            // If the hashes don't match we have multiple pending updates that each update a 
            // different section of the route. In this case we use this data object to force 
            // a full update.
            if (snapshot.Valid(wayPoints.Length + 2) && (!updateHash.HasValue || updateHash.Value == Hash))
            {
                // get the way points of the route portion to be updated
                RequestWaypoints = Waypoints.Take(to + 1).Skip(@from).ToArray();

                // beginning and end must be a stop - remove any via type set
                RequestWaypoints.First().viaType = null;
                RequestWaypoints.Last().viaType = null;

                // get the parts of the route that remain stable
                Right = snapshot.Right(to);
                Left = snapshot.Left(@from);
            }
        }

        /// <summary>
        /// Gets the id of the route request.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the key of the update operation.
        /// </summary>
        public ulong Hash { get; private set; }

        /// <summary>
        /// Gets the labels of all way points (snapshot)
        /// </summary>
        public String[] Labels { get; set; }

        /// <summary>
        /// Gets the input way points.
        /// </summary>
        public WayPoint[] OriginalWayPoints { get; set; }

        /// <summary>
        /// Gets the descriptions of all way points.
        /// </summary>
        public WaypointDesc[] Waypoints { get; set; }

        /// <summary>
        /// Gets the way point descriptions for the route portion to be updated
        /// </summary>
        public WaypointDesc[] RequestWaypoints { get; set; }

        /// <summary>
        /// Gets the snapshot of the left rout part that remains stable.
        /// </summary>
        public RouteSnapshot Left { get; set; }

        /// <summary>
        /// Gets the snapshot of the right route part that remains stable.
        /// </summary>
        public RouteSnapshot Right { get; set; }

        /// <summary>
        /// Determines if this data object is valid. 
        /// It is if there are enough way points to calculate a route.
        /// </summary>
        public bool Valid
        {
            get { return RequestWaypoints != null && RequestWaypoints.Length > 1; }
        }
    }
}
