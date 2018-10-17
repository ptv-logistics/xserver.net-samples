// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Linq;
using System.Collections;
using System.Windows;
using Ptv.XServer.Demo.XlocateService;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Demo.Tools;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Demonstrates how the geocoding of a single field input can be implemented. </summary>
    public class SingleFieldGeocoder : GeocoderBase
    {
        /// <summary> Gets or sets the suggestions. Suggestions are proposals for a correct search string. For example
        /// missing words are completed and typos removed. </summary>
        public IEnumerable Suggestions { get; set; }

        /// <summary> Initializes a new instance of the <see cref="SingleFieldGeocoder"/> class. Adds a new layer which
        /// can later contain the geocoding results to the map. </summary>
        /// <param name="errDlg"> The delegate method to call in case of an error. </param>
        /// <param name="succDlg"> The delegate method to call to propagate results. </param>
        /// <param name="wpfMap"> The map where the result layer is added to. </param>
        public SingleFieldGeocoder(ErrorDelegate errDlg, SuccessDelegate succDlg, WpfMap wpfMap)
            : base(errDlg, succDlg, wpfMap)
        {
        }

        /// <summary> Triggers the suggestion calculation for the given string. The call is synchronously i.e. the
        /// caller is blocked until the server responds. The result is stored in the <see cref="Suggestions"/> property. </summary>
        /// <param name="toLocate"> The string to look up suggestions for. </param>
        public void Suggest(string toLocate)
        {
            if (!string.IsNullOrEmpty(toLocate))
            {
                #region doc:suggest
                XLocateWS xLocate = XServerClientFactory.CreateXLocateClient(Properties.Settings.Default.XUrl);

                var response = xLocate.findSuggestion(new findSuggestionRequest
                {
                    String_1 = toLocate
                });

                Suggestions = response.result.wrappedSuggestionList;
                #endregion
            }
            else
                Suggestions = new Suggestion[] { };
        }

        /// <summary> Triggers the geocoding of the given string. The call is asynchronously i.e. the caller is not
        /// blocked. The result is stored in the <see cref="GeocoderBase.Addresses"/> property. </summary>
        /// <param name="toLocate"> The string to geocode. </param>
        public void LocateSingleField(string toLocate)
        {
            Addresses.Clear();

            if (string.IsNullOrEmpty(toLocate)) return;

            #region doc:call xlocate
            XLocateWS xLocate = XServerClientFactory.CreateXLocateClient(Properties.Settings.Default.XUrl);

            try
            {
                xLocate.BeginfindAddressByText(new findAddressByTextRequest
                {
                    String_1 = toLocate,
                    String_2 = Properties.Settings.Default.XUrl.ToUpper().Contains("-CN-N")?  "CHN " : null, // decarta needs Country for OpenLR geocoding
                    CallerContext_6 = new CallerContext
                    {
                        wrappedProperties = new[] { new CallerContextProperty { key = "CoordFormat", value = "PTV_MERCATOR" } }
                    }
                }, LocateSingleFieldComplete, xLocate);
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
        private void LocateSingleFieldComplete(IAsyncResult result)
        {
            #region doc:evaluate response
            try
            {
                findAddressByTextResponse response = (result.AsyncState as XLocateWS)?.EndfindAddressByText(result);

                Addresses = response?.result.wrappedResultList.ToList();
                Application.Current.Dispatcher.BeginInvoke(new Action(UdpatePins));

                if (response != null && response.result.errorCode < 0)
                {
                    errorDelegate.Invoke(response.result.errorDescription);
                }
                else
                {
                    successDelegate.Invoke(this);
                }
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
