// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Layers.Tiled;

namespace Ptv.XServer.Demo.UseCases.Here
{
    /// <summary>
    /// Provides the functionality to add a HERE layer to a WpfMap.
    /// </summary>
    public class HereUseCase : UseCase
    {
        /// <summary>
        /// Tries to create a HERE layer with specified account information. Throws an exception if the key is wrong.
        /// </summary>
        protected override void Enable()
        {
            #region doc:HereSatelliteView
            const string hereStatelliteUrl = "http://{0}.aerial.maps.api.here.com/maptile/2.1/maptile/newest/satellite.day/{3}/{1}/{2}/256/png8?app_id={app_id}&app_code={app_code}";

            var metaInfo = new HereMetaInfo(Properties.Settings.Default.HereAppId, Properties.Settings.Default.HereAppCode);
            if (string.IsNullOrEmpty(metaInfo.AppCode) || string.IsNullOrEmpty(metaInfo.AppId))
            {
                // Throws an exception if no valid access information was specified.
                throw new Exception("Please enter valid HERE access information.");
            }

            // Insert on top of xServer background.
            var idx = wpfMap.Layers.IndexOf(wpfMap.Layers["Background"]) + 1;

            var hereUrl = hereStatelliteUrl.Replace("{app_id}", metaInfo.AppId).Replace("{app_code}", metaInfo.AppCode);

            var hereSatelliteLayer = new TiledLayer("HERE_Satellite")
            {
                TiledProvider = new RemoteTiledProvider
                {
                    MinZoom = 0,
                    MaxZoom = 20,
                    RequestBuilderDelegate = (x, y, z) => String.Format(hereUrl, (x + y) % 4 + 1, x, y, z)
                },
                IsBaseMapLayer = true,
                Opacity = .8,
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Aerials.png"),
                Copyright = "© HERE",
                Caption = "Here Satellite",
            };

            wpfMap.Layers.Insert(idx, hereSatelliteLayer);

            #endregion
        }

        /// <summary>
        /// Removes the HERE layer from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["HERE_Satellite"]);
        }
    }

    public class HereMetaInfo
    {
        public string AppId { get; set; }
        public string AppCode { get; set; }

        public HereMetaInfo(string AppId, string AppCode)
        {
            this.AppId = AppId;
            this.AppCode = AppCode;
        }
    }
}
