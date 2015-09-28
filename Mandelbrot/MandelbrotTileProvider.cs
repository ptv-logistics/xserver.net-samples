// from http://www.coldcity.com/index.php/fractals-in-c/

using System;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Ptv.XServer.Controls.Map.Layers.Tiled;

namespace Mandelbrot
{
    public class MandelbrotTileProvider : ITiledProvider
    {
        #region IMapTileServer Members

        public System.IO.Stream GetImageStream(int tx, int ty, int zoom)
        {
            int imageWidth = 256;
            int imageHeight = 256;
           
            double axMin = -1.95;
            double axMax = axMin + 2.5;
            double ayMin = -1.25;
            double ayMax = ayMin + 2.5;

            double part = 1.0 / (1 << zoom);
            double xMin = axMin + tx * part * (axMax - axMin);
            double xMax = axMin + (tx+1) * part * (axMax - axMin);
            double yMin = ayMin + ty * part * (ayMax - ayMin);
            double yMax = ayMin + (ty + 1) * part * (ayMax - ayMin);

            int iterations = 1000;

            HDRImage image = new HDRImage(imageWidth, imageHeight);

            double xInc = (xMax - xMin) / imageWidth;
            double yInc = (yMax - yMin) / imageHeight;
            int colsDone = 0;

            double x = xMin;        // Real part

            for (int screenX = 0; screenX < imageWidth; screenX++)
            {
                double y = yMin;    // Imaginary part

                for (int screenY = 0; screenY < imageHeight; screenY++)
                {
                    double x1 = 0, y1 = 0;

                    int iter = 0;
                    while (iter < iterations && x1 * x1 + y1 * y1 < 4) 
                    //OH-was: while (iter < iterations && Math.Sqrt((x1 * x1) + (y1 * y1)) < 2)
                    {
                        iter++;
                        double xx = (x1 * x1) - (y1 * y1) + x;
                        y1 = 2 * x1 * y1 + y;
                        x1 = xx;
                    }

                    image.SetPixel(screenX, screenY, iter);
                    y += yInc;
                }

                colsDone++;
                x += xInc;
            }

            Bitmap bmp = image.ToBitmap();
                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
           
        }

        public string CacheId
        {
            get { return "MandelbrotTileServer"; }
        }

        public int MinZoom
        {
            get { return 0; }
        }

        public int MaxZoom
        {
            get { return 30; }
        }

        #endregion
    }
}
