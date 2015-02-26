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
using System.Windows;
using System.Windows.Input;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Canvases;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ptv.XServer.Controls.Map
{
    namespace Layers.Untiled
    {
        /// <summary>
        /// A layer displaying arbitrary layer elements taken from xMap Server. Implements a tool tip mechanism based 
        /// on the object information returned by xMap.
        /// </summary>
        /// <remarks>
        /// XMapLayer is an untiled layer that requests map images from xMap Server which it displays on the map. The 
        /// layer also evaluates object information returned from the server, transforming the object information into 
        /// tool tips. It is up to the caller to configure the layer elements in <see cref="CustomXMapLayers" /> to 
        /// generate object information.
        ///
        /// XMapLayer uses some basic hit testing for finding tool tips and it displays the descriptions returned from xMap 
        /// Server as they are. When using XMapLayer you may want to look at:
        ///
        /// - <see cref="ToolTipHitTest"/> implements the basic hit test on <see cref="xserver.LayerObject"/> information 
        ///   objects. Elements within a range of 10 pixels (measured taking either the reference point or the full response 
        ///   geometry into account - depending on what actually is available) qualify for tool tip display. Depending on the 
        ///   xMap content requested you may want to change this behavior. <see cref="ToolTipHitTest"/> can be overridden in 
        ///   derived classes.
        /// 
        /// - <see cref="GetToolTipFromLayerObject" /> transforms a <see cref="xserver.LayerObject"/> information object into 
        ///   a tool tip text. By default, <see cref="GetToolTipFromLayerObject" /> returns the xMap Server description as it 
        ///   is. Usually this description needs further formatting; <see cref="GetToolTipFromLayerObject" /> can be overridden 
        ///   in derived classes.
        /// </remarks>
        public class XMapLayer : UntiledLayer
        {
            /// <summary>the map itself</summary>
            private UserControl map;

            /// <summary>main map canvas</summary>
            private MapCanvas canvas;

            /// <summary>xMap object information</summary>
            private xserver.ObjectInfos[] objectInfos;

            /// <summary>xMap image size</summary>
            private Size imageSize;

            /// <summary>a timer for popping up the tool tips</summary>
            private readonly DispatcherTimer toolTipTimer = new DispatcherTimer();

            /// <summary>active tool tip, may be null</summary>
            private ToolTip toolTip;

            /// <summary>
            /// Creates and initializes the xMap layer. Be sure to specify a xMap profile and the 
            /// layers to be requested in the properties profile and CustomXMapLayers.
            /// </summary>
            /// <param name="name">Layer name</param>
            /// <param name="url">xMap Server URL</param>
            /// <param name="user">xMap Server user</param>
            /// <param name="password">xMap Server password</param>
            public XMapLayer(string name, string url, string user, string password)
                : base(name)
            {
                InitializeFactory(CanvasCategory.Content, CreateCanvas);

                var provider = new ExtendedXMapTiledProvider(url, user, password);
                provider.MapUdpate += UdpateOjectInfos;

                UntiledProvider = provider;
                ToolTipDelay = ToolTipService.GetInitialShowDelay(new Canvas());
                toolTipTimer.Tick += ShowToolTip;

                // default - just return the description
                GetToolTipFromLayerObject = o => o.descr;
            }

            /// <summary>
            /// Property delegate. Reads or writes the xMap profile.
            /// </summary>
            public String Profile
            {
                get { return (UntiledProvider as ExtendedXMapTiledProvider).CustomProfile; }
                set { (UntiledProvider as ExtendedXMapTiledProvider).CustomProfile = value; }
            }

            /// <summary>
            /// Property delegate. Reads or writes the set of custom xMap Server layers.
            /// </summary>
            public IEnumerable<xserver.Layer> CustomXMapLayers
            {
                get { return (UntiledProvider as ExtendedXMapTiledProvider).CustomXMapLayers; }
                set { (UntiledProvider as ExtendedXMapTiledProvider).CustomXMapLayers = value; }
            }

            /// <summary>
            /// Property delegate. Reads or writes the set of custom Caller Contexts.
            /// </summary>
            public IEnumerable<xserver.CallerContextProperty> CustomCallerContextProperties
            {
                get { return (UntiledProvider as ExtendedXMapTiledProvider).CustomCallerContextProperties; }
                set { (UntiledProvider as ExtendedXMapTiledProvider).CustomCallerContextProperties = value; }
            }

            /// <summary>
            /// Indicates the marker object is a ballon, so the hit-box is y-shifted
            /// </summary>
            public bool MarkerIsBalloon { get; set; }

            /// <summary>
            /// Canvas factory. Sort of overridden; also used to store the main map and its canvas.
            /// </summary>
            /// <param name="mapView"></param>
            /// <returns></returns>
            private MapCanvas CreateCanvas(MapView mapView)
            {
                return
                    canvas =
                        new UntiledCanvas(mapView, UntiledProvider)
                        {
                            MaxRequestSize = MaxRequestSize,
                            MinLevel = MinLevel
                        };
            }

            /// <inheritdoc/>
            public override void AddToMapView(MapView mapView)
            {
                if (mapView != null && mapView.Name == "Map")
                {
                    map = mapView;
                    map = map.FindAncestor<WpfMap>();
                    map.MouseLeave += HandleMouseLeaveEvent;
                    map.MouseEnter += HandleMouseEnterEvent;
                    map.PreviewMouseMove += HandleMouseEnterEvent;
                }

                base.AddToMapView(mapView);
            }


            /// <inheritdoc/>
            public override void RemoveFromMapView(MapView mapView)
            {
                if (mapView != null && mapView.Name == "Map")
                {
                    map = mapView;
                    map = map.FindAncestor<WpfMap>();
                    map.MouseLeave -= HandleMouseLeaveEvent;
                    map.MouseEnter -= HandleMouseEnterEvent;
                    map.PreviewMouseMove -= HandleMouseEnterEvent;
                }

                base.RemoveFromMapView(mapView);
            }

            private void HandleMouseEnterEvent(object sender, MouseEventArgs e)
            {
                ToolTipUpdate(e);
            }

            private void HandleMouseLeaveEvent(object sender, MouseEventArgs e)
            {
                ToolTipUpdate();
            }

            /// <summary>
            /// Tests if the cursor position changed. If so, stores the new position and triggers a tool tip update.
            /// </summary>
            /// <param name="e">Event arguments</param>
            private void ToolTipUpdate(MouseEventArgs e = null)
            {
                Point? p = e == null ? null : (Point?) e.GetPosition(map);

                // test if position has changed
                if ((LatestPosition.HasValue == p.HasValue) &&
                    (!p.HasValue || ((LatestPosition.Value - p.Value).Length <= 1e-4)))
                    return;

                // clear previous tool tip
                ClearToolTip();

                const MouseButtonState state = MouseButtonState.Released;

                // trigger update, if not mouse button is pressed
                if (Mouse.LeftButton == state && Mouse.MiddleButton == state && Mouse.RightButton == state)
                    DelayedToolTipUpdate(LatestPosition = p);
            }

            /// <summary>
            /// Stores the latest known position handled by UpdatePosition.
            /// </summary>
            private Point? LatestPosition { get; set; }

            /// <summary>
            /// Triggers a tool tip update
            /// </summary>
            /// <param name="p"></param>
            private bool DelayedToolTipUpdate(Point? p)
            {
                if (canvas == null)
                    return false;

                // flag indicating if a tool tip is to be shown. 
                // Initially, image and specified position must be valid.
                bool hasToolTip = canvas.IsVisible && HasImage && p.HasValue && ToolTipDelay >= 0;

                if (hasToolTip)
                {
                    // now test if there are any 'tool tip able' layer objects. Set IsHitTestVisible accordingly.
                    Image.IsHitTestVisible = LatestPosition.HasValue && HasToolTips(LatestPosition.Value);

                    // if a hit test on the map returns the image of this layer, we have to show a tool tip
                    hasToolTip = Image.IsHitTestVisible && map.HitTest(p.Value) == Image;
                }

                // disable or enable the tooltip timer, according to hasToolTip

                toolTipTimer.Stop();

                if (hasToolTip)
                {
                    toolTipTimer.Interval = TimeSpan.FromMilliseconds(ToolTipDelay);
                    toolTipTimer.Start();
                }

                return hasToolTip;
            }

            /// <summary>
            /// Reads or writes the value (in [ms]) to delay tool tip display.
            /// </summary>
            public int ToolTipDelay { get; set; }

            /// <summary>
            /// Event handler: Tool tip timer has elapsed, tool tip is to be shown.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ShowToolTip(object sender, EventArgs e)
            {
                // stop the timer and clear any previous tool tips.
                toolTipTimer.Stop();
                ClearToolTip();

                // get the tool tip texts
                string[] toolTips = GetToolTips(Mouse.GetPosition(map)).ToArray();

                // if texts are valid
                if (toolTips.Length <= 0) return;

                // build tool tip text
                String toolTipString = toolTips.Aggregate("", (current, tt) => current + (tt + "\n"));

                // create and show tool tip
                toolTip = new ToolTip
                {
                    Content = toolTipString.Substring(0, toolTipString.Length - 1),
                    IsOpen = true
                };
            }

            /// <summary>
            /// Updates the object information. See comments on <see cref="Ptv.XServer.Controls.Map.TileProviders.MapUpdateDelegate"/>.
            /// </summary>
            /// <param name="xServerMap">Map object of xServer</param>
            /// <param name="requestedSize">Requested image size.</param>
            public void UdpateOjectInfos(xserver.Map xServerMap, Size requestedSize)
            {
                map.Dispatcher.Invoke((Action) (() =>
                {
                    ClearToolTip();

                    objectInfos = xServerMap != null ? xServerMap.wrappedObjects : null;
                    imageSize = requestedSize;
                }), DispatcherPriority.Send, null);
            }

            /// <summary>
            /// Checks if the layer has a valid map image.
            /// </summary>
            public bool HasImage
            {
                get { return canvas.Children.Count == 1 || canvas.Children[0] is Image; }
            }

            /// <summary>
            /// Tries to get the layer's current map image.
            /// </summary>
            private Image Image
            {
                get { return HasImage ? canvas.Children[0] as Image : null; }
            }

            /// <summary>
            /// Hit tests the object informations and returns those 
            /// matching layer objects with a valid tool tip string.
            /// </summary>
            /// <param name="p">Point to test</param>
            /// <returns>Matching layer objects</returns>
            private IEnumerable<xserver.LayerObject> GetLayerObjects(Point p)
            {
                if (!HasImage || objectInfos == null || objectInfos.Length <= 0) yield break;

                p = new Point(p.X*imageSize.Width/map.ActualWidth, p.Y*imageSize.Height/map.ActualHeight);
                foreach (
                    var layerObject in
                        ToolTipHitTest(objectInfos, p)
                            .Where(layerObject => !String.IsNullOrEmpty(GetToolTipFromLayerObject(layerObject))))
                    yield return layerObject;
            }

            /// <summary>
            /// Hit tests the given layer objects.
            /// </summary>
            /// <param name="infos">Object information to hit test.</param>
            /// <param name="p">Point to test</param>
            /// <returns>Matching layer objects.</returns>
            /// <remarks>The default implementation simply returns objects within a range of 10 pixels of the given point.</remarks>
            protected virtual IEnumerable<xserver.LayerObject> ToolTipHitTest(IEnumerable<xserver.ObjectInfos> infos,
                Point p)
            {
                int hitradius = 10;
                int yOffset = MarkerIsBalloon ? hitradius / 2 : 0;

                foreach (var info in infos)
                {
                    if (info.wrappedObjects == null || info.wrappedObjects.Length <= 0)
                        continue;

                    foreach (var o in info.wrappedObjects)
                    {
                        bool isMatch = false;

                        if (o.geometry != null && o.geometry.pixelGeometry != null)
                        {
                            var lineString = o.geometry.pixelGeometry as xserver.PlainLineString;
                            if (lineString != null && lineString.wrappedPoints != null)
                            {
                                if (lineString.wrappedPoints.Length > 1)
                                {
                                    isMatch =
                                        PointInRangeOfLinestring(
                                            lineString.wrappedPoints.Select(pp => new Point(pp.x, pp.y)).ToArray(), p,
                                            hitradius);
                                }
                                else if (lineString.wrappedPoints.Length == 1)
                                {
                                    isMatch =
                                        (p - new Point(lineString.wrappedPoints[0].x, lineString.wrappedPoints[0].y))
                                            .Length <= hitradius;
                                }
                            }
                        }

                        // ReSharper disable once RedundantAssignment
                        if (isMatch |= (p - new Point(o.pixel.x, o.pixel.y - yOffset)).Length <= hitradius)
                            yield return o;
                    }
                }
            }

            /// <summary>
            /// Checks whether a point is near a line string.
            /// </summary>
            /// <param name="lineString">The point array.</param>
            /// <param name="p">The point to check.</param>
            /// <param name="range">The distance.</param>
            /// <returns></returns>
            public static bool PointInRangeOfLinestring(Point[] lineString, Point p, double range)
            {
                double range2 = range*range; // square of range

                for (int i = 0; i < lineString.Length - 1; i++)
                {
                    Point c = ClosestPointOnSegment(lineString[i], lineString[i + 1], p);
                    var d = new Point(c.X - p.X, c.Y - p.Y); // distance vector

                    if (d.X*d.X + d.Y*d.Y <= range2)
                        return true;
                }

                return false;
            }

            /// <summary>
            /// Gets the closest point on a segment.
            /// </summary>
            /// <param name="a">The first point of the segment.</param>
            /// <param name="b">The second point of the segment.</param>
            /// <param name="p">The point to check.</param>
            /// <returns></returns>
            public static Point ClosestPointOnSegment(Point a, Point b, Point p)
            {
                var d = new Point(b.X - a.X, b.Y - a.Y);
                double number = (p.X - a.X)*d.X + (p.Y - a.Y)*d.Y;

                if (number <= 0.0)
                    return a;

                double denom = d.X*d.X + d.Y*d.Y;

                return (number >= denom) ? b : new Point(a.X + (number/denom)*d.X, a.Y + (number/denom)*d.Y);
            }

            /// <summary>
            /// Gets a tool tip for a LayerObject.
            /// </summary>
            /// <param name="o">LayerObject to get the tool tip for.</param>
            /// <returns>Tool tip string.</returns>
            /// <remarks>The default implementation returns the description of the layer object (as is).</remarks>
            public Func<xserver.LayerObject, string> GetToolTipFromLayerObject;

            /// <summary>
            /// Checks if there are tool tips for a given position.
            /// </summary>
            /// <param name="p">Position to check.</param>
            /// <returns>True, if there are tool tips. False otherwise.</returns>
            private bool HasToolTips(Point p)
            {
                return GetLayerObjects(p).GetEnumerator().MoveNext();
            }

            /// <summary>
            /// Determines the tool tip texts for a given position
            /// </summary>
            /// <param name="p">Position to get the tool tips for.</param>
            /// <returns>Tool tip texts.</returns>
            private IEnumerable<String> GetToolTips(Point p)
            {
                return
                    GetLayerObjects(p)
                        .Select(x => (GetToolTipFromLayerObject != null) ? GetToolTipFromLayerObject(x) : x.descr);
            }

            /// <summary>
            /// Removes the latest tool tip created by this layer.
            /// </summary>
            private void ClearToolTip()
            {
                if (toolTip == null)
                    return;

                // close tool tip
                toolTip.IsOpen = false;
                toolTip = null;
            }
        }

        /// <summary>
        /// Provides extensions used by XMapLayer.
        /// </summary>
        public static class XMapLayerExtensions
        {
            /// <summary>
            /// Gets the parent of a depedency object
            /// </summary>
            /// <param name="obj">The dependency object to get the parent for</param>
            /// <returns>Parent of the dependency object</returns>
            public static DependencyObject GetParent(this DependencyObject obj)
            {
                if (obj == null)
                    return null;

                var ce = obj as ContentElement;

                if (ce == null) return VisualTreeHelper.GetParent(obj);

                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                var fce = ce as FrameworkContentElement;

                return fce != null ? fce.Parent : null;
            }

            /// <summary>
            /// Finds a specific ancestor of a dependency object.
            /// </summary>
            /// <typeparam name="T">Type to lookup</typeparam>
            /// <param name="obj">Dependency object to find the ancestor for.</param>
            /// <returns>Found ancestor or null.</returns>
            public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
            {
                while (obj != null)
                {
                    var objTest = obj as T;

                    if (objTest != null)
                        return objTest;

                    obj = GetParent(obj);
                }

                return null;
            }

            /// <summary>
            /// Replacement for VisualTreeHelper.HitTest. 
            /// 
            /// This 'HitTest' method also takes the 'IsHitTestVisible' and 'IsVisible' properties 
            /// into account, so use it instead of the normal VisualTreeHelper.HitTest.
            /// </summary>
            /// <param name="visual">Hit test root</param>
            /// <param name="point">Point to test</param>
            /// <returns>DependencyObject hit or null.</returns>
            public static DependencyObject HitTest(this Visual visual, Point point)
            {
                HitTestResult result = null;

                VisualTreeHelper.HitTest(visual, target =>
                {
                    var uiElement = target as UIElement;
                    return ((uiElement != null) && (!uiElement.IsHitTestVisible || !uiElement.IsVisible))
                        ? HitTestFilterBehavior.ContinueSkipSelfAndChildren
                        : HitTestFilterBehavior.Continue;
                }, target =>
                {
                    result = target;
                    return HitTestResultBehavior.Stop;
                },
                    new PointHitTestParameters(point)
                    );

                return result != null ? result.VisualHit : null;
            }
        }
    }

    namespace TileProviders
    {
        using xserver;

        /// <summary>
        /// Definition of a delegate handling map updates.
        /// </summary>
        /// <param name="map">Map returned by xMap Server.</param>
        /// <param name="size">Size of the requested image.</param>
        /// <remarks>Delegate is always called twice; with map set to null, if an update begins 
        /// and with the resulting Map object, when an update ends.</remarks>
        internal delegate void MapUpdateDelegate(Map map, Size size);

        /// <summary>
        /// Fetches and provides images from xMap Server.
        /// </summary>
        internal class ExtendedXMapTiledProvider : XMapTiledProvider
        {
            /// <summary>
            /// url field
            /// </summary>
            private readonly string url;

            /// <summary>
            /// Creates the xMap provider.
            /// </summary>
            /// <param name="url">xMapServer URL.</param>
            /// <param name="user">User name of the XMap authentication.</param>
            /// <param name="password">Password of the XMap authentication.</param>

            public ExtendedXMapTiledProvider(string url, string user, string password)
                : base(url, XMapMode.Custom)
            {
                this.url = url;
                User = user;
                Password = password;
            }

            public override string CacheId
            {
                get
                {
                    string cacheId = base.CacheId;
                    if (this.CustomCallerContextProperties != null)
                    {
                        foreach (var ccp in this.CustomCallerContextProperties)
                            cacheId = cacheId + "/" + ccp.key + "/" + ccp.value;
                    }
                    return cacheId;
                }
            }

            /// <summary>
            /// MapUpdate event. See remarks on <see cref="MapUpdateDelegate"/>.
            /// </summary>
            public event MapUpdateDelegate MapUdpate;

            public IEnumerable<xserver.CallerContextProperty> CustomCallerContextProperties { get; set; }

            /// <inheritdoc/>
            public override byte[] TryGetStreamInternal(double left, double top, double right, double bottom, int width,
                int height)
            {
                var size = new Size(width, height);

                if (MapUdpate != null)
                    MapUdpate(null, size);

                using (var service = new XMapWSServiceImpl(url))
                {
                    var mapParams = new MapParams {showScale = false, useMiles = false};
                    var imageInfo = new ImageInfo {format = ImageFileFormat.GIF, height = height, width = width};

                    var bbox = new BoundingBox
                    {
                        leftTop = new Point {point = new PlainPoint {x = left, y = top}},
                        rightBottom = new Point {point = new PlainPoint {x = right, y = bottom}}
                    };

                    string profile = CustomProfile?? "ajax-av";

                    var ccProps = new List<CallerContextProperty>
                    {
                        new CallerContextProperty {key = "CoordFormat", value = "PTV_MERCATOR"},
                        new CallerContextProperty {key = "Profile", value = profile}
                    };

                    if (CustomCallerContextProperties != null)
                        ccProps.AddRange(CustomCallerContextProperties);

                    var cc = new CallerContext
                    {
                        wrappedProperties = ccProps.ToArray()
                    };


                    if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
                    {
                        service.PreAuthenticate = true;

                        var credentialCache = new System.Net.CredentialCache
                        {
                            {new Uri(url), "Basic", new System.Net.NetworkCredential(User, Password)}
                        };

                        service.Credentials = credentialCache;
                    }

                    var map = service.renderMapBoundingBox(bbox, mapParams, imageInfo, (CustomXMapLayers != null)? CustomXMapLayers.ToArray(): null,
                        true, cc);

                    if (MapUdpate != null)
                        MapUdpate(map, size);

                    return map.image.rawImage;
                }
            }
        }
    }
}
