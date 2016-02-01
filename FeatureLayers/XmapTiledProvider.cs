using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Ptv.XServer.Controls.Map.Tools;
using xserver;
using Ptv.XServer.Controls.Map.TileProviders;

namespace FeatureLayers
{


    /// <summary> A provider implementation for the xMapServer delivering tiled bitmaps. </summary>
    public class XMapTiledProviderEx : XMapTiledProviderBase, IObjectInfoProvider
    {
        /// <summary> Logging restricted to this class. </summary>
        private static readonly Logger logger = new Logger("XMapTiledProviderEx");

        /// <summary> URL of the xMap server. </summary>
        private readonly string url = string.Empty;
        /// <summary> Mode of the xMap layer. </summary>
        private readonly XMapMode mode;
        /// <summary> The user name for basic Http authentication. </summary>
        public string User { get; set; }
        /// <summary> The password for basic Http authentication. </summary>
        public string Password { get; set; }

        /// <summary> Initializes a new instance of the <see cref="XMapTiledProvider"/> class with the given connection
        /// string and mode. </summary>
        /// <param name="url"> The url to connect to the xMap server. </param>
        /// <param name="user">User name of the XMap authentication.</param>
        /// <param name="password">Password of the XMap authentication.</param>
        /// <param name="mode"> The mode of this tiled provider instance. </param>
        public XMapTiledProviderEx(string url, string user, string password, XMapMode mode)
        {
            ServicePointManager.FindServicePoint(new Uri(url)).ConnectionLimit = 8;
            this.url = url;
            User = user;
            Password = password;
            this.mode = mode;

            base.needsTransparency = mode != XMapMode.Background;

            // TODO: REVIEW ME - DEEP ZOOM
            //
            // To support 'deep zoom', the UntiledLayer was modified to take MaxZoom defined by this 
            // provider into consideration. As MaxZoom defaults to 18 (see XMapTiledProviderBase) this 
            // change also affects the default rendering behavior as it restricts label rendering where 
            // no restriction was before. We therefore change MaxZoom for the label layer at the least 
            // to 19, which is also the default for the map control itself. This way all the changes are 
            // considered to provide backward compatibility for the map's default configuration.
            //  
            // If this is ok, remove this comment. Otherwise find a better solution.

            if (mode == XMapMode.Town)
                base.MaxZoom = 19;
        }

        /// <summary> Initializes a new instance of the <see cref="XMapTiledProvider"/> class with the given connection
        /// string and mode. </summary>
        /// <param name="url"> The url to connect to the xMap server. </param>
        /// <param name="mode"> The mode of this tiled provider instance. </param>
        public XMapTiledProviderEx(string url, XMapMode mode) : this(url, string.Empty, string.Empty, mode) { }


        /// <summary> MapUpdate event. See remarks on <see cref="MapUpdateDelegate"/>. </summary>
        public event MapUpdateDelegate MapUdpate;

        public IEnumerable<CallerContextProperty> CustomCallerContextProperties { get; set; }

        /// <summary> Gets or sets the custom layers of the xMapServer. </summary>
        public IEnumerable<Layer> CustomXMapLayers { get; set; }

        /// <summary>
        /// The reference time
        /// </summary>
        public DateTime? ReferenceTime { get; set; }

        /// <inheritdoc/>
        public override byte[] TryGetStreamInternal(double left, double top, double right, double bottom, int width, int height)
        {
            var size = new System.Windows.Size(width, height);

            if (MapUdpate != null)
                MapUdpate(null, size);

            using (var service = new XMapWSServiceImpl(url))
            {
                if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
                {
                    service.PreAuthenticate = true;
                    service.Credentials = new CredentialCache { { new Uri(url), "Basic", new NetworkCredential(User, Password) } };
                }

                var mapParams = new MapParams {showScale = false, useMiles = false};
                var imageInfo = new ImageInfo {format = ImageFileFormat.GIF, height = height, width = width};
                var bbox = new BoundingBox
                {
                    leftTop = new Point {point = new PlainPoint {x = left, y = top}},
                    rightBottom = new Point {point = new PlainPoint {x = right, y = bottom}}
                };
                
                var profile = string.Empty;
                var layers = new List<Layer>();
                switch (mode)
                {
                    case XMapMode.Street: // only streets
                        profile = "ajax-bg";
                        layers.Add(new StaticPoiLayer {name = "town", visible = false, category = -1, detailLevel = 0});
                        layers.Add(new StaticPoiLayer {name = "background", visible = false, category = -1, detailLevel = 0});
                        break;

                    case XMapMode.Town: // only labels
                        profile = "ajax-fg";
                        break;

                    case XMapMode.Custom: // no base layer
                        profile = "ajax-fg";
                        layers.Add(new StaticPoiLayer {name = "town", visible = false, category = -1, detailLevel = 0});
                        layers.Add(new StaticPoiLayer {name = "street", visible = false, category = -1, detailLevel = 0});
                        layers.Add(new StaticPoiLayer {name = "background", visible = false, category = -1, detailLevel = 0});
                        break;

                    case XMapMode.Background: // only streets and polygones
                        profile = "ajax-bg";
                        layers.Add(new StaticPoiLayer {name = "town", visible = false, category = -1, detailLevel = 0});
                        break;
                }

                // add custom xmap layers
                if (CustomXMapLayers != null)
                {
                    // remove layers in the local 'layers' which are also defined as custom layers...
                    foreach (var customXMapLayers in CustomXMapLayers)
                    {
                        var xMapLayers = customXMapLayers; // Temporary variable needed for solving closure issues in the next code line
                        foreach (var layer in layers.Where(layer => (layer.GetType() == xMapLayers.GetType()) && (layer.name == xMapLayers.name)))
                        {
                            layers.Remove(layer);
                            break;
                        }
                    }

                    layers.AddRange(CustomXMapLayers);
                }

                if (!String.IsNullOrEmpty(CustomProfile))
                {
                    profile = CustomProfile;
                }

                var callerContextProps = new List<CallerContextProperty>
                {
                    new CallerContextProperty {key = "CoordFormat", value = "PTV_MERCATOR"},
                    new CallerContextProperty {key = "Profile", value = profile}
                };
                if (CustomCallerContextProperties != null)
                    callerContextProps.AddRange(CustomCallerContextProperties);

                if (!string.IsNullOrEmpty(ContextKey))
                    callerContextProps.Add(new CallerContextProperty {key = "ContextKey", value = ContextKey});

                var cc = new CallerContext {wrappedProperties = callerContextProps.ToArray()};

                if(ReferenceTime.HasValue)
                    mapParams.referenceTime = ReferenceTime.Value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                service.Timeout = 8000;
                var map = service.renderMapBoundingBox(bbox, mapParams, imageInfo, layers.ToArray(), true, cc);

#if DEBUG
                if (!BoundingBoxesAreEqual(bbox, map.visibleSection.boundingBox))
                    IssueBoundingBoxWarning(bbox, map.visibleSection.boundingBox, width, height, profile);
#endif

                if (MapUdpate != null)
                    MapUdpate(map, size);

                return map.image.rawImage;
            }
        }

#if DEBUG
        /// <summary>
        /// Convert a bounding box to a [minx, miny, maxx, maxy] representation.
        /// </summary>
        /// <param name="b">Bounding box to convert.</param>
        /// <returns>The [minx, miny, maxx, maxy] representation.</returns>
        public double[] GetMinMax(BoundingBox b)
        {
            return new[] {
                Math.Min(b.leftTop.point.x, b.rightBottom.point.x),
                Math.Min(b.leftTop.point.y, b.rightBottom.point.y),
                Math.Max(b.leftTop.point.x, b.rightBottom.point.x),
                Math.Max(b.leftTop.point.y, b.rightBottom.point.y)
            };
        }

