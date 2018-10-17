// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> <para>Demonstrates the usage of the xLocate server. To support the AutoCompleteTextBox control, this class
    /// exposes some public properties. Use AddTo() and Remove() to add or remove a geocoder to/from the map. The
    /// Remove() call is essential for cleaning up resources.</para>
    /// <para>See the <conceptualLink target="fe48cb51-c6ce-487e-b4c0-168537c184c3"/> topic for an example.</para> </summary>
    public class GeocoderDemo : INotifyPropertyChanged
    {
        #region private fields and properties
        /// <summary> Wait message which is displayed while the xLocate server is queried for suggestions. </summary>
        private readonly List<string> _WaitMessage = new List<string> { "Please Wait..." };

        /// <summary> The map where the geocoding results are displayed. </summary>
        private WpfMap _wpfMap;
        
        /// <summary> Backing field for the QueryText property. Stores the query text of a single field geocoding
        /// request. </summary>
        private string _QueryText;
        
        /// <summary> The geocoder instance for single field geocoding. </summary>
        private SingleFieldGeocoder sfg;
        
        /// <summary> The geocoder instance for multi field geocoding. </summary>
        private MultiFieldGeocoder mfg;
        #endregion

        #region public properties
        /// <summary> Gets the wait message which is shown while the xLocate server is queried for suggestions. </summary>
        public IEnumerable WaitMessage => _WaitMessage;

        /// <summary> Gets or sets the query for single field geocoding. </summary>
        public string QueryText
        {
            get { return _QueryText; }
            set
            {
                if (_QueryText == value) return;

                _QueryText = value;
                OnPropertyChanged("QueryText");
                sfg.Suggestions = null;
                OnPropertyChanged("Suggestions");
                Debug.Print("QueryText: " + value);
            }
        }

        /// <summary> Gets the suggestions for single field geocoding. </summary>
        public IEnumerable Suggestions
        {
            get
            {
                Debug.Print("---" + sfg?.Suggestions);

                if (!string.IsNullOrEmpty(QueryText) && QueryText.Length >= 3)
                    sfg?.Suggest(QueryText);

                return sfg != null ? sfg.Suggestions ?? new string[] { } : new string[] { };
            }
        }

        #endregion

        #region private API
        /// <summary> Makes the geocoding result visible on the map. The map display section is adapted to the map
        /// rectangle containing all results. </summary>
        /// <param name="sender"> The geocoder instance which is providing results. </param>
        private void SetMapEnvelopeToResult(GeocoderBase sender)
        {
            #region doc:bring result into view
            if (Dispatcher.FromThread(Thread.CurrentThread) != null)
            {
                sender.ContentLayer.Refresh();

                var resultList = sender.Addresses;

                if (resultList.Count > 1)
                {
                    var winPoints = from address in resultList
                                    select GeoTransform.PtvMercatorToWGS(new Point(address.coordinates.point.x, address.coordinates.point.y));

                    _wpfMap.SetEnvelope(new MapRectangle(winPoints).Inflate(1.2));
                }
                else if (resultList.Count == 1)
                {
                    _wpfMap.SetMapLocation(GeoTransform.PtvMercatorToWGS(new Point(
                        resultList[0].coordinates.point.x,
                        resultList[0].coordinates.point.y)), _wpfMap.Zoom);
                }

                Mouse.OverrideCursor = null;
            }
            else
                Application.Current.Dispatcher.BeginInvoke(new Action<GeocoderBase>(SetMapEnvelopeToResult), sender);
            #endregion
        }

        /// <summary> Shows an error message box if something goes wrong during geocoding. </summary>
        /// <param name="errorMessage"> The error message to display. </param>
        private static void DisplayError(string errorMessage)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) != null)
            {
                MessageBox.Show(errorMessage);
                Mouse.OverrideCursor = null;
            }
            else
                Application.Current.Dispatcher.BeginInvoke(new Action<string>(DisplayError), errorMessage);
        }
        #endregion

        #region protected API
        /// <summary> Helper method to fire property changed events. </summary>
        /// <param name="prop"> The name of the changed property. </param>
        protected void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        #region public API

        /// <summary> Adds a geocoding results layer to the given map. </summary>
        /// <param name="wpfMap"> The map. </param>
        /// <param name="layerName"> The name of the geocoding layer. </param>
        /// <param name="singleField"> True for a single field geocoder, false for a multi field geocoder. </param>
        public void AddTo(WpfMap wpfMap, string layerName, bool singleField)
        {
            if (wpfMap == null)
                return;

            if (singleField)
            {
                sfg?.Remove(wpfMap);

                sfg = new SingleFieldGeocoder(DisplayError, SetMapEnvelopeToResult, wpfMap);
            }
            else
            {
                mfg?.Remove(wpfMap);

                mfg = new MultiFieldGeocoder(DisplayError, SetMapEnvelopeToResult, wpfMap);
            }

            _wpfMap = wpfMap;
        }

        /// <summary> Removes a geocoding results layer from the map. </summary>
        /// <param name="wpfMap"> The map. </param>
        /// <param name="singleField"> True if the single field geocoder should be removed, false if the multi field geocoder should be removed. </param>
        public void Remove(WpfMap wpfMap, bool singleField)
        {
            if (singleField)
            {
                sfg?.Remove(wpfMap);
                sfg = null;
            }
            else
            {
                mfg?.Remove(wpfMap);
                mfg = null;
            }
        }

        /// <summary> Performs a single field geocoding request. </summary>
        /// <param name="toLocate"> The query for the single field geocoding. </param>
        public void LocateSingleField(string toLocate)
        {
            if (sfg == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            sfg.LocateSingleField(toLocate);
        }

        /// <summary> Performs a multi field geocoding request. </summary>
        /// <param name="data"> The data for the multi field geocoding. </param>
        public void LocateMultiField(MultiFieldData data)
        {
            if (mfg == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            mfg.LocateMultiField(data);
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
