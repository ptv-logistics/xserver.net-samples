// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Tools;

using Map = Ptv.XServer.Controls.Map.Map;

namespace Ptv.XServer.Demo.Tools
{
    /// <summary>
    /// Helper class providing shortcuts for inserting different types of layers into the map.
    /// </summary>
    public static class XServerInternetLayers
    {
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
                MinLevel = 16, // Minimal level (defined by Google), for which Poi objects are shown. 
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

        public static void InsertDataManagerLayer(this Map map, XMapMetaInfo xMapMetaInfo, string layerName, string layerId, string layerCaption, int minZoom = 0, bool markerIsBalloon = false)
        {
            // always use xmap-eu
            var url = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap/";
            var layer = new XMapLayer(layerName, url, xMapMetaInfo.User, xMapMetaInfo.Password)
            {
                Caption = layerCaption,
                MaxRequestSize = new System.Windows.Size(2048, 2048),
                MinLevel = minZoom,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/POI.png"),
                CustomXMapLayers = new xserver.Layer[] {  
                    new xserver.SMOLayer
                    { 
                        name = layerId + "." + layerId,
                        // Request REFERENCEPOINT based object information to provide tool tips on the icons.
                        objectInfos = xserver.ObjectInfoType.REFERENCEPOINT, 
                        visible = true 
                    }
                },
                Profile = "ajax-av",
                CustomCallerContextProperties = new[] { new xserver.CallerContextProperty
                {
                    key = "ProfileXMLSnippet", value = "/profiles/datamanager/xmap/" + layerId
                } },
                MarkerIsBalloon = markerIsBalloon
            };

            map.Layers.Add(layer);
        }
    }
}
