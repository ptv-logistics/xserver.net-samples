using FeatureLayers;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ptv.XServer.Demo.UseCases.FeatureLayer
{
    /// <summary>
    /// Enumeration of a few existing Feature Layer themes, which will be used in this use case.
    /// Such a theme contains a set of modifications for the street segments, which are used in the map
    /// and are commonly addressed to one subject only.
    /// 
    /// Usually a theme is not available for all existing maps, therefore only a subset is used for
    /// one dedicated map. For example, the 'preferred routes' theme (giving some route segments a
    /// bonus, when these segments are used for route calculation) is available in the North America map, 
    /// but not in the Europe map.
    /// </summary>
    public enum Theme
    {
        /// <summary> This layer provides up-to-date traffic information to consider incidents like traffic jams in the route planning. </summary>
        TrafficIncidents,
        /// <summary> Truck Attributes provide data to calculate and display exact routes for extraordinary vehicles, for example vehicles exceeding the conventional height. </summary>
        TruckAttributes,
        /// <summary> This layer of this theme provides restricted transit areas for truck transit zones. </summary>
        RestrictionZones,
        /// <summary> This Feature Layer represents subnetworks of the road network to be prioritized. </summary>
        PreferredRoutes,
        /// <summary> With speed pattern data the average traffic situation during the week is modelled. </summary>
        SpeedPatterns
    }

    /// <summary>
    /// This use case encapsulates all relevant arrangements necessary for rendering the Feature Layer attributes
    /// and use them for influencing the route calculation. Additional data is necessary for a proper functioning,
    /// therefore this use case must be configured according the available Feature Layer data. Three different ways
    /// are shown how the configuration can be made, see <see cref="FeatureLayerPresenter"/> constructor.
    /// </summary>
    public class FeatureLayerPresenter
    {
        /// <summary> 
        /// Map object needed to draw the segment attributes and example routes, which are calculated in combination
        /// with the Feature Layer data.
        /// </summary>
        private readonly Map map;


        /// <summary>
        /// Constructor of this use case, which tries to determine which Feature Layers are configured for the currently selected map.
        /// Additionally, a shape layer is created and inserted into the <see cref="map"/> parameter, for rendering the Feature Layer
        /// symbols and the example routes.
        /// </summary>
        /// <param name="map">Map object for which the feature layers have to be rendered and example routes are shown.</param>
        public FeatureLayerPresenter(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// XElement containing the so-called XML snippet, which is needed for the xMap and xRoute requests. Some sub elements of this XElement
        /// are manipulated at runtime, especially the enabled flag for a Feature Layer.
        /// Setting this XElement containing the configuration of all installed Feature Layers, results in an update of the map.
        /// Some suggestions are made Settings.settings file for the PTV XServer internet products, but without any warranty for completeness of all 
        /// available Feature Layers. Please adapt the snippet strings in these settings.
        /// </summary>
        private XElement FeatureLayerXElement
        {
            get
            {
                var layers = new List<XElement>();

                if (UseTrafficIncidents)
                    layers.Add(new XElement("Theme", new XAttribute("id", "PTV_TrafficIncidents"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)));

                if (UseTruckAttributes)
                    layers.Add(new XElement("Theme", new XAttribute("id", "PTV_TruckAttributes"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)));

                if (UsePreferredRoutes)
                    layers.Add(new XElement("Theme", new XAttribute("id", "PTV_PreferredRoutes"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)));

                if (UseRestrictionZones)
                    layers.Add(new XElement("Theme", new XAttribute("id", "PTV_RestrictionZones"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)));

                if (UseSpeedPatterns)
                    layers.Add(new XElement("Theme", new XAttribute("id", "PTV_SpeedPatterns"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)));

                // Configuration of the Feature Layer, whereby in this implementation no themes are activated. This is because the installed themes
                // are unknown at your concrete system; Please configure it according your configuration, by uncommenting the corresponding XElement Theme nodes.
                // Maybe additional information is needed for configuration of a Feature Layer (Preferred Route theme may be such a candidate).
                XNamespace nameSpace = "http://www.w3.org/2001/XMLSchema-instance";
                return new XElement(nameSpace + "Profile",
                                        new XElement("FeatureLayer", new XAttribute("majorVersion", 1), new XAttribute("minorVersion", 0),
                                            new XElement("GlobalSettings", new XAttribute("enableTimeDependency", true)),
                                            new XElement("Themes", layers
                                                )));
            }
        }

        /// <summary> 
        /// After a feature layer property has changed, the content in the map window has to be updated. The rendering of this information
        /// involves more than one base layer, therefore an enumeration is implemented. Each iteration step provides all necessary information
        /// needed for the rendering requests. 
        /// </summary>
        private struct RefreshInfo
        {
            public XMapTiledProviderEx provider;
            public BaseLayer layer;
        }

        /// <summary> Iterator which steps through all relevant layers, which are involved in the feature layer rendering. </summary>
        /// <returns> All needed information for an update of the map window content.</returns>
        private IEnumerable<RefreshInfo> RefreshInfos()
        {
            var backgroundLayer = map.Layers["Background"] as TiledLayer;
            if (backgroundLayer != null)
                yield return new RefreshInfo { provider = (XMapTiledProviderEx) backgroundLayer.TiledProvider, layer = backgroundLayer };

            var foregroundLayer = map.Layers["Labels"] as UntiledLayer;
            if (foregroundLayer != null)
                yield return new RefreshInfo { provider = (XMapTiledProviderEx) foregroundLayer.UntiledProvider, layer = foregroundLayer };
        }

        public bool UseTrafficIncidents { get; set; }
        public bool UseTruckAttributes { get; set; }
        public bool UsePreferredRoutes { get; set; }
        public bool UseRestrictionZones { get; set; }
        public bool UseSpeedPatterns { get; set; }

        /// <summary>
        /// Sending a request for all involved base layers, which provide some extension of their requests by a snippet mechanism.
        /// </summary>
        public void RefreshMap()
        {
            var snippet = FeatureLayerXElement.ToString();
            foreach (var refreshInfo in RefreshInfos())
            {
                refreshInfo.provider.CustomCallerContextProperties = new[] {new xserver.CallerContextProperty {key = "ProfileXMLSnippet", value = snippet}};
                if (refreshInfo.layer.Name == "Labels")
                {
                    // The insertion of xserver.FeatureLayer objects into the CustomXMapLayers results in a providing of ObjectInfo objects in the return value
                    // of XMap.RenderMapBoundingBox(). Only by means of these objects, it is possible to retrieve information for tool tips.
                    var xServerLayers = new List<xserver.Layer>();
                    if (UseTrafficIncidents)
                        xServerLayers.Add( new xserver.FeatureLayer { name = "PTV_TrafficIncidents", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });
                    if (UseTruckAttributes)
                        xServerLayers.Add(new xserver.FeatureLayer { name = "PTV_TruckAttributes", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });
                    if (UsePreferredRoutes)
                        xServerLayers.Add(new xserver.FeatureLayer { name = "PTV_PreferredRoutes", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });
                    if (UseRestrictionZones)
                        xServerLayers.Add(new xserver.FeatureLayer { name = "PTV_RestrictionZones", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });
                    if (UseSpeedPatterns)
                        xServerLayers.Add(new xserver.FeatureLayer { name = "PTV_SpeedPatterns", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });

                    refreshInfo.provider.CustomXMapLayers = xServerLayers;
                }
                refreshInfo.layer.Refresh();
            }
        }
    }
}
