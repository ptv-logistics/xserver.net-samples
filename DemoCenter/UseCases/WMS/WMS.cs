// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Tools;


namespace Ptv.XServer.Demo.UseCases.WMS
{
    /// <summary>
    /// Provides the functionality to add a WMS Layer to a WpfMap.
    /// </summary>
    public class WMSUseCase : UseCase
    {
        const string firstWmsLayerName = "GeobaseDataHessianAdministration";
        const string secondWmsLayerName = "Terrestris";

        bool isEnabled;

        /// <summary>
        /// Adds a WMS Layer to the specified WpfMap.
        /// </summary>
        protected override void Enable()
        {
            EnableGeobaseData();
            EnableBusStops();

            // Zoom to Frankfurt
            wpfMap.SetMapLocation(new Point(8.682222, 50.110556), 17);
            isEnabled = true;
        }

        private void EnableGeobaseData()
        {
            if (wpfMap.Layers.Contains(wpfMap.Layers[firstWmsLayerName])) return;

            #region doc:wms
            const string urlTemplate = "http://www.gds-srv.hessen.de/cgi-bin/lika-services/ogc-free-maps.ows" +
                                       "?VERSION=1.1.1&REQUEST=GetMap&SERVICE=WMS&LAYERS=adv_alk&STYLES=,&SRS=EPSG:25832&BBOX=${boundingbox}&WIDTH=${width}&HEIGHT=${height}" +
                                       "&FORMAT=image/png&BGCOLOR=0xffffff&TRANSPARENT=FALSE&EXCEPTIONS=application/vnd.ogc.se_inimage";

            wpfMap.Layers.Add(new WmsLayer(urlTemplate, UseTiledVersionOfGeobaseData, true, firstWmsLayerName, "© dgs-srv hessen")
            {
                Caption = "Geobase data of Hessian administration",
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
            });
            #endregion
        }

        private void EnableBusStops()
        {
            if (wpfMap.Layers.Contains(wpfMap.Layers[secondWmsLayerName])) return;

            const string urlTemplate = "http://ows.terrestris.de/osm-haltestellen?LAYERS=OSM-Bushaltestellen&TRANSPARENT=true&FORMAT=image/png&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&STYLES=&SRS=EPSG:3857";

            // For demonstration of proxy settings some lines of code later
            // const string urlTemplate = "http://80.146.239.180/WMS/WMS?REQUEST=GetMap&format=image/png&version=1.1.1&layers=xmap-plain&srs=EPSG:505456&styles=";
            
            var wmsLayer = new WmsLayer(urlTemplate, false, false, secondWmsLayerName, "© terrestris")
            {
                Caption = "Bus stops",
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
            };
            wpfMap.Layers.Add(wmsLayer);

            // code sample demonstrating the settings for configuring a proxy for the web requests
            // const string domain = "ska-lx-test02";
            // const string port = "3128";
            // wmsLayer.ReprojectionProvider.Proxy = new WebProxy(string.Format("http://{0}:{1}", domain, port)) { Credentials = new NetworkCredential("marvin", "marvin", domain) };
        }

        /// <summary> Deletes the WMS layer from the WpfMap. </summary>
        protected override void Disable()
        {
            isEnabled = false;

            DisableGeobaseData();
            DisableBusStops();
        }

        private void DisableGeobaseData() { wpfMap.Layers.Remove(wpfMap.Layers[firstWmsLayerName]); }
        private void DisableBusStops() { wpfMap.Layers.Remove(wpfMap.Layers[secondWmsLayerName]); }

        private bool useTiledVersionOfGeobaseData;

        /// <summary> Enable the tiled version of the Layer 'Geobase data of Hessian Administration'". </summary>
        public bool UseTiledVersionOfGeobaseData
        {
            get { return useTiledVersionOfGeobaseData;}
            set
            {
                useTiledVersionOfGeobaseData = value;
                if (!isEnabled) return;
             
                DisableGeobaseData();
                EnableGeobaseData();
            }
        }
    }
}