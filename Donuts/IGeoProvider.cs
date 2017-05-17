//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;


namespace Ptv.XServer.Demo.MapMarket
{
    /// <summary> <para>This structure contains the data for a geographic item.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public struct GeoItem
    {
        /// <summary> Gets or sets the geometry well-known binary. </summary>
        public byte[] Wkb { get; set; }

        /// <summary> Gets or sets the id of the item. </summary>
        public object Id { get; set; }

        /// <summary> Gets or sets the minimum x coordinate of the item. </summary>
        public double XMin { get; set; }

        /// <summary> Gets or sets the maximum x coordinate of the item. </summary>
        public double YMin { get; set; }

        /// <summary> Gets or sets the minimum y coordinate of the item. </summary>
        public double XMax { get; set; }

        /// <summary> Gets or sets the maximum y coordinate of the item. </summary>
        public double YMax { get; set; }

        /// <summary> A dictionary for additional attributes. </summary>
        public Dictionary<string, object> Atributes;
    }

    /// <summary> <para>The interface for providing geographic data.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public interface IGeoProvider
    {
        /// <summary> Return all items whose envelope are within a bounding box. </summary>
        /// <param name="xmin"> The minimum x value of the bounding box. </param>
        /// <param name="ymin"> The maximum x value of the bounding box. </param>
        /// <param name="xmax"> The minimum y value of the bounding box. </param>
        /// <param name="ymax"> The maximum y value of the bounding box. </param>
        /// <param name="attributes"> Additional attributes to be delivered. </param>
        /// <returns> The enumeration of the GeoItem result set. </returns>
        IEnumerable<GeoItem> QueryBBox(double xmin, double ymin, double xmax, double ymax, string[] attributes);

        /// <summary> Return all which hit a point. </summary>
        /// <param name="x"> The minimum x value of the bounding box. </param>
        /// <param name="y"> The maximum x value of the bounding box. </param>
        /// <param name="attributes"> Additional attributes to be delivered. </param>
        /// <returns> The enumeration of the GeoItem result set. </returns>
        IEnumerable<GeoItem> QueryPoint(double x, double y, string[] attributes);
    }
}
