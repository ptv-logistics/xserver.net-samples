//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Tools;
using System.Text.RegularExpressions;
using ServerSideRendering;
using xserver;
using Map = Ptv.XServer.Controls.Map.Map;

namespace Ptv.XServer.Demo.Tools
{
    /// <summary>
    /// Helper class providing shortcuts for inserting different types of layers into the map.
    /// </summary>
    public static class XServerInternetLayers
    {
        /// <summary>
        /// Inserts a road editor layer into the map. At the very base, "Truck Attributes" is road editor content.
        /// </summary>
        /// <param name="map">The new road editor layer will be inserted into this map instance, used as an extension.</param>
        /// <param name="xMapMetaInfo">Meta information, containing the xMap Server URL and authentication information, needed for layer access.</param>
        /// <param name="layerName">The name used in the map control to identify the layer.</param>
        /// <param name="xmapName">The xMap Server layer name</param>
        /// <param name="xmapProfile">The xMap Server profile to use. See remarks below.</param>
        /// <param name="layerCaption">The layer caption, used e.g. in the quick settings of the map control.</param>
        /// <remarks>
        /// Due to a restriction of xMap Server, road editor rendering is dependent on the street layer and requires
        /// the street layer to be visible. If the street layer is invisible, the road editor content will be invisible 
        /// also. By default, the provider used by XMapLayer turns the street layer off, so it must explicitly be enabled 
        /// through XMapLayer.CustomXMapLayers. As a side effect, enabling the street layer makes its contents visible in 
        /// the map. To hide this content, to display only the road editor content, a special rendering profile will 
        /// usually be used. For this reason this method provides an additional profile parameter where the rendering 
        /// profile can be specified.
        /// 
        /// The demo server incorporated into this sample was deployed with the necessary profiles to display Truck 
        /// Attributes correctly. The xMap Server profile along with its rendering profile can be viewed / downloaded 
        /// using the following management console links:
        /// 
        /// xMap profile: 
        /// https://xroute-eu-n-test.cloud.ptvgroup.com/xroute/rrxmap/pages/viewConfFileFormatted.jsp?name=xmap-truckattributes.properties
        /// 
        /// Corresponding rendering profile:
        /// https://xroute-eu-n-test.cloud.ptvgroup.com/xroute//rrxmap/pages/viewConfFileFormatted.jsp?name=truckattributes.ini
        /// </remarks>
        public static void InsertRoadEditorLayer(this Map map, XMapMetaInfo xMapMetaInfo, string layerName, string xmapName, string xmapProfile, string layerCaption)
        {
            var roadEditorLayer = new XMapLayer(layerName, xMapMetaInfo.Url, xMapMetaInfo.User, xMapMetaInfo.Password)
            {
                Caption = layerCaption,
                Profile = xmapProfile,
                MaxRequestSize = new System.Windows.Size(2048, 2048),
                MinLevel = 14,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/RoadEditor.png"),
                CustomXMapLayers = new xserver.Layer[] {  
                    new xserver.RoadEditorLayer { 
                        // Road Editor element. Request REFERENCEPOINT based object 
                        // information to provide tool tips on the signs.
                        name = xmapName, 
                        // Request REFERENCEPOINT based object information to 
                        // provide tool tips on the signs.
                        objectInfos=xserver.ObjectInfoType.REFERENCEPOINT, 
                        visible = true                        
                    },
                    new xserver.StaticPoiLayer {
                        // see remarks above about street layer element
                        name = "street", 
                        visible = true, 
                        category = -1, 
                        detailLevel = 0 
                    }
                }
            };

            map.Layers.Add(roadEditorLayer);
        }

        /// <summary>
        /// Inserts a Traffic Information layer into the map.
        /// </summary>
        /// <param name="map">The new Traffic Information layer will be inserted into this map instance, used as an extension.</param>
        /// <param name="xMapMetaInfo">Meta information, containing the xMap Server URL and authentication information, needed for layer access.</param>
        /// <param name="layerName">The name used in the map control to identify the layer.</param>
        /// <param name="xmapName">The xMap Server layer name. See remarks below.</param>
        /// <param name="layerCaption">The layer caption, used e.g. in the quick settings of the map control.</param>
        /// <remarks>
        /// The Traffic Information layer does not require a special profile. Note that the layer requests 
        /// object information of type xserver.ObjectInfoType.FULLGEOMETRY to enable tool tips along the 
        /// lines displayed.
        /// 
        /// xmapName has to be provided in the form &lt;configuration&gt;.&lt;layername&gt;[.&lt;profile&gt;][;&lt;SQL Filter&gt;] as 
        /// specified by xMap Server. Please refer to the xMap Server documentation for details.
        /// </remarks>
        public static void InsertTrafficInfoLayer(this Map map, XMapMetaInfo xMapMetaInfo, string layerName, string xmapName, string layerCaption)
        {
            var layer = new XMapLayer(layerName, xMapMetaInfo.Url, xMapMetaInfo.User, xMapMetaInfo.Password)
            {
                Caption = layerCaption,
                MaxRequestSize = new System.Windows.Size(2048, 2048),
                MinLevel = 10,
                CustomXMapLayers = new xserver.Layer[] {  
                    new xserver.SMOLayer
                    { 
                        name = xmapName, 
                        // request object information of type FULLGEOMETRY 
                        // to enable tool tips along the lines displayed.
                        objectInfos=xserver.ObjectInfoType.FULLGEOMETRY, 
                        visible = true 
                    }
                }
            };

            map.Layers.Add(layer);
        }

        /// <summary>
        /// Inserts a POI layer into the map.
        /// </summary>
        /// <param name="map">The new POI layer will be inserted into this map instance, used as an extension.</param>
        /// <param name="xMapMetaInfo">Meta information, containing the xMap Server URL and authentication information, needed for layer access.</param>
        /// <param name="layerName">The name used in the map control to identify the layer.</param>
        /// <param name="xmapName">The xMap Server layer name. See remarks below.</param>
        /// <param name="layerCaption">The layer caption, for example used in the quick settings of the map control.</param>
        /// <remarks>
        /// The POI layer does not require a special profile. xmapName has to be provided in the form 
        /// &lt;configuration&gt;.&lt;layername&gt;[.&lt;profile&gt;][;&lt;SQL Filter&gt;] as specified by xMap Server. Please 
        /// refer to the xMap Server documentation for details.
        /// </remarks>
        public static void InsertPoiLayer(this Map map, XMapMetaInfo xMapMetaInfo, string layerName, string xmapName, string layerCaption)
        {
            var layer = new XMapLayer(layerName, xMapMetaInfo.Url, xMapMetaInfo.User, xMapMetaInfo.Password)
            {
                Caption = layerCaption,
                MaxRequestSize = new System.Windows.Size(2048, 2048),
                MinLevel = 14,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/POI.png"),
                CustomXMapLayers = new xserver.Layer[] {  
                    new xserver.SMOLayer
                    { 
                        name = xmapName, 
                        // Request REFERENCEPOINT based object information to provide tool tips on the icons.
                        objectInfos = xserver.ObjectInfoType.REFERENCEPOINT, 
                        visible = true 
                    }
                }
            };

            map.Layers.Add(layer);
        }
    }
}
