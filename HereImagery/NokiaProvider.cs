using System;
using System.Linq;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map;
using System.Text.RegularExpressions;

namespace Ptv.XServer.Controls.Map
{
    public static class Nokia
    {
        /// <summary>
        /// The basic map types
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Complete map
            /// </summary>
            MapTile,  
            /// <summary>
            /// The basic map tile without streets and labels
            /// </summary>
            BaseTile,
            /// <summary>
            /// Street and label overlay onlay
            /// </summary>
            StreetTile,
            /// <summary>
            /// Label overlay only
            /// </summary>
            LabelTile,
        };

        /// <summary>
        /// The scheme for the tiles
        /// </summary>
        public enum Scheme
        {
            NormalDay,
            NormalDayCustom,
            CarNavDay,
            SatelliteDay,
            HybridDay,
            TerrainDay,
            NormalDayTransit,
            NormalDayGrey,
            CarnavDayGrey,
            NormalNightMobile,
            PedestrianDay,
            PedestrianNight,
        }

        /// <summary>
        /// Removes all nokia base map layers.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        public static void RemoveNokiaLayers(this LayerCollection layers)
        {
            var nokiaLayers = from layer in layers where layer.Name.StartsWith("Nokia_") select layer;
            foreach (var layer in nokiaLayers.ToList())
                layers.Remove(layer);
        }

        /// <summary>
        /// Add a nokia layer to the layers collection of the map.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        /// <param name="type">The basic map type.</param>
        /// <param name="scheme">The scheme of the map.</param>
        /// <param name="appId">The application id.</param>
        /// <param name="token">The token.</param>
        public static void AddNokiaLayer(this LayerCollection layers, Type type, Scheme scheme, string appId, string token)
        {
            // request schema is
            // http://SERVER-URL/maptile/2.1/TYPE/MAPID/SCHEME/ZOOM/COL/ROW/RESOLUTION/FORMAT?param=value&...

            string copyrightText = (scheme == Scheme.SatelliteDay) ? "© 2012 DigitalGlobe" : "© 2012 NAVTEQ";
            string schemeString = Regex.Replace(scheme.ToString(), "[a-z][A-Z]", m => m.Value[0] + "." + m.Value[1]).ToLower();
            string typeString = type.ToString().ToLower();
            string caption = (type == Type.StreetTile) ? "Streets" : (type == Type.LabelTile) ? "Labels" : "BaseMap";

            layers.Add(new TiledLayer("Nokia_" + type.ToString())
            {
                Caption = caption,
                IsLabelLayer = type == Type.LabelTile || type == Type.StreetTile,
                IsBaseMapLayer = type == Type.MapTile || type == Type.BaseTile,
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 20,
                    RequestBuilderDelegate = (x, y, level) =>
                    string.Format("http://{0}.maps.nlp.nokia.com/maptile/2.1/{1}/newest/{2}/{3}/{4}/{5}/256/png8?app_id={6}&token={7}",
                    "1234"[(x ^ y) % 4], typeString, schemeString, level, x, y, appId, token)
                },
                Copyright = copyrightText,
            });
        }
    }
}