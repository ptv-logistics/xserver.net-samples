// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows;
using System.Linq;
using Ptv.XServer.Demo.XlocateService;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Demo.Tools;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Demonstrates how the geocoding of a multi field input can be implemented. </summary>
    public class MultiFieldGeocoder : GeocoderBase
    {
        /// <summary> Initializes a new instance of the <see cref="MultiFieldGeocoder"/> class. Adds a new layer which
        /// can later contain the geocoding results to the map. </summary>
        /// <param name="errDlg"> The delegate method to call in case of an error. </param>
        /// <param name="succDlg"> The delegate method to call to propagate results. </param>
        /// <param name="wpfMap"> The map where the result layer is added to. </param>
        public MultiFieldGeocoder(ErrorDelegate errDlg, SuccessDelegate succDlg, WpfMap wpfMap)
            : base(errDlg, succDlg, wpfMap)
        {
        }

        /// <summary> Triggers the geocoding of the given multi field data. The call is asynchronously i.e. the caller
        /// is not blocked. The result is stored in the <see cref="GeocoderBase.Addresses"/> property. </summary>
        /// <param name="data"> The data to geocode. </param>
        public void LocateMultiField(MultiFieldData data)
        {
            Addresses.Clear();

            if (data.IsEmpty()) return;

            #region doc:call xlocate
            XLocateWS xLocate = XServerClientFactory.CreateXLocateClient(Properties.Settings.Default.XUrl);

            var address = new Address
            {
                country = data.Country,
                state = data.State,
                postCode = data.PostalCode,
                city = data.City,
                street = data.Street
            };
            try
            {
                xLocate.BeginfindAddress(new findAddressRequest
                {
                    Address_1 = address,
                    CallerContext_5 = new CallerContext
                    {
                        wrappedProperties = new[] { new CallerContextProperty { key = "CoordFormat", value = "PTV_MERCATOR" } }
                    },
                }, LocateMultiFieldComplete, xLocate);
            }
            catch (EntryPointNotFoundException)
            {
                errorDelegate.Invoke(Properties.Resources.ErrorGeocodeEndpointNotFound);
            }
            catch (Exception ex)
            {
                errorDelegate.Invoke(ex.Message);
            }
            #endregion
        }
        
        /// <summary> Callback for asynchronous response processing of the geocoding request. </summary>
        /// <param name="result"> The XLocateWS instance to be used for processing the result. </param>
        private void LocateMultiFieldComplete(IAsyncResult result)
        {
            #region doc:evaluate response
            try
            {
                findAddressResponse response = (result.AsyncState as XLocateWS)?.EndfindAddress(result);

                Addresses = response?.result.wrappedResultList.ToList();
                Application.Current.Dispatcher.BeginInvoke(new Action(UdpatePins));

                if (response != null && response.result.errorCode < 0)
                    errorDelegate.Invoke(response.result.errorDescription);
                else
                    successDelegate.Invoke(this);
            }
            catch (EntryPointNotFoundException)
            {
                errorDelegate.Invoke(Properties.Resources.ErrorGeocodeEndpointNotFound);
            }
            catch (Exception ex)
            {
                errorDelegate.Invoke(ex.Message);
            }
            #endregion
        }
    }
}
