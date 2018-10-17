// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows.Controls;
using System.Windows.Media;
using Ptv.XServer.Demo.XlocateService;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Canvases;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Data container class for exchanging data between the GeocodeResultsLayer and the business logic code. </summary>
    /// <typeparam name="T"> Type of the contained data. </typeparam>
    public class DataContainer<T>
    {
        /// <summary> Gets or sets the data of the container. </summary>
        public T Data { get; set; }
    }
    
    /// <summary> Layer displaying the geocoding results. </summary>
    public class GeocodeResultsLayer : WorldCanvas
    {
        #region private variables
        /// <summary> Transformation object to adjust the scale of the displayed objects. </summary>
        private readonly ScaleTransform adjustTransform;
        #endregion

        #region public variables
        /// <summary> Gets or sets the geocoding results. </summary>
        public DataContainer<AddressResponse> AddressResponse { get; set; }

        /// <summary> Gets or sets the map. </summary>
        public new MapView MapView { get; set; }
        
        /// <summary> Gets or sets the color of the pins. </summary>
        public Color PinColor { get; set; }
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="GeocodeResultsLayer"/> class. </summary>
        /// <param name="mapView"> Map where the geocoding results are displayed on. </param>
        public GeocodeResultsLayer(MapView mapView)
            : base(mapView)
        {
            PinColor = Colors.Red;
            MapView = mapView;
            // initialize tranformation for power-law scaling of symbols
            adjustTransform = new ScaleTransform();
        }
        #endregion

        #region helper methods
        /// <summary> Adjust the transformation for logarithmic scaling. </summary>
        private void AdjustTransform()
        {
            // if the factor is 1.0, the elements have pixel size
            // if the factor is 0.0, the elements have Mercator size
            // for a factor between 0.0 and 1.0, the elements are scaled with a logarithmic multiplicator
            const double logicalScaleFactor = .85;

            adjustTransform.ScaleX = 5 * Math.Pow(MapView.CurrentScale, logicalScaleFactor);
            adjustTransform.ScaleY = 5 * Math.Pow(MapView.CurrentScale, logicalScaleFactor);
        }

        /// <summary> Builds up the layer content. Adds a pin to the map for each geocoding result. </summary>
        private void UdpatePins()
        {
            Children.Clear();

            if (AddressResponse?.Data == null)
                return;

            // Add a pin to the map for each result.
            foreach (ResultAddress address in AddressResponse.Data.wrappedResultList)
            {
                // transform wgs to ptv Mercator coordinate
                System.Windows.Point mapPoint = GeoToCanvas(new System.Windows.Point(address.coordinates.point.x, address.coordinates.point.y));

                var pin = new Pin
                {
                    Color = PinColor,
                    ToolTip = "Pin",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                    Height = 20,
                    Width = 20,
                    RenderTransformOrigin = new System.Windows.Point(1, 1), // origin is lower right
                    RenderTransform = adjustTransform

                };
                ToolTipService.SetToolTip(pin, $"{address.postCode} {address.city} {address.city2} {address.street} {address.houseNumber}");
                SetLeft(pin, mapPoint.X - pin.Width);
                SetTop(pin, mapPoint.Y - pin.Height);
                Children.Add(pin);
            }
        }
        #endregion

        #region public methods
        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            if (updateMode == UpdateMode.Refresh)
                UdpatePins();

            AdjustTransform();
        }
        #endregion
    }
}
