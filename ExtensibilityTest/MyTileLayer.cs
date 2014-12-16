//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Drawing.Imaging;
using System.IO;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Layers.Tiled;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> a tile provider which renders tile borders </summary>
    class MyTileRenderer : ITiledProvider
    {
        #region public methods
        /// <summary>  </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public System.IO.Stream GetImageStream(int x, int y, int zoom)
        {
            // Create a bitmap of size 256x256
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(256, 256))
            {
                // get graphics from bitmap
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    // draw a rectangle
                    graphics.DrawRectangle(System.Drawing.Pens.Black, 0, 0, 255, 255);
                }

                // crate a memory stream
                MemoryStream stream = new MemoryStream();

                // save image to stream
                bmp.Save(stream, ImageFormat.Png);

                // rewind stream
                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
        }

        /// <summary> the unique name for the WpfMap tile cache </summary>
        public string CacheId
        {            
            get { return "GridTileServer"; }
        }

        /// <summary> the minimum level for the layer </summary>
        public int MinZoom
        {
            get { return 0; }
        }

        /// <summary> the maximum level for the layer </summary>
        public int MaxZoom
        {
            get { return 19; }
        }
        #endregion
    }
}
