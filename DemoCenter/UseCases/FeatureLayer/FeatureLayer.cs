// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Layers.Shapes;

using Ptv.XServer.Demo.Properties;
using Ptv.XServer.Demo.Tools;
using Ptv.XServer.Demo.XrouteService;

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
    /// are shown how the configuration can be made, see <see cref="FeatureLayerUseCase"/> constructor.
    /// </summary>
    public class FeatureLayerUseCase
    {
        /// <summary> 
        /// Map object needed to draw the segment attributes and example routes, which are calculated in combination
        /// with the Feature Layer data.
        /// </summary>
        private readonly Map map;

        /// <summary>Displays way points and courses of the example routes.</summary>
        private ShapeLayer shapeLayer;


        /// <summary>
        /// Constructor of this use case, which tries to determine which Feature Layers are configured for the currently selected map.
        /// Additionally, a shape layer is created and inserted into the <see cref="map"/> parameter, for rendering the Feature Layer
        /// symbols and the example routes.
        /// </summary>
        /// <param name="map">Map object for which the feature layers have to be rendered and example routes are shown.</param>
        public FeatureLayerUseCase(Map map)
        {
            this.map = map;
        }

        private XElement featureLayerXElement;
        /// <summary>
        /// XElement containing the so-called XML snippet, which is needed for the xMap and xRoute requests. Some sub elements of this XElement
        /// are manipulated at runtime, especially the enabled flag for a Feature Layer.
        /// Setting this XElement containing the configuration of all installed Feature Layers, results in an update of the map.
        /// Some suggestions are made Settings.settings file for the PTV XServer internet products, but without any warranty for completeness of all 
        /// available Feature Layers. Please adapt the snippet strings in these settings.
        /// </summary>
        private XElement FeatureLayerXElement
        {
            set
            {
                featureLayerXElement = value;
                SetMissingLanguageAttribute();
                RefreshMap();
            }

            get
            {
                if (featureLayerXElement != null) return featureLayerXElement;

                // Select the whole set of scenarios for the complete map, containing all Feature Layer themes known by the demo implementation.
                var region = UseCase.ManagedAuthentication.XMapMetaInfo.GetRegion();
                if (region == Region.eu || region == Region.world)
                    FeatureLayerXElement = XElement.Parse(Settings.Default.FeatureLayerEurope);
                else if (region == Region.na)
                    FeatureLayerXElement = XElement.Parse(Settings.Default.FeatureLayerNorthAmerica);
                else if (region == Region.au)
                    FeatureLayerXElement = XElement.Parse(Settings.Default.FeatureLayerAustralia);
                else
                {
                    // Hard-coded configuration of the Feature Layer, whereby in this implementation no themes are activated. This is because the installed themes
                    // are unknown at your concrete system; Please configure it according your configuration, by uncommenting the corresponding XElement Theme nodes.
                    // Maybe additional information is needed for configuration of a Feature Layer (Preferred Route theme may be such a candidate).
                    XNamespace nameSpace = "http://www.w3.org/2001/XMLSchema-instance";
                    FeatureLayerXElement = new XElement(nameSpace + "Profile",
                                            new XElement("FeatureLayer", new XAttribute("majorVersion", 1), new XAttribute("minorVersion", 0),
                                                new XElement("GlobalSettings", new XAttribute("enableTimeDependency", true)),
                                                new XElement("Themes" //,
                        //new XElement("Theme", new XAttribute("id", "PTV_TrafficIncidents"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)),
                        //new XElement("Theme", new XAttribute("id", "PTV_TruckAttributes"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)),
                        //new XElement("Theme", new XAttribute("id", "PTV_PreferredRoutes"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)),
                        //new XElement("Theme", new XAttribute("id", "PTV_RestrictionZones"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0)),
                        //new XElement("Theme", new XAttribute("id", "PTV_SpeedPatterns"), new XAttribute("enabled", true), new XAttribute("priorityLevel", 0))
                                                    )),
                                            new XElement("Routing", new XAttribute("majorVersion", 1), new XAttribute("minorVersion", 0),
                                                new XElement("Course",
                                                    new XElement("AdditionalDataRules", new XAttribute("enabled", true)),
                                                    new XElement("DynamicRouting", new XAttribute("limitDynamicSpeedToStaticSpeed", false)))));
                }

                return featureLayerXElement;
            }
        }

        /// <summary> Helper method setting the language attribute in the Common XElement, if missing. Without this setting, the traffic incident messages are always in English. </summary>
        private void SetMissingLanguageAttribute()
        {
            if (featureLayerXElement == null)
                return;

            var languageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var element = featureLayerXElement.XPathSelectElement("./Common");
            if (element == null)
            {
                featureLayerXElement.Add(new XElement("Common", 
                                                      new XAttribute("language", languageName), 
                                                      new XAttribute("majorVersion", 1), 
                                                      new XAttribute("minorVersion", 0)));
                return;
            }

            if (element.Attribute("language") != null) return;
            element.Add(new XAttribute("language", languageName));
        }

        /// <summary>
        /// This property returns a clone of the <see cref="FeatureLayerXElement"/> element, except the enabled flag is set to false for each installed
        /// Feature Layer. By means of this XElement an additional route can be calculated, allowing a comparison of a route with and without the Feature Layers. 
        /// </summary>
        private XElement DeactivatedXElement
        {
            get
            {
                var result = new XElement(FeatureLayerXElement);
                foreach (var theme in result.XPathSelectElements("./FeatureLayer/Themes/Theme"))
                    try { theme.Attribute("enabled").Value = "false"; }
                    catch { }

                return result;
            }
        }

        /// <summary> 
        /// Helper method for setting the enable flag of a dedicated Feature Layer (specified by the corresponding theme). 
        /// If the XML file contains some invalid content, the current value is not changed.
        /// </summary>
        /// <param name="themeId">Textual representation of the theme (id), which is directly used the XML snippet. </param>
        /// <param name="value">Enable/disable the corresponding Feature Layer, i.e. showing/hiding its content in the map and
        /// taking into consideration its attributes or not. </param>
        private void SetEnabled(string themeId, bool value)
        {
            try
            {
                FeatureLayerXElement.XPathSelectElements("./FeatureLayer/Themes/Theme")
                    .First(theme => theme.Attribute("id").Value == themeId)
                    .Attribute("enabled")
                    .Value = value.ToString().ToLower();

                if (UseTrafficIncidents || UseTruckAttributes || UsePreferredRoutes || UseRestrictionZones || UseSpeedPatterns)
                {
                    if (map.Layers["Feature Layer routes"] == null)
                    {
                        shapeLayer = new ShapeLayer("Feature Layer routes") {SpatialReferenceId = "OG_GEODECIMAL"};
                        map.Layers.InsertBefore(shapeLayer, "Labels");
                    }
                }
                else
                {
                    if (map.Layers["Feature Layer routes"] != null)
                    {
                        map.Layers.Remove(map.Layers["Feature Layer routes"]);
                        shapeLayer = null;
                    }
                }
            }
            catch { return; } // Something does not match the XML

            RefreshMap();
        }

        /// <summary> 
        /// Helper method for getting the enable flag of a dedicated Feature Layer (specified by the corresponding theme). 
        /// If the XML file contains some invalid content, the value 'false' is returned.
        /// </summary>
        /// <param name="themeId">Textual representation of the theme (id), which is directly used the XML snippet. </param>
        /// <returns>Returns the Enable/disable flag of the corresponding Feature Layer. </returns>
        private bool GetEnabled(string themeId)
        {
            try
            {
                return Convert.ToBoolean(
                    FeatureLayerXElement.XPathSelectElements("./FeatureLayer/Themes/Theme")
                        .First(theme => theme.Attribute("id").Value == themeId)
                        .Attribute("enabled").Value);
            }
            catch { return false; } // Something does not match the XML
        }

        private bool GetAvailable(string themeId)
        {
            try { return FeatureLayerXElement.XPathSelectElements("./FeatureLayer/Themes/Theme").Any(theme => theme.Attribute("id")?.Value == themeId); }
            catch { return false; } // Something does not match the XML
        }

        /// <summary> Check if traffic incidents are configured. </summary>
        public bool AvailableTrafficIncidents => GetAvailable("PTV_TrafficIncidents");

        /// <summary> Enable flag of the traffic incidents. If this Feature Layer is not available, the setter returns always false. </summary>
        public bool UseTrafficIncidents
        {
            get { return AvailableTrafficIncidents && GetEnabled("PTV_TrafficIncidents"); }
            set { SetEnabled("PTV_TrafficIncidents", AvailableTrafficIncidents && value); }
        }

        /// <summary> Check if truck attributes are installed at the xServer system. </summary>
        public bool AvailableTruckAttributes => GetAvailable("PTV_TruckAttributes");

        /// <summary> Enable flag of the truck attributes. If this Feature Layer is not available, the setter returns always false. </summary>
        public bool UseTruckAttributes
        {
            get { return AvailableTruckAttributes && GetEnabled("PTV_TruckAttributes"); }
            set { SetEnabled("PTV_TruckAttributes", AvailableTruckAttributes && value); }
        }

        /// <summary> Check if the Feature Layer 'Preferred Routes' is installed at the xServer system. </summary>
        public bool AvailablePreferredRoutes => GetAvailable("PTV_PreferredRoutes");

        /// <summary> Enable flag of the preferred routes. If this Feature Layer is not available, the setter returns always false. </summary>
        public bool UsePreferredRoutes
        {
            get { return AvailablePreferredRoutes && GetEnabled("PTV_PreferredRoutes"); }
            set { SetEnabled("PTV_PreferredRoutes", AvailablePreferredRoutes && value); }
        }

        /// <summary> Check if the restriction zones are installed at the xServer system. </summary>
        public bool AvailableRestrictionZones => GetAvailable("PTV_RestrictionZones");

        /// <summary> Enable flag of the restriction zones. If this Feature Layer is not available, the setter returns always false. </summary>
        public bool UseRestrictionZones
        {
            get { return AvailableRestrictionZones && GetEnabled("PTV_RestrictionZones"); }
            set { SetEnabled("PTV_RestrictionZones", AvailableRestrictionZones && value); }
        }

        /// <summary> Check if the speed patterns are installed at the xServer system. </summary>
        public bool AvailableSpeedPatterns => GetAvailable("PTV_SpeedPatterns");

        /// <summary> Enable flag of the speed patterns. If this Feature Layer is not available, the setter returns always false. </summary>
        public bool UseSpeedPatterns
        {
            get { return AvailableSpeedPatterns && GetEnabled("PTV_SpeedPatterns"); }
            set { SetEnabled("PTV_SpeedPatterns", AvailableSpeedPatterns && value); }
        }

        /// <summary> 
        /// After a feature layer property has changed, the content in the map window has to be updated. The rendering of this information
        /// involves more than one base layer, therefore an enumeration is implemented. Each iteration step provides all necessary information
        /// needed for the rendering requests. 
        /// </summary>
        private struct RefreshInfo
        {
            public XMapTiledProvider provider;
            public BaseLayer layer;
        }

        /// <summary> Iterator which steps through all relevant layers, which are involved in the feature layer rendering. </summary>
        /// <returns> All needed information for an update of the map window content.</returns>
        private IEnumerable<RefreshInfo> RefreshInfos()
        {
            if (map.Layers["Background"] is TiledLayer backgroundLayer)
                yield return new RefreshInfo { provider = (XMapTiledProvider) backgroundLayer.TiledProvider, layer = backgroundLayer };

            if (map.Layers["Labels"] is UntiledLayer foregroundLayer)
                yield return new RefreshInfo { provider = (XMapTiledProvider) foregroundLayer.UntiledProvider, layer = foregroundLayer };
        }

        /// <summary>
        /// Sending a request for all involved base layers, which provide some extension of their requests by a snippet mechanism.
        /// </summary>
        private void RefreshMap()
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
                        xServerLayers.Add(new xserver.FeatureLayer { name = "PTV_TrafficIncidents", visible = true, objectInfos = xserver.ObjectInfoType.REFERENCEPOINT });
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


        // ROUTING

        /// <summary> 
        /// A scenario contains a way point list defining a route and a its textual description shown in a combo box. 
        /// The route is calculated in two different ways: First the route is calculated without any Feature Layer information,
        /// meanwhile the second time the same route is used with Feature Layer taken into consideration. The way points are
        /// chosen in such a way, that the two calculation differ.
        /// </summary>
        private class Scenario
        {
            public string description;
            public PlainPoint[] wayPoints;
        }

        /// <summary> Set of scenarios associated to one dedicated theme. </summary>
        private class Scenarios
        {
            public Theme theme;
            public List<Scenario> scenarios;
        }

        /// <summary>
        /// Stores the set of Feature Layer scenarios for Europe. It is divided into different sets according the
        /// available Feature Layer themes. For each theme different routes are available, showing the impact of the
        /// corresponding Feature Layer on the course.
        /// </summary>
        private static readonly List<Scenarios> europeanScenarios = new List<Scenarios>
        {
            new Scenarios
            { 
                theme = Theme.TrafficIncidents,
                scenarios = new List<Scenario>
                {
                    new Scenario { description = "DE, Karlsruhe", wayPoints = new [] { new PlainPoint { x = 8.40118, y = 49.00722 }, new PlainPoint { x = 8.40437, y = 48.99926 } } },
                    new Scenario { description = "BE, Brussel", wayPoints = new [] { new PlainPoint { x = 4.31084, y = 50.83376 }, new PlainPoint { x = 4.31798, y = 50.83062 } } },
                }
            },
            new Scenarios
            { 
                theme = Theme.TruckAttributes,
                scenarios = new List<Scenario>
                {
                    new Scenario { description = "DE, Karlsruhe Hardtwald", wayPoints = new [] { new PlainPoint { x = 8.39991, y = 49.06487 }, new PlainPoint { x = 8.44814, y = 49.04496 } } },
                    new Scenario { description = "DE, Hamburg Elbtunnel", wayPoints = new [] { new PlainPoint { x = 9.89731, y = 53.50826 }, new PlainPoint { x = 9.89061, y = 53.56754 } } },
                }
            },
            new Scenarios
            { 
                theme = Theme.RestrictionZones,
                scenarios = new List<Scenario>
                {
                    new Scenario { description = "DE, Fellbach \u2192 DE, Musberg", wayPoints = new [] { new PlainPoint { x = 9.28413, y = 48.81319 }, new PlainPoint { x = 9.12277, y = 48.69413 } } },
                    new Scenario { description = "DE, Ulm (bypass road) ", wayPoints = new [] { new PlainPoint { x = 9.96867, y = 48.43358 }, new PlainPoint { x = 10.09026, y = 48.31968 } } },
                }
            },
            new Scenarios
            { 
                theme = Theme.SpeedPatterns,
                scenarios = new List<Scenario>
                {
                    new Scenario { description = "DE, Karlsruhe Adenauerring", wayPoints = new [] { new PlainPoint { x = 8.37922, y = 49.01502 }, new PlainPoint { x = 8.42806, y = 49.01328 } } },
                    new Scenario { description = "US, New York", wayPoints = new [] { new PlainPoint { x = -74.03283, y = 40.78560 }, new PlainPoint { x = -74.00660, y = 40.71387 } } },
                }
            }
        };

        /// <summary>
        /// Stores the set of Feature Layer scenarios for North America. It is divided into different sets according the
        /// available Feature Layer themes. For each theme different routes are available, showing the impact of the
        /// corresponding Feature Layer on the course.
        /// </summary>
        private static readonly List<Scenarios> northAmericanScenarios = new List<Scenarios>
        {
            new Scenarios 
            { 
                theme = Theme.PreferredRoutes,
                scenarios = new List<Scenario> 
                {  
                    new Scenario { description = "US, New York", wayPoints = new [] { new PlainPoint { x = -74.11548614501952, y = 40.934654261458064 }, new PlainPoint { x = -74.21195983886719, y = 40.85017679415775 } } },
                    new Scenario { description = "US, Washington", wayPoints = new [] { new PlainPoint { x = -76.827392578125, y = 38.98022899416542 }, new PlainPoint { x = -77.13912963867188, y = 39.15349256868933 } } }
                }
            },
            new Scenarios 
            { 
                theme = Theme.SpeedPatterns,
                scenarios = new List<Scenario> 
                {
                    new Scenario { description = "US, New York / Newark", wayPoints = new [] { new PlainPoint { x = -74.14355278015137, y = 40.730380832918506 }, new PlainPoint { x = -74.1379094, y = 40.72816938500141 } } },
                    new Scenario { description = "US, New York / Queens", wayPoints = new [] { new PlainPoint { x = -73.8058090209961, y = 40.77228687788679 }, new PlainPoint { x = -73.7647819519043, y = 40.75018302371518 } } },
                }
            }
        };

        /// <summary>
        /// Stores the set of Feature Layer scenarios for Australia. It is divided into different sets according the
        /// available Feature Layer themes. For each theme different routes are available, showing the impact of the
        /// corresponding Feature Layer on the course.
        /// </summary>
        private static readonly List<Scenarios> australiaScenarios = new List<Scenarios>
        {
            new Scenarios 
            { 
                theme = Theme.PreferredRoutes,
                scenarios = new List<Scenario> 
                {  
                    new Scenario { description = "AU, Canberra", wayPoints = new [] { new PlainPoint { x = 149.16790008544922, y = -35.2587962018063 }, new PlainPoint { x = 149.15897369384763, y = -35.18685679233885 } } },
                    new Scenario { description = "AU, Melbourne", wayPoints = new [] { new PlainPoint { x = 145.30517578125, y = -36.86259206599487 }, new PlainPoint { x = 145.70892333984375, y = -36.9795180188502 } } }
                }
            },
            new Scenarios
            { 
                theme = Theme.TruckAttributes,
                scenarios = new List<Scenario>
                {
                    new Scenario { description = "AU, Sydney", wayPoints = new [] { new PlainPoint { x = 151.19080066680908, y = -33.86699475113555 }, new PlainPoint { x = 151.19841814041138, y = -33.88165690137696 } } },
                    new Scenario { description = "AU, Perth", wayPoints = new [] { new PlainPoint { x = 115.76791763305664, y = -31.752985018861764 }, new PlainPoint { x = 115.80980300903319, y = -31.886303498308433 } } }
                }
            },
            new Scenarios 
            { 
                theme = Theme.SpeedPatterns,
                scenarios = new List<Scenario> 
                {
                    new Scenario { description = "AU, Brisbane", wayPoints = new [] { new PlainPoint { x = 153.0859851837158, y = -27.44373485115151 }, new PlainPoint { x = 153.0756640434265, y = -27.433032217684314 } } },
                    new Scenario { description = "AU, Melbourne", wayPoints = new [] { new PlainPoint { x = 144.91814374923706, y = -37.705943946680854 }, new PlainPoint { x = 144.90556955337524, y = -37.70850734686791 } } }
                }
            }
        };

        private List<Scenarios> scenariosByCluster;
        /// <summary>Stores the currently selected Feature Layer scenario set, containing all types of Feature Layers, commonly of a complete continent.
        /// Getter is never null.</summary>
        private List<Scenarios> ScenariosByCluster
        {
            get
            {
                if (scenariosByCluster != null) return scenariosByCluster;

                // Select the whole set of scenarios for the complete map, containing all Feature Layer themes known by the demo implementation.
                var region = UseCase.ManagedAuthentication.XMapMetaInfo.GetRegion();
                if (region == Region.eu || region == Region.world)
                    scenariosByCluster = europeanScenarios;
                else if (region == Region.na)
                    scenariosByCluster = northAmericanScenarios;
                else if (region == Region.au)
                    scenariosByCluster = australiaScenarios;
                else
                    scenariosByCluster = new List<Scenarios>(); // Insert empty list to avoid repeated trials for getting the scenarios

                return scenariosByCluster;
            }
        }


        /// <summary>
        /// The scenarios of variable <see cref="ScenariosByCluster"/> are divided into separate themes. A string collection containing the 
        /// description of each scenario belonging to the specified theme is returned by this method. It is used for filling the combo boxes.
        /// </summary>
        /// <param name="theme">Theme for which the scenarios are selected for.</param>
        /// <returns>String collection containing the description of each scenario belonging to the specified theme. </returns>
        public IEnumerable<string> GetScenarioDescriptionsBy(Theme theme)
        {
            try { return ScenariosByCluster.Find(scenarios => scenarios.theme == theme).scenarios.Select(scenario => scenario.description); }
            catch { return new List<string>(); } // ScenariosByCluster maybe null or theme not found
        }

        /// <summary>
        /// Callback delegate for getting informed if scenario is about to change.
        /// The first Action parameter specifies which new theme is selected, while the second parameter indicates,
        /// if the (commonly) long duration job is started (= true) or finished (= false).
        /// </summary>
        public Action<Theme, bool> ScenarioChanged;

        /// <summary>
        /// According the specified theme parameter and and the list position, a dedicated scenario is selected uniquely.
        /// Calling this asynchronous method results in calculating the route in two different ways: With and without
        /// any Feature Layer.
        /// </summary>
        /// <param name="theme">The theme which should be used.</param>
        /// <param name="position">Position of the scenario in the theme specific scenario list.</param>
        public async void SetScenario(Theme theme, int position)
        {
            if (position < 0) return;

            var currentScenario = ScenariosByCluster.Find(scenarios => scenarios.theme == theme).scenarios[position];

            // Fires callback to inform MainWindow about a scenario change.
            ScenarioChanged(theme, true);

            shapeLayer.Shapes.Clear();
            SetWayPointPins(currentScenario);

            // Starts the normal and Feature Layer route calculation in parallel.
            var calcNorm = Task.Factory.StartNew(() => CalculateRoute(currentScenario, false));
            var calcFeat = Task.Factory.StartNew(() => CalculateRoute(currentScenario, true));

            // Waits for the route calculations.
            await calcNorm;
            await calcFeat;

            ScenarioChanged(theme, false);
        }


        /// <summary> Adds a pin to the map for each way point of the example route. </summary>
        private void SetWayPointPins(Scenario scenario)

        {
            for (var i = 0; i < scenario.wayPoints.Length; ++i)
            {
                var pin = new Pin
                {
                    // Sets the color of the pin to green if it's the start waypoint. Otherwise red.
                    Color = (i == 0) ? Colors.Green : Colors.Red,
                    Width = 40,
                    Height = 40,
                    // Sets the name of the pin to Start it it's the start waypoint. Otherwise to End.
                    Name = (i == 0) ? "Start" : "End",
                };

                // Sets Anchor and Location of the pin.
                ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
                ShapeCanvas.SetLocation(pin, new System.Windows.Point(scenario.wayPoints[i].x, scenario.wayPoints[i].y));

                // Sets tooltip of the pin to Start if it is the start waypoint. Otherwise to End.
                ToolTipService.SetToolTip(pin, pin.Name);

                // Adds the pin to the layer.
                shapeLayer.Shapes.Add(pin);
            }
        }

        /// <summary>
        /// Calculates a route and displays the calculated polygons in the map.
        /// </summary>
        /// <param name="scenario">Scenario which defines the route to calculate.</param>
        /// <param name="calculateWithFeatureLayer">Option if the currently selected Feature Layer theme should be used.</param>
        private void CalculateRoute(Scenario scenario, bool calculateWithFeatureLayer)
        {
            XRouteWS xRoute = XServerClientFactory.CreateXRouteClient(Settings.Default.XUrl);

            try
            {
                var now = DateTime.Now;
                var nowString = $"{now.Year}-{now.Month}-{now.Day}T{now.Hour}:{now.Minute}:{now.Second}+00:00";

                var response = xRoute.calculateRoute(new calculateRouteRequest
                {
                    ArrayOfWaypointDesc_1 = scenario.wayPoints.Select(p => new WaypointDesc
                    {
                        wrappedCoords = new[] {new Point {point = p}},
                        linkType = LinkType.NEXT_SEGMENT,
                        fuzzyRadius = 10000
                    }).ToArray(),

                    ResultListOptions_4 = new ResultListOptions { dynamicInfo = calculateWithFeatureLayer, polygon = true },
                    ArrayOfRoutingOption_2 = new[] { new RoutingOption { parameter = RoutingParameter.START_TIME, value = nowString } },

                    CallerContext_5 = new CallerContext
                    {
                        wrappedProperties = new[]
                        {
                            new CallerContextProperty { key = "CoordFormat", value = "OG_GEODECIMAL" },
                            new CallerContextProperty { key = "Profile", value = "truckfast" },
                            new CallerContextProperty { key = "ProfileXMLSnippet", value = calculateWithFeatureLayer ? FeatureLayerXElement.ToString() : DeactivatedXElement.ToString() }
                        }
                    }
                });

                map.Dispatcher.BeginInvoke(new Action<Route>(DisplayRouteInMap), response.result);
            }
            catch (EntryPointNotFoundException) { System.Windows.MessageBox.Show(Properties.Resources.ErrorRouteCalculationDefault); }
            catch (System.ServiceModel.FaultException<XRouteException> faultException)
            {
                var s = faultException.Detail.stackElement;
                System.Windows.MessageBox.Show((s.errorKey == null || s.errorKey == "2530") ? Properties.Resources.ErrorRouteCalculationDefault : s.message);
            }
            catch { System.Windows.MessageBox.Show(Properties.Resources.ErrorRouteCalculationDefault); }
        }

        /// <summary> Shows the calculated route as a RoutePolyline on screen. </summary>
        /// <param name="route">The calculated route.</param>
        private void DisplayRouteInMap(Route route)
        {
            if (route == null) return;

            var points = new PointCollection();
            foreach (PlainPoint p in route.polygon.lineString.wrappedPoints)
                points.Add(new System.Windows.Point(p.x, p.y));

            new RoutePolyline(shapeLayer)
            {
                Points = points,
                ToolTip = $"{route.info.distance / 1000.0:0,0.0}km\n{TimeSpan.FromSeconds(route.info.time)}",
                Color = (route.dynamicInfo != null) ? Colors.Green : Colors.Gray,
                Width = (route.dynamicInfo != null) ? 25 : 15
            };

            ZoomToRoute(route);
        }

        /// <summary> Sets the map view to the extend of route. A frame of additional 20 percent of the minimal bounding rectangle
        /// is used to offset the route from the map window borders. </summary>
        /// <param name="route">The route object which should be centered in the map view.</param>
        private void ZoomToRoute(Route route)
        {
            var winPoints = from plainPoint in route.polygon.lineString.wrappedPoints
                            select new System.Windows.Point(plainPoint.x, plainPoint.y);

            map.SetEnvelope(new MapRectangle(winPoints).Inflate(1.2));
        }
    }
}
