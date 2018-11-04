//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using xserver;

namespace Ptv.XServer.Demo.Tools
{
    /// <summary>
    /// Impementation of a custom xMapLayer class.
    /// </summary>
    public class XynLayer : UntiledLayer
    {
        public XynLayer(XMapMetaInfo meta, string layerName, string layerCaption)
            : base(layerName)
        {
            // note: the class XMapTiledProvider implements both the interface for tiled and non-tiled layers!
            UntiledProvider = new XMapTiledProvider(meta.Url, XMapMode.Custom)
            {
                User = meta.User,
                Password = meta.Password,
            };

            Caption = layerCaption;
            MinLevel = 4;
            Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/RoadEditor.png");
        }

        public void SetXYN(string xyn)
        {
            if (string.IsNullOrEmpty(xyn))
            {
                ((XMapTiledProvider)UntiledProvider).CustomXMapLayers = null;
                Refresh();
                return;
            }

            ((XMapTiledProvider)UntiledProvider).CustomXMapLayers = new xserver.Layer[] {
                     new xserver.GeometryLayer
                    {
                        name="XYN_LAYER",
                        drawPriority = 1000,
                        visible = true,
                        wrappedGeometries = new Geometries [] { new Geometries {wrappedGeometries = new Geometry[] {
                            new GeometryExt {
                                geometryString=xyn
                            }
                        },
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
