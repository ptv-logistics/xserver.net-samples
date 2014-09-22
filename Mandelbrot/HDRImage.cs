// from http://www.coldcity.com/index.php/fractals-in-c/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;

namespace Mandelbrot
{
    public class HDRImage
    {
        int m_width, m_height;							        // Width and height of image
        double[] buffer;										// The image buffer

        public HDRImage(int width, int height)
        {
            m_width = width;
            m_height = height;
            buffer = new double[width * height];
            for (int i = 0; i < m_width * m_height; i++)
                buffer[i] = 0;
        }

        public double GetPixel(int x, int y)
        {
            return buffer[y * m_width + x];
        }

        public void SetPixel(int x, int y, double level)
        {
            buffer[y * m_width + x] = level;
        }

        public Bitmap ToBitmap()
        {
            Bitmap image = new Bitmap(m_width, m_height);

            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    double val = GetPixel(x, y);
                    image.SetPixel(x, y, getColorFromWaveLength((int)val + 350));
                }
            }

            return image;
        }

        private Color getColorFromWaveLength(int Wavelength)
        {
            double Gamma = 1.00;
            int IntensityMax = 255;

            double Blue;
            double Green;
            double Red;
            double Factor;

            if (Wavelength >= 350 && Wavelength <= 439)
            {
                Red = -(Wavelength - 440d) / (440d - 350d);
                Green = 0.0;
                Blue = 1.0;
            }
            else if (Wavelength >= 440 && Wavelength <= 489)
            {
                Red = 0.0;
                Green = (Wavelength - 440d) / (490d - 440d);
                Blue = 1.0;
            }
            else if (Wavelength >= 490 && Wavelength <= 509)
            {
                Red = 0.0;
                Green = 1.0;
                Blue = -(Wavelength - 510d) / (510d - 490d);
            }
            else if (Wavelength >= 510 && Wavelength <= 579)
            {
                Red = (Wavelength - 510d) / (580d - 510d);
                Green = 1.0;
                Blue = 0.0;
            }
            else if (Wavelength >= 580 && Wavelength <= 644)
            {
                Red = 1.0;
                Green = -(Wavelength - 645d) / (645d - 580d);
                Blue = 0.0;
            }
            else if (Wavelength >= 645 && Wavelength <= 780)
            {
                Red = 1.0;
                Green = 0.0;
                Blue = 0.0;
            }
            else
            {
                Red = 0.0;
                Green = 0.0;
                Blue = 0.0;
            }

            if (Wavelength >= 350 && Wavelength <= 419)
            {
                Factor = 0.3 + 0.7 * (Wavelength - 350d) / (420d - 350d);
            }
            else if (Wavelength >= 420 && Wavelength <= 700)
            {
                Factor = 1.0;
            }
            else if (Wavelength >= 701 && Wavelength <= 780)
            {
                Factor = 0.3 + 0.7 * (780d - Wavelength) / (780d - 700d);
            }
            else
            {
                Factor = 0.0;
            }

            int R = this.factorAdjust(Red, Factor, IntensityMax, Gamma);
            int G = this.factorAdjust(Green, Factor, IntensityMax, Gamma);
            int B = this.factorAdjust(Blue, Factor, IntensityMax, Gamma); return Color.FromArgb(R, G, B);
        }

        private int factorAdjust(double Color, double Factor, int IntensityMax, double Gamma)
        {
            if (Color == 0.0)
            {
                return 0;
            }
            else
            {
                return (int)Math.Round(IntensityMax * Math.Pow(Color * Factor, Gamma));
            }
        }
    }
}