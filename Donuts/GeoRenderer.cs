//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Tools;


namespace Ptv.XServer.Demo.MapMarket
{
    /// <summary> <para>A style for rendering GDI shapes.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public class GdiStyle
    {
        /// <summary> Gets or sets the filling of a polygon. </summary>
        public Brush Fill { get; set; }

        /// <summary> Gets or sets the outline of a polygon / line-style for a line string. </summary>
        public Pen Outline { get; set; }
    }

    /// <summary> <para>A dynamic style (= theme) depending on attribute data.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public class GdiTheme
    {
        /// <summary> Gets or sets the fields required for thematic mapping. </summary>
        public string[] RequiredFields { get; set; }

        /// <summary> Gets or sets the dynamic style. </summary>
        public Func<GeoItem, GdiStyle> Mapping { get; set; }
    }

    /// <summary> <para>A renderer which renders tiles given an <see cref="IGeoProvider" /> and a <see cref="GdiTheme" />.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public class TileRenderer : ITiledProvider
    {
        #region public variables
        /// <summary> Gets or sets the provider. </summary>
        public IGeoProvider Provider { get; set; }

        /// <summary> Gets or sets the theme. </summary>
        public GdiTheme Theme { get; set; }
        #endregion

        #region public methods
        /// <summary> Creates a stream object containing the image at position (<paramref name="x"/>,<paramref name="y"/>) 
        /// and zoom level <paramref name="zoom"/>. </summary>
        /// <param name="x"> X-coordinate for the requested tile. </param>
        /// <param name="y"> Y-coordinate for the requested tile. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> The stream object containing the image of the tile. </returns>
        public Stream GetImageStream(int x, int y, int zoom)
        {
            // Create a bitmap of size 256x256
            using (var bmp = new Bitmap(256, 256))
            {
                // calc rect from tile key
                var rect = GeoTransform.TileToPtvMercatorAtZoom(x, y, zoom);
                var wgsRect = new System.Windows.Rect(GeoTransform.PtvMercatorToWGS(rect.TopLeft), 
                    GeoTransform.PtvMercatorToWGS(rect.BottomRight));

                // PTV_Mercator to Image function
                Func <double, double, Point> wgsToImage =
                    (lon, lat) =>
                    {
                        var mercatorP = GeoTransform.WGSToPtvMercator(new System.Windows.Point(lon, lat));
                        var p = new Point(
          x = (int)((mercatorP.X - rect.Left) / (rect.Right - rect.Left) * 256),
          y = 256 + (int)-((mercatorP.Y - rect.Top) / (rect.Bottom - rect.Top) * 256));
                        return p;
                    };

                // get graphics from bitmap
                using (var graphics = Graphics.FromImage(bmp))
                {
                    // query the provider for the items in the envelope
                    var result = Provider.QueryBBox(wgsRect.Left, wgsRect.Top, wgsRect.Right, wgsRect.Bottom, Theme.RequiredFields);

                    foreach (var item in result)
                    {
                        // create GDI path from wkb
                        var path = WkbToGdi.Parse(item.Wkb, wgsToImage);

                        // evaluate style
                        var style = Theme.Mapping(item);

                        // fill polygon
                        if (style.Fill != null)
                            graphics.FillPath(style.Fill, path);

                        // draw outline
                        if (style.Outline != null)
                            graphics.DrawPath(style.Outline, path);
                    }
                }

                // crate a memory stream
                var stream = new MemoryStream();

                // save image to stream
                bmp.Save(stream, ImageFormat.Png);

                // rewind stream
                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
        }

        /// <summary> Gets the unique name for the tile cache. </summary>
        public string CacheId { get; set; }

        /// <summary> Gets the minimum level for the layer. </summary>
        public int MinZoom
        {
            get { return 0; }
        }

        /// <summary> Gets the maximum level for the layer. </summary>
        public int MaxZoom
        {
            get { return 19; }
        }
        #endregion
    }
}
