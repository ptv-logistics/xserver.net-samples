//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Tools;
using xserver;

namespace Ptv.XServer.Demo.Tools
{
    /// <summary>
    /// Helper class providing shortcuts for inserting different types of layers into the map.
    /// </summary>
    public class XynLayer : XMapLayer
    {
        public XynLayer(XMapMetaInfo xMapMetaInfo, string layerName, string layerCaption)
            : base(layerName, xMapMetaInfo.Url, xMapMetaInfo.User, xMapMetaInfo.Password)
        {
            Caption = layerCaption;
            MinLevel = 14;
            Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/RoadEditor.png");
        }

        public void SetXYN(string xyn)
        {
            if(string.IsNullOrEmpty(xyn))
            {
                CustomXMapLayers = null;
                Refresh();
                return;
            }

            CustomXMapLayers = new xserver.Layer[] {
                     new xserver.GeometryLayer
                    {
                        name="XYN_LAYER",
                        drawPriority = 1000,
                        visible = true,
                        wrappedGeometries = new Geometries [] { new Geometries {wrappedGeometries = new Geometry[] {
                            new GeometryExt {
                                geometryString=xyn
                            }
                        }
                        ,
                        wrappedOptions  = new [] {
                            new GeometryOption {option = GeometryOptions.BORDERLINECOLOR, value = "#FF0000" },
                            new GeometryOption {option = GeometryOptions.LINEWIDTH, value = "5" },
                        } } }
                    }
                };

            Refresh();
        }
    }
}
