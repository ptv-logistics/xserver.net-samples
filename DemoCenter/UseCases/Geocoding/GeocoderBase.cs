// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Collections.Generic;
using Ptv.XServer.Demo.XlocateService;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Windows.Media;
using System.Windows.Controls;
using Ptv.XServer.Controls.Map.Symbols;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Delegate for notifying error messages. </summary>
    /// <param name="msg"> The occurred error message.</param>
    public delegate void ErrorDelegate(string msg);
    
    /// <summary> Delegate for notifying success. </summary>
    /// <param name="sender"> The geocoding instance which provides results.</param>
    public delegate void SuccessDelegate(GeocoderBase sender);

    /// <summary> Base class for geocoding. </summary>
    public class GeocoderBase
    {
        #region protected fields

        /// <summary> Method called if an error occurs.</summary>
        protected ErrorDelegate errorDelegate;
        
        /// <summary> Method called to propagate results.</summary>
        protected SuccessDelegate successDelegate;

        #endregion

        #region public properties
        /// <summary> Gets or sets the color of the pins. Each geocoding result is marked on the map by a pin. </summary>
        public Color PinColor { get; set; }
        
        /// <summary> Gets or sets the layer containing the geocoding results. </summary>
        public ShapeLayer ContentLayer { get; set; }
        
        /// <summary> List of geocoding result addresses. </summary>
        public List<ResultAddress> Addresses = new List<ResultAddress>();
        
        #endregion

        /// <summary> Initializes a new instance of the <see cref="GeocoderBase"/> class. Adds a new layer which can
        /// later contain the geocoding results to the map. </summary>
        /// <param name="errorDelegate"> The delegate method to call in case of an error. </param>
        /// <param name="successDelegate"> The delegate method to call to propagate results. </param>
        /// <param name="map"> The map where the result layer is added to. </param>
        public GeocoderBase(ErrorDelegate errorDelegate, SuccessDelegate successDelegate, IMap map)
        {
            this.errorDelegate = errorDelegate;
            this.successDelegate = successDelegate;
            PinColor = Colors.Blue;
            
            #region doc:add result layer
            ContentLayer = new ShapeLayer("Addresses") { SpatialReferenceId = "PTV_MERCATOR" };
            map.Layers.Add(ContentLayer);
            #endregion
        }

        /// <summary> Builds up the layer content. Shows a pin for each geocoded address on the map. </summary>
        protected void UdpatePins()
        {
            #region doc:display pins for addresses
            ContentLayer.Shapes.Clear();

            if (Addresses == null || Addresses.Count == 0)
                return;

            // Add a pin to the map for each result.
            foreach (ResultAddress address in Addresses)
            {
                var pin = new Pin
                {
                    Color = PinColor,
                    Height = 40,
                    Width = 40
                };
                ToolTipService.SetToolTip(pin, $"{address.postCode} {address.city} {address.city2} {address.street} {address.houseNumber}");
                ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
                ShapeCanvas.SetLocation(pin, new System.Windows.Point(address.coordinates.point.x, address.coordinates.point.y));
                ContentLayer.Shapes.Add(pin);
            }

            ContentLayer.Refresh();
            #endregion
        }

        #region public API

        /// <summary> Removes the layer containing the geocoding results from the LayerCollection. </summary>
        /// <param name="wpfMap"> The map which contains the layer. </param>
        public void Remove(WpfMap wpfMap)
        {
            ContentLayer.Shapes.Clear();

            ContentLayer?.Refresh();

            #region doc:remove result layer

            wpfMap?.Layers.Remove(ContentLayer);

            ContentLayer = null;
            #endregion
        }

        #endregion
    }
}
