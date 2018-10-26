//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using xserver;

namespace ServerSideRendering
{
    public class XMapLayer : UntiledLayer
    {
        public XMapLayer(string name, string url, string user, string password)
            : base(name)
        {
            var provider = new ExtendedXMapTiledProvider(url, user, password);

            UntiledProvider = provider;  
        }

        /// <summary>
        /// Property delegate. Reads or writes the xMap profile.
        /// </summary>
        public string Profile
        {
            get { return ((ExtendedXMapTiledProvider) UntiledProvider).CustomProfile; }
            set { ((ExtendedXMapTiledProvider) UntiledProvider).CustomProfile = value; }
        }

        /// <summary>
        /// Property delegate. Reads or writes the set of custom xMap Server layers.
        /// </summary>
        public IEnumerable<Layer> CustomXMapLayers
        {
            get { return ((ExtendedXMapTiledProvider) UntiledProvider).CustomXMapLayers; }
            set { ((ExtendedXMapTiledProvider) UntiledProvider).CustomXMapLayers = value; }
        }
    }

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

        public new IEnumerable<CallerContextProperty> CustomCallerContextProperties { get; set; }

        /// <inheritdoc/>
        public override byte[] TryGetStreamInternal(double left, double top, double right, double bottom, int width,
            int height, out IEnumerable<IMapObject> mapObjects)
        {
            using (var service = new XMapWSServiceImpl(url))
            {
                var mapParams = new MapParams {showScale = false, useMiles = false};
                var imageInfo = new ImageInfo {format = ImageFileFormat.GIF, height = height, width = width};

                var bbox = new BoundingBox
                {
                    leftTop = new Point {point = new PlainPoint {x = left, y = top}},
                    rightBottom = new Point {point = new PlainPoint {x = right, y = bottom}}
                };

                var profile = CustomProfile == null ? "ajax-av" : CustomProfile;

                var ccProps = new List<CallerContextProperty>
                {
                    new CallerContextProperty {key = "CoordFormat", value = "PTV_MERCATOR"},
                    new CallerContextProperty {key = "Profile", value = profile}
                };

                if (!string.IsNullOrEmpty(ContextKey))
                    ccProps.Add(new CallerContextProperty {key = "ContextKey", value = ContextKey});

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

                var map = service.renderMapBoundingBox(bbox, mapParams, imageInfo,
                    CustomXMapLayers != null ? CustomXMapLayers.ToArray() : null,
                    true, cc);

                mapObjects = map.wrappedObjects?
                    .Select(objects =>
                        objects.wrappedObjects?.Select(layerObject =>
                            (IMapObject) new XMap1MapObject(objects, layerObject)))
                    .Where(objects => objects != null && objects.Any())
                    .SelectMany(objects => objects);
                
                return map.image.rawImage;
            }
        }
    }
}