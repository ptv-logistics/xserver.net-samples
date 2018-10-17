// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Address data which can be entered for a multi field geocoding. </summary>
    public class MultiFieldData
    {
        /// <summary> Gets or sets the country of the search address. </summary>
        public string Country { get; set; }

        /// <summary> Gets or sets the state of the search address. </summary>
        public string State { get; set; }

        /// <summary> Gets or sets the postal code of the search address. </summary>
        public string PostalCode { get; set; }

        /// <summary> Gets or sets the city of the search address. </summary>
        public string City { get; set; }

        /// <summary> Gets or sets the street of the search address. </summary>
        public string Street { get; set; }

        /// <summary> Checks whether the search address is empty. </summary>
        /// <returns> A value indicating whether the search address is empty. </returns>
        public bool IsEmpty()
        {
            bool result = true;
            result &= string.IsNullOrEmpty(Country);
            result &= string.IsNullOrEmpty(State);
            result &= string.IsNullOrEmpty(PostalCode);
            result &= string.IsNullOrEmpty(City);
            result &= string.IsNullOrEmpty(Street);
            
            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Country} / {State} / {PostalCode} / {City} / {Street}";
        }
    }
}
