// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows.Media;


namespace Ptv.XServer.Demo.GeoRSS
{
    /// <summary> Defines arrays of colors and positions used for interpolating color blending in a multicolor gradient. </summary>
    public class ColorBlend
    {
        #region private variables
        /// <summary> Array of colors along the gradient line. </summary>
        private Color[] _Colors;

        /// <summary> Array of positions along the gradient line. </summary>
        private float[] _Positions;
        #endregion

        #region public variables
        /// <summary> Gets or sets an array of colors that represents the colors to use at corresponding positions
        /// along a gradient. </summary>
        /// <value> An array of <see cref="Color"/> structures that represents the colors to use at corresponding
        /// positions along a gradient. </value>
        /// <remarks> This property is an array of <see cref="Color"/> structures that represents the colors to use at
        /// corresponding positions along a gradient. Along with the <see cref="Positions"/> property, this property
        /// defines a multicolor gradient. </remarks>
        public Color[] BlendColors
        {
            get { return _Colors; }
            set { _Colors = value; }
        }

        /// <summary> Gets or sets the positions along a gradient line. </summary>
        /// <value> An array of values that specify percentages of distance along the gradient line. </value>
        /// <remarks>
        /// <para> The elements of this array specify percentages of distance along the gradient line. For example, an
        /// element value of 0.2f specifies that this point is 20 percent of the total distance from the starting
        /// point. The elements in this array are represented by float values between 0.0f and 1.0f, and the first
        /// element of the array must be 0.0f and the last element must be 1.0f. </para>
        /// <pre> Along with the Colors property, this property defines a multicolor gradient. </pre>
        /// </remarks>
        public float[] Positions
        {
            get { return _Positions; }
            set { _Positions = value; }
        }

        /// <summary> Initializes a new instance of the ColorBlend class. </summary>
        /// <param name="colors"> An array of Color structures that represents the colors to use at corresponding positions along a gradient. </param>
        /// <param name="positions"> An array of values that specify percentages of distance along the gradient line. </param>
        public ColorBlend(Color[] colors, float[] positions)
        {
            _Colors = colors;
            _Positions = positions;
        }
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="ColorBlend"/> class. </summary>
        internal ColorBlend() { }
        #endregion

        #region Predefined color scales
        /// <summary> Gets a linear gradient scale with seven colors showing a rainbow from red to violet. </summary>
        /// <remarks> Colors span the following with an interval of 1/6: { Color.Red, Color.Orange, Color.Yellow,
        /// Color.Green, Color.Blue, Color.Indigo, Color.Violet }. </remarks>
        public static ColorBlend Rainbow7
        {
            get
            {
                var cb = new ColorBlend {_Positions = new float[7]};
                for (var i = 1; i < 7; i++)
                    cb.Positions[i] = i / 6f;
                cb.BlendColors = new[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Color.FromArgb(0xff, 0x4B, 0x00, 0x82), Color.FromArgb(0xff, 0xEE, 0x82, 0xEE) };
                return cb;
            }
        }

        /// <summary> Gets a linear gradient scale with four colors showing a danger categorization from green to red. </summary>
        /// <remarks> Colors span the following with an interval of 1/3: { Color.Green, Color.Yellow, Color.Orange,
        /// Color.Red }. </remarks>
        public static ColorBlend Danger
        {
            get
            {
                var cb = new ColorBlend {_Positions = new float[4]};
                for (var i = 1; i < 4; i++)
                    cb.Positions[i] = i / 3f;
                cb.BlendColors = new[] { Colors.Green, Colors.Yellow, Colors.Orange, Colors.Red };
                return cb;
            }
        }

        /// <summary> Gets a linear gradient scale with five colors showing a rainbow from red to blue. </summary>
        /// <remarks> Colors span the following with an interval of 0.25: { Color.Red, Color.Yellow, Color.Green,
        /// Color.Cyan, Color.Blue }. </remarks>
        public static ColorBlend Rainbow5 => new ColorBlend(
            new[] { Colors.Red, Colors.Yellow, Colors.Green, Colors.Cyan, Colors.Blue },
            new[] { 0f, 0.25f, 0.5f, 0.75f, 1f });

