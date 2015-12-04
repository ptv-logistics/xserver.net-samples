//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Routing;
using System.Windows;

namespace Ptv.XServer.Demo.UseCases.RoutingDragAndDrop
{
    public class RoutingDragAndDropUseCase
    {
        private RouteLayer layer;
        private WpfMap wpfMap;

        public RoutingDragAndDropUseCase(WpfMap wpfMap, string xRouteUrl, string user = "", string password = "")
        {
            this.wpfMap = wpfMap;
            layer = new RouteLayer(wpfMap, xRouteUrl, user, password);
        }

        public void SetRoute(Point startPoint, Point destPoint)
        {
            layer.First = new Stop(layer, startPoint, RouteLayer.STOP_STYLE);
            layer.Last = new Stop(layer, destPoint, RouteLayer.STOP_STYLE);            
        }

        public void Enable()
        {
            wpfMap.Layers.Add(layer);
        }
       
        public void Disable()
        {
            wpfMap.Layers.Remove(layer);
        }
    }
}
