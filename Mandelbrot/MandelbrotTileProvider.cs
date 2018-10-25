// from http://www.coldcity.com/index.php/fractals-in-c/

using System.IO;
using System.Drawing.Imaging;
using Ptv.XServer.Controls.Map.Layers.Tiled;

namespace Mandelbrot
{
    public class MandelbrotTileProvider : ITiledProvider
    {
        #region IMapTileServer Members

        public Stream GetImageStream(int tx, int ty, int zoom)
        {
            const int imageWidth = 256;
            const int imageHeight = 256;
           
            const double axMin = -1.95;
            const double axMax = axMin + 2.5;
            const double ayMin = -1.25;
            const double ayMax = ayMin + 2.5;

            double part = 1.0 / (1 << zoom);
            double xMin = axMin + tx * part * (axMax - axMin);
            double xMax = axMin + (tx+1) * part * (axMax - axMin);
            double yMin = ayMin + ty * part * (ayMax - ayMin);
            double yMax = ayMin + (ty + 1) * part * (ayMax - ayMin);

            const int iterations = 1000;

            var hdrImage = new HDRImage(imageWidth, imageHeight);

            double xInc = (xMax - xMin) / imageWidth;
            double yInc = (yMax - yMin) / imageHeight;

            double x = xMin;        // Real part

            for (int screenX = 0; screenX < imageWidth; screenX++)
            {
                double y = yMin;    // Imaginary part

                for (int screenY = 0; screenY < imageHeight; screenY++)
                {
                    double x1 = 0, y1 = 0;

                    int iter = 0;
                    while (iter < iterations && x1 * x1 + y1 * y1 < 4) 
                    //OH-was: while (iter < iterations && Math.Sqrt(x1 * x1 + y1 * y1) < 2)
                    {
                        iter++;
                        double xx = x1 * x1 - y1 * y1 + x;
                        y1 = 2 * x1 * y1 + y;
                        x1 = xx;
                    }

                    hdrImage.SetPixel(screenX, screenY, iter);
                    y += yInc;
                }

                x += xInc;
            }

            var bitmap = hdrImage.ToBitmap();
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
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
