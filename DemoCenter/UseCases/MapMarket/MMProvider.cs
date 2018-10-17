// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;


namespace Ptv.XServer.Demo.MapMarket
{
    /// <summary> <para>Specifies if coordinate values are stored as integer or floating point.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public enum CoordinateType
    {
        /// <summary> Integer coordinates. </summary>
        Integer,
        /// <summary> Floating point coordinates. </summary>
        Float,
    }

    /// <summary> <para>A geo provider for the map&amp;market format.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public class MMProvider : IGeoProvider
    {
        #region public variables
        /// <summary> Gets or sets the OLE-DB connection. </summary>
        public OleDbConnection Connection { get; set; }

        /// <summary> Gets or sets the name of the table containing the data. </summary>
        public string Table { get; set; }

        /// <summary> Gets or sets the name of the field that contains the id. </summary>
        public string IdColumn { get; set; }

        /// <summary> Gets or sets the name of the field that contains the geometry WKB. </summary>
        public string GeometryColumn { get; set; }

        /// <summary> Gets or sets the name of the field that contains the minimum x coordinate. </summary>
        public string XMinColumn { get; set; }

        /// <summary> Gets or sets the name of the field that contains the minimum y coordinate. </summary>
        public string YMinColumn { get; set; }

        /// <summary> Gets or sets the name of the field that contains the maximum x coordinate. </summary>
        public string XMaxColumn { get; set; }

        /// <summary> Gets or sets the name of the field that contains the maximum y coordinate. </summary>
        public string YMaxColumn { get; set; }

        /// <summary> Gets or sets the the number type of the coordinates. </summary>
        public CoordinateType CoordinateType { get; set; }

        /// <summary> Gets or sets an optional filter condition. </summary>
        public string Filter { get; set; }
        #endregion

        #region protected methods

        /// <summary> Returns a string for a coordinate value. </summary>
        /// <param name="value"> The coordinate value. </param>
        /// <returns> The string for the coordinate. </returns>
        protected string GetOrdinateString(double value)
        {
            switch (CoordinateType)
            {
                default: return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                case CoordinateType.Float: return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }
        #endregion

        #region public methods

        /// <inheritdoc/>
        public IEnumerable<GeoItem> QueryBBox(double xmin, double ymin, double xmax, double ymax, string[] attributes)
        {
            var attributeFields = string.Empty;
            if (attributes != null && attributes.Length > 0)
            {
                attributeFields = ", ";

                for (var i = 0; i < attributes.Length; i++)
                {
                    attributeFields = attributeFields + attributes[i];
                    if (i < attributes.Length - 1)
                        attributeFields = attributeFields + ", ";
                }
            }

            var strSQL = "Select " +
                IdColumn + ", " + GeometryColumn + ", " +
                XMinColumn + ", " + YMinColumn + ", " + XMaxColumn + ", " + YMaxColumn + attributeFields +
                " FROM " + Table + " WHERE ";
            if (!String.IsNullOrEmpty(Filter))
                strSQL += "(" + Filter + ") AND ";
            //Limit to the points within the boundingbox
            strSQL +=
                XMinColumn + " < " + GetOrdinateString(xmax) + " AND " +
                XMaxColumn + " > " + GetOrdinateString(xmin) + " AND " +
                YMinColumn + " < " + GetOrdinateString(ymax) + " AND " +
                YMaxColumn + " > " + GetOrdinateString(ymin);

            using (var command = new OleDbCommand(strSQL, Connection))
            {
                using (var dr = command.ExecuteReader())
                {
                    while (dr != null && dr.Read())
                    {
                        var geoItem = new GeoItem
                        {
                            Id = dr[0],
                            Wkb = (byte[])dr[1],
                            XMin = Convert.ToDouble(dr[2]),
                            YMin = Convert.ToDouble(dr[3]),
                            XMax = Convert.ToDouble(dr[4]),
                            YMax = Convert.ToDouble(dr[5]),
                        };

                        if (attributes != null && attributes.Length > 0)
                        {
                            geoItem.Atributes = new Dictionary<string, object>();

                            for (var i = 0; i < attributes.Length; i++)
                            {
                                geoItem.Atributes[attributes[i]] = dr[6 + i];
                            }
                        }

                        yield return geoItem;
                    }
                }
            }
        }
        #endregion
    }
}
