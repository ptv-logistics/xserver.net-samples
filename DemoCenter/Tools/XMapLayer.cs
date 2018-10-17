// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using xserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Canvases;
using Point = System.Windows.Point;

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
        /// </remarks>
        public class XMapLayer : UntiledLayer
        {
            /// <summary>
            /// Creates and initializes the xMap layer. Be sure to specify a xMap profile and the 
            /// layers to be requested in the properties profile and CustomXMapLayers.
            /// </summary>
            /// <param name="name">Layer name</param>
            /// <param name="url">xMap Server URL</param>
            /// <param name="user">xMap Server user</param>
            /// <param name="password">xMap Server password</param>
            public XMapLayer(string name, string url, string user, string password) : base(name)
            {
                InitializeFactory(CanvasCategory.Content, mapView => new UntiledCanvas(mapView, UntiledProvider)
                {
                    UpdateMapObjects = mapView.Name == "Map" ? UpdateMapObjects : (Action<IEnumerable<IMapObject>, Size>) null,
                    MaxRequestSize = MaxRequestSize,
                    MinLevel = MinLevel
                });
                UntiledProvider = new XMapTiledProvider(url, user, password, XMapMode.Custom);
            }

            /// <summary> Property delegate. Reads or writes the xMap profile. </summary>
            public String Profile
            {
                get => ((XMapTiledProvider) UntiledProvider).CustomProfile;
                set => ((XMapTiledProvider) UntiledProvider).CustomProfile = value;
            }

            /// <summary> Property delegate. Reads or writes the set of custom xMap Server layers. </summary>
            public IEnumerable<Layer> CustomXMapLayers
            {
                get => ((XMapTiledProvider) UntiledProvider).CustomXMapLayers;
                set => ((XMapTiledProvider) UntiledProvider).CustomXMapLayers = value;
            }

            /// <summary> Property delegate. Reads or writes the set of custom Caller Contexts. </summary>
            public IEnumerable<CallerContextProperty> CustomCallerContextProperties
            {
                get => ((XMapTiledProvider) UntiledProvider).CustomCallerContextProperties;
                set => ((XMapTiledProvider) UntiledProvider).CustomCallerContextProperties = value;
            }

            /// <summary>
            /// Indicates the marker object is a ballon, so the hit-box is y-shifted
            /// </summary>
            public bool MarkerIsBalloon { get; set; }

            /// <summary> Hit tests the given layer objects.  </summary>
            /// <param name="infos">Object information to hit test.</param>
            /// <param name="center">Point to test</param>
            /// <param name="maxPixelDistance">Maximal distance from the specified position to get the tool tips for.</param>
            /// <returns>Matching layer objects.</returns>
            protected override IEnumerable<IMapObject> ToolTipHitTest(IEnumerable<IMapObject> infos, Point center, double maxPixelDistance)
            {
                var enumerable = infos ?? Enumerable.Empty<IMapObject>();
                var list = base.ToolTipHitTest(enumerable, center, maxPixelDistance).ToList();

                foreach (var info in enumerable.Except(list))
                { 
                    var wrappedPoints = ((info.Source as LayerObject)?.geometry?.pixelGeometry as PlainLineString)?.wrappedPoints;
                    if (wrappedPoints == null)
                        continue;
                    
                    bool isMatch = (wrappedPoints.Length > 1)
                        ? PointInRangeOfLinestring(wrappedPoints.Select(pp => new Point(pp.x, pp.y)).ToArray(), center, maxPixelDistance)
                        : (wrappedPoints.Length == 1) && (center - new Point(wrappedPoints[0].x, wrappedPoints[0].y)).Length <= maxPixelDistance;

                    if (isMatch)
                        list.Add(info);
                }

                return list;
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
                double range2 = range * range; // square of range

                for (var i = 0; i < lineString.Length - 1; i++)
                {
                    Point c = ClosestPointOnSegment(lineString[i], lineString[i + 1], p);
                    var d = new Point(c.X - p.X, c.Y - p.Y); // distance vector

                    if (d.X * d.X + d.Y * d.Y <= range2)
                        return true;
                }

                return false;
            }

            /// <summary> Gets the closest point on a segment. </summary>
            /// <param name="a">The first point of the segment.</param>
            /// <param name="b">The second point of the segment.</param>
            /// <param name="p">The point to check.</param>
            /// <returns></returns>
            public static Point ClosestPointOnSegment(Point a, Point b, Point p)
            {
                var d = new Point(b.X - a.X, b.Y - a.Y);
                double number = (p.X - a.X) * d.X + (p.Y - a.Y) * d.Y;

                if (number <= 0.0)
                    return a;

                double denom = d.X * d.X + d.Y * d.Y;

                return (number >= denom) ? b : new Point(a.X + (number / denom) * d.X, a.Y + (number / denom) * d.Y);
            }
        }


        /// <summary>
        /// Provides extensions used by XMapLayer.
        /// </summary>
        public static class XMapLayerExtensions
        {
            /// <summary>
            /// Gets the parent of a dependency object
            /// </summary>
            /// <param name="obj">The dependency object to get the parent for</param>
            /// <returns>Parent of the dependency object</returns>
            public static DependencyObject GetParent(this DependencyObject obj)
            {
                if (obj == null) return null;

                if (!(obj is ContentElement contentElement)) return VisualTreeHelper.GetParent(obj);

                return ContentOperations.GetParent(contentElement) ?? (contentElement as FrameworkContentElement)?.Parent;
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
                    if (obj is T objTest)
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

                VisualTreeHelper.HitTest(visual, 
                    target => ((target is UIElement uiElement) && (!uiElement.IsHitTestVisible || !uiElement.IsVisible))
                        ? HitTestFilterBehavior.ContinueSkipSelfAndChildren
                        : HitTestFilterBehavior.Continue, 
                    target => {
                        result = target;
                        return HitTestResultBehavior.Stop;
                    },
                    new PointHitTestParameters(point)
                );

                return result?.VisualHit;
            }
        }
    }
}
