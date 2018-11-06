//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Drawing;
using System.Drawing.Imaging;
using System;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using System.IO;

namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> a tile provider which renders tile borders </summary>
    internal class MyTileRenderer : ITiledProvider
    {
        #region public methods

        /// <summary>  </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public Stream GetImageStream(int x, int y, int z)
        {
            // Create a bitmap of size 256x256
            using (var bmp = new Bitmap(256, 256))
                // get graphics from bitmap
            using (var graphics = Graphics.FromImage(bmp))
            {
                // draw background
                //var brush = new LinearGradientBrush(new Point(0, 0), new Point(256, 256),
                //    Color.LightBlue, Color.Transparent);
                //graphics.FillRectangle(brush, new Rectangle(0, 0, 256, 256));
                //brush.Dispose();

                var rect = TransformTools.TileToWgs(x, y, z);
                int left = (int) Math.Floor(rect.Left);
                int right = (int) Math.Floor(rect.Right);
                int top = (int) Math.Floor(rect.Top);
                int bottom = (int) Math.Floor(rect.Bottom);

                for (int lon = left; lon <= right; lon++)
                {
                    for (int lat = top; lat <= bottom; lat++)
                    {
                        var g1 = new System.Windows.Point(lon, lat);
                        var g2 = new System.Windows.Point(lon + 1, lat + 1);
                        var p1 = TransformTools.WgsToTile(x, y, z, g1);
                        var p2 = TransformTools.WgsToTile(x, y, z, g2);

                        graphics.DrawLine(Pens.Black, new System.Drawing.Point((int) p1.X, (int) p1.Y),
                            new System.Drawing.Point((int) p2.X, (int) p1.Y));
                        graphics.DrawLine(Pens.Black, new System.Drawing.Point((int) p1.X, (int) p1.Y),
                            new System.Drawing.Point((int) p1.X, (int) p2.Y));
                    }
                }


                // crate a memory stream
                var memoryStream = new MemoryStream();

                // save image to stream
                bmp.Save(memoryStream, ImageFormat.Png);

                // rewind stream
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
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

    /// <summary>
    /// A tools class that does the arithmetics for tiled maps
    /// </summary>
    public static class TransformTools
    {
        /// <summary>
        /// Convert a WGS84 coordinate (Lon/Lat) to generic spherical mercator.
        /// When using tiles with wgs, the actual earth radius doesn't matter, we can just use radius 1.
        /// To use this formula with "Google Mercator", you have to multiply the output coordinates by 6378137.
        /// For "PTV Mercator" use 6371000
        /// </summary>
        public static System.Windows.Point WgsToSphereMercator(System.Windows.Point point)
        {
            double x = point.X * Math.PI / 180.0;
            double y = Math.Log(Math.Tan(Math.PI / 4.0 + point.Y * Math.PI / 360.0));

            return new System.Windows.Point(x, y);
        }

        /// <summary>
        /// The reverse of the function above
        /// To use this formula with "Google Mercator", you have to divide the input coordinates by 6378137.
        /// For "PTV Mercator" use 6371000
        /// </summary>
        public static System.Windows.Point SphereMercatorToWgs(System.Windows.Point point)
        {
            double x = (180 / Math.PI) * point.X;
            double y = (360 / Math.PI) * (Math.Atan(Math.Exp(point.Y)) - (Math.PI / 4));

            return new System.Windows.Point(x, y);
        }

        /// <summary>
        /// Calculate the Mercator bounds for a tile key
        /// </summary>
        public static System.Windows.Rect TileToSphereMercator(int x, int y, int z)
        {
            // the width of a tile (when the earth has radius 1)
            double arc = Math.PI * 2.0 / Math.Pow(2, z);

            double x1 = -Math.PI + x * arc;
            double x2 = x1 + arc;

            double y1 = Math.PI - y * arc;
            double y2 = y1 - arc;

            return new System.Windows.Rect(new System.Windows.Point(x1, y2), new System.Windows.Point(x2, y1));
        }

        /// <summary>
        /// Calculate WGS (Lon/Lat) bounds for a tile key
        /// </summary>
        public static System.Windows.Rect TileToWgs(int x, int y, int z, int bleedingPixels = 0)
        {
            var rect = TileToSphereMercator(x, y, z);

            if (bleedingPixels != 0)
            {
                double bleedingFactor = bleedingPixels / 256.0 * 2;

                rect.Inflate(rect.Width * bleedingFactor, rect.Height * bleedingFactor);
            }

            return new System.Windows.Rect(SphereMercatorToWgs(rect.TopLeft), SphereMercatorToWgs(rect.BottomRight));
        }

        /// <summary>
        /// Convert a point relative to a mercator viewport to a point relative to an image
        /// </summary>
        public static System.Drawing.Point MercatorToImage(System.Windows.Rect mercatorRect,
            System.Windows.Size imageSize, System.Windows.Point mercatorPoint)
        {
            return new System.Drawing.Point(
                (int) ((mercatorPoint.X - mercatorRect.Left) / (mercatorRect.Right - mercatorRect.Left) *
                       imageSize.Width),
                (int) (imageSize.Height - (mercatorPoint.Y - mercatorRect.Top) /
                       (mercatorRect.Bottom - mercatorRect.Top) * imageSize.Height));
        }

        /// <summary>
        /// Convert a WGS (Lon,Lat) coordinate to a point relative to a tile image
        /// </summary>
        public static System.Drawing.Point WgsToTile(int x, int y, int z, System.Windows.Point wgsPoint,
            double clipWgsAtDegrees = 85.05)
        {
            if (clipWgsAtDegrees < 90)
                wgsPoint = ClipWgsPoint(wgsPoint, clipWgsAtDegrees);

            return MercatorToImage(TileToSphereMercator(x, y, z), new System.Windows.Size(256, 256),
                WgsToSphereMercator(wgsPoint));
        }

        /// <summary>
        /// Clip the latitude value to avoid overflow at the poles
        /// </summary>
        public static System.Windows.Point ClipWgsPoint(System.Windows.Point p, double degrees = 85.05)
        {
            if (p.Y > degrees)
                p.Y = degrees;
            if (p.Y < -degrees)
                p.Y = -degrees;

            return p;
        }
    }
}