        /// <summary> Gets a linear gradient scale from black to white. </summary>
        public static ColorBlend BlackToWhite => new ColorBlend(new[] { Colors.Black, Colors.White }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from white to black. </summary>
        public static ColorBlend WhiteToBlack => new ColorBlend(new[] { Colors.White, Colors.Black }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from red to green. </summary>
        public static ColorBlend RedToGreen => new ColorBlend(new[] { Colors.Red, Colors.Green }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from green to red. </summary>
        public static ColorBlend GreenToRed => new ColorBlend(new[] { Colors.Green, Colors.Red }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from blue to green. </summary>
        public static ColorBlend BlueToGreen => new ColorBlend(new[] { Colors.Blue, Colors.Green }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from green to blue. </summary>
        public static ColorBlend GreenToBlue => new ColorBlend(new[] { Colors.Green, Colors.Blue }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from red to blue. </summary>
        public static ColorBlend RedToBlue => new ColorBlend(new[] { Colors.Red, Colors.Blue }, new[] { 0f, 1f });

        /// <summary> Gets a linear gradient scale from blue to red. </summary>
        public static ColorBlend BlueToRed => new ColorBlend(new[] { Colors.Blue, Colors.Red }, new[] { 0f, 1f });

        #endregion

        #region helper methods
        /// <summary> Gets the color from the scale at the given position. </summary>
        /// <remarks> If the position is outside the scale [0..1] only the fractional part is used (in other words the
        /// scale restarts for each integer-part). </remarks>
        /// <param name="pos"> Position on scale between 0.0f and 1.0f. </param>
        /// <returns> Color on scale. </returns>
        public Color GetColor(float pos)
        {
            if (_Colors.Length != _Positions.Length)
                throw (new ArgumentException("Colors and Positions arrays must be of equal length"));
            if (_Colors.Length < 2)
                throw (new ArgumentException("At least two colors must be defined in the ColorBlend"));
            if (Math.Abs(_Positions[0]) > 0.00001)
                throw (new ArgumentException("First position value must be 0.0f"));
            if (Math.Abs(_Positions[_Positions.Length - 1] - 1f) > 0.00001)
                throw (new ArgumentException("Last position value must be 1.0f"));
            
            if (pos > 1 || pos < 0) 
                pos -= (float)Math.Floor(pos);
            var i = 1;
            while (i < _Positions.Length && _Positions[i] < pos)
                i++;
            float frac = (pos - _Positions[i - 1]) / (_Positions[i] - _Positions[i - 1]);
            var R = (byte)Math.Round((_Colors[i - 1].R * (1 - frac) + _Colors[i].R * frac));
            var G = (byte)Math.Round((_Colors[i - 1].G * (1 - frac) + _Colors[i].G * frac));
            var B = (byte)Math.Round((_Colors[i - 1].B * (1 - frac) + _Colors[i].B * frac));
            var A = (byte)Math.Round((_Colors[i - 1].A * (1 - frac) + _Colors[i].A * frac));
            return Color.FromArgb(A, R, G, B);
        }

        /// <summary> Creates a linear gradient scale from two colors. </summary>
        /// <param name="fromColor"> The beginning color of the gradient. </param>
        /// <param name="toColor"> The destination color of the gradient. </param>
        /// <returns> The new linear color gradient. </returns>
        public static ColorBlend TwoColors(Color fromColor, Color toColor)
        {
            return new ColorBlend(new[] { fromColor, toColor }, new[] { 0f, 1f });
        }

        /// <summary> Creates a linear gradient scale from three colors. </summary>
        /// <param name="fromColor"> The beginning color of the gradient. </param>
        /// <param name="middleColor"> The middle color of the gradient. </param>
        /// <param name="toColor"> The destination color of the gradient. </param>
        /// <returns> The new linear color gradient. </returns>
        public static ColorBlend ThreeColors(Color fromColor, Color middleColor, Color toColor)
        {
            return new ColorBlend(new[] { fromColor, middleColor, toColor }, new[] { 0f, 0.5f, 1f });
        }
        #endregion
    }
}
