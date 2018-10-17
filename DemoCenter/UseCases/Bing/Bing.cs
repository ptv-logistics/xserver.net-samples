// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Net;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo.UseCases.Bing
{
    /// <summary>
    /// Provides the functionality to add a Bing layer to a WpfMap.
    /// </summary>
    public class BingUseCase
    {
        /// <summary>
        /// Tries to create a Bing layer with specified key. Throws an exception if the key is wrong.
        /// </summary>
        /// <param name="wpfMap">WpfMap object to which the Bing layer will be added.</param>
        /// <param name="bingKey">Key for the Bing map provider.</param>
        public void CreateBingLayer(WpfMap wpfMap, string bingKey)
        {
            if(string.IsNullOrEmpty(bingKey))
            {
                // Throws an exception if no valid Bing key was specified.
                throw new Exception("Please enter a valid Microsoft Bing access key first!");
            }

            #region doc:Bing

            // Insert on top of xServer background.
            var idx = wpfMap.Layers.IndexOf(wpfMap.Layers["Background"]) + 1;

            try
            {
                // Adds a new layer called "Bing" to the WpfMap.
                wpfMap.AddBingLayer("Bing", idx, bingKey, BingImagerySet.Aerial, BingMapVersion.v1, true, .8, 
                    ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Aerials.png"));
            }
            catch (WebException we)
            {
                if (((HttpWebResponse)(we.Response)).StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Throws an exception if authorization failed.
                    throw new Exception("The entered Microsoft Bing access key seems to be wrong!");
                }
            }          
            #endregion // doc:Bing
        }

        /// <summary>
        /// Deletes the Bing layer from the WpfMap.
        /// </summary>
        /// <param name="wpfMap">WpfMap object from which the Bing layer will be removed.</param>
        public void Reset(WpfMap wpfMap)
        {
            wpfMap.RemoveBingLayer("Bing");
        }
    }
}