        /// <summary>
        /// Calculates the delta for the minx, miny, maxx and maxy values of the given bounding boxes.
        /// </summary>
        /// <param name="b1">First bounding box.</param>
        /// <param name="b2">Second bounding box.</param>
        /// <returns>Delta values, in this order: minx, miny, maxx, maxy.</returns>
        public double[] GetDelta(BoundingBox b1, BoundingBox b2)
        {
            double[] minMax1 = GetMinMax(b1);
            double[] minMax2 = GetMinMax(b2);

            double[] delta = new double[4];

            for (int i = 0; i < 4; ++i)
                delta[i] = Math.Abs(minMax1[i] - minMax2[i]);

            return delta;
        }

        /// <summary>
        /// Checks if two bounding boxes are equal.
        /// </summary>
        /// <param name="b1">First bounding box.</param>
        /// <param name="b2">Second bounding box.</param>
        /// <param name="epsilon">Allowable tolerance.</param>
        /// <returns>True, if the bounding boxes are equal. False otherwise.</returns>
        public bool BoundingBoxesAreEqual(BoundingBox b1, BoundingBox b2, double epsilon = 1e-4)
        {
            return GetDelta(b1, b2).All(delta => (delta <= epsilon));
        }

        /// <summary>
        /// Generates a diagnostic output.
        /// </summary>
        /// <param name="requested">Requested bounding box.</param>
        /// <param name="returned">Returned bounding box.</param>
        /// <param name="width">Requested width.</param>
        /// <param name="height">Requested height.</param>
        /// <param name="profile">Requested profile.</param>
        public void IssueBoundingBoxWarning(BoundingBox requested, BoundingBox returned, int width, int height, string profile)
        {
            double[] minMaxRequested = GetMinMax(requested);
            double[] minMaxReturned = GetMinMax(returned);
            double[] delta = GetDelta(requested, returned);

            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            logger.Writeline(TraceEventType.Warning, String.Format("xMap did not return the requested map rectangle:\n" +
                "\trequested: [{3:0.000000}, {4:0.000000}, {5:0.000000}, {6:0.000000}], {1}x{2}, {0},\n" +
                "\t returned: [{7:0.000000}, {8:0.000000}, {9:0.000000}, {10:0.000000}]\n" +
                "\t    delta: [{11:0.000000}, {12:0.000000}, {13:0.000000}, {14:0.000000}]",
                profile, width, height, minMaxRequested[0], minMaxRequested[1], minMaxRequested[2],
                minMaxRequested[3], minMaxReturned[0], minMaxReturned[1], minMaxReturned[2],
                minMaxReturned[3], delta[0], delta[1], delta[2], delta[3]
            ));

            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }
#endif

        /// <inheritdoc/>
        public override string CacheId 
        { 
            get 
            { 
                var cacheId = "PtvXMap" + url + mode;
                if (!string.IsNullOrEmpty(User))
                    cacheId += "usr=" + User;
                if (!string.IsNullOrEmpty(Password))
                    cacheId += "pwd=" + Password;
                if(!string.IsNullOrEmpty(CustomProfile))
                    cacheId += "custProfile=" + CustomProfile;
                if(ReferenceTime.HasValue)
                    cacheId += ReferenceTime.ToString();

                if (CustomCallerContextProperties != null)
                {
                    cacheId = CustomCallerContextProperties.Aggregate(cacheId, (current, ccp) => current + ("/" + ccp.key + "/" + ccp.value));
                }
                return cacheId;
            } 
        }
    }
}

