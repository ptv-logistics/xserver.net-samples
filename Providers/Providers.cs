using System.Linq;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using System.Text.RegularExpressions;
using System;

namespace Ptv.XServer.Controls.Map
{
    public static class Here
    {
        /// <summary>
        /// The basic map types
        /// </summary>
        public enum HereType
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
            /// Street and label overlay only
            /// </summary>
            StreetTile,
            /// <summary>
            /// Label overlay only
            /// </summary>
            LabelTile
        }

        /// <summary>
        /// The scheme for the tiles
        /// </summary>
        public enum HereScheme
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
            PedestrianNight
        }

        /// <summary>
        /// Removes all nokia base map layers.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        public static void RemoveBaseMapLayers(this LayerCollection layers)
        {
            var nokiaLayers = from layer in layers
                where layer.Name.StartsWith("HERE_") || layer.Name.StartsWith("OSM_")
                select layer;
            foreach (var layer in nokiaLayers.ToList())
                layers.Remove(layer);
        }

        /// <summary>
        /// Add a HERE layer to the layers collection of the map.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        /// <param name="type">The basic map type.</param>
        /// <param name="scheme">The scheme of the map.</param>
        /// <param name="appId">The application id.</param>
        /// <param name="token">The token.</param>
        public static void AddHereLayer(this LayerCollection layers, HereType type, HereScheme scheme, string appId, string token)
        {
            string schemeString = Regex.Replace(scheme.ToString(), "[a-z][A-Z]", m => m.Value[0] + "." + m.Value[1]).ToLower();
            string baseString = scheme == Here.HereScheme.SatelliteDay || scheme == Here.HereScheme.TerrainDay ? "aerial" : "base";
            string typeString = type.ToString().ToLower();
            string caption = type == HereType.StreetTile 
                ? "Streets" 
                : type == HereType.LabelTile ? "Labels" : "BaseMap";

            layers.Add(new TiledLayer("HERE_" + type)
            {
                Caption = caption,
                IsBaseMapLayer = type == HereType.MapTile || type == HereType.BaseTile,
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 20,
                    RequestBuilderDelegate = (x, y, level) =>
                    $"https://{"1234"[(x ^ y) % 4]}.{baseString}.maps.api.here.com/maptile/2.1/{typeString}/newest/{schemeString}/{level}/{x}/{y}/256/png8?app_id={appId}&token={token}",
                },
                Copyright = $"Map © 1987-{DateTime.Now.Year} HERE"
            });
        }

        /// <summary>
        /// Add an OSM layer to the layers collection of the map.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        public static void AddOSMLayer(this LayerCollection layers)
        {
            layers.Add(new TiledLayer("OSM_DE")
            {
                Caption = "OpenStreetMap.DE",
                IsBaseMapLayer = true,
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 19,
                    RequestBuilderDelegate = (x, y, level) =>
                        $"https://{"abc"[(x ^ y) % 3]}.tile.openstreetmap.de/tiles/osmde//{level}/{x}/{y}.png",
                },
                Copyright = $"Map © OpenStreetMap contributors"
            });
        }
    }
}