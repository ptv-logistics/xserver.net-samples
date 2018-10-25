//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Layers.Untiled;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public struct LatLon
    {
        /// <summary>  </summary>
        public double Latitude { get; set; }
        /// <summary>  </summary>
        public double Longitude { get; set; }
    }

    /// <summary>  </summary>
    public class MyOverlayRenderer : IUntiledProvider
    {
        #region public variables
        /// <summary> list of locations </summary>
        public List<LatLon> Locations { get; set; }

        /// <summary> size of the symbol </summary>
        public int SymbolSize = 10;

        /// <summary> color of the symbol </summary>
        public System.Windows.Media.Color SymbolColor = System.Windows.Media.Colors.Blue;
        #endregion

        #region public methods
        /// <summary>  </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<LatLon> CreateTestLocations(int count)
        {
            var result = new List<LatLon>();

            const double centerLat = 49;
            const double centerLon = 8.4;
            var r = new Random();

            for (int i = 0; i < count; i++)
            {
                result.Add(new LatLon
                {
                    Latitude = centerLat + r.NextDouble() * 10 - 5,
                    Longitude = centerLon + r.NextDouble() * 10 - 5
                });
            }

            return result;
        }
        #endregion

        #region IMapOverlayServer Members
        /// <summary>  </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Stream GetImageStream(double left, double top, double right, double bottom, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            int symbolRadius = SymbolSize / 2;

            foreach (var location in Locations)
            {              
                System.Windows.Point mercatorPoint = GeoTransform.WGSToPtvMercator(
                    new System.Windows.Point(location.Longitude, location.Latitude));

                var pixelPoint = new Point(
                    (int)((mercatorPoint.X - left) / (right - left) * width),
                    (int)((mercatorPoint.Y - bottom) / (top - bottom) * height));

                if (pixelPoint.X < -symbolRadius || pixelPoint.X > width + symbolRadius
                    || pixelPoint.Y < -symbolRadius || pixelPoint.Y > height + symbolRadius)
                    continue;

                graphics.FillEllipse(new SolidBrush(Color.FromArgb(SymbolColor.A, SymbolColor.R, SymbolColor.G, SymbolColor.B)),
                    pixelPoint.X - symbolRadius, pixelPoint.Y - symbolRadius, SymbolSize, SymbolSize);
            }

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin); // goto stream begin

            return memoryStream;
        }
        #endregion
    }
}
