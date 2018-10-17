// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;


namespace Ptv.XServer.Demo.GeoRSS
{
    /// <summary> <para>Show earthquakes on the map with pins. The size and color of the pins depends on the earthquake magnitude.</para>
    /// <para>See the <conceptualLink target="6b437145-f3ed-4267-a3fc-e71737e6db65"/> topic for an example.</para> </summary>
    public class GeoRssCanvas : WorldCanvas
    {
        #region private variables
        /// <summary> Transformation object for scaling the elements. </summary>
        private readonly ScaleTransform adjustTransform;
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="GeoRssCanvas"/> class. Reads the earthquakes from an
        /// xml file and shows them as pins on the map. </summary>
        /// <param name="mapView"> The map to be used in the GeoRssCanvas class. </param>
        public GeoRssCanvas(MapView mapView)
            : base(mapView)
        {
            // initialize tranformation for power-law scaling of symbols
            adjustTransform = new ScaleTransform();
            AdjustTransform();
 
            ParseAtomUsingLinq();
        }
        #endregion

        #region public methods
        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            AdjustTransform();
        }
        #endregion

        #region helper methods
        #region doc:GeoRssCanvas
        /// <summary> Reads the earthquakes from an xml file and adds a pin element for each to the map. </summary>
        private void ParseAtomUsingLinq()
        {
            var feedXML = System.Xml.Linq.XDocument.Load("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.atom");
            System.Xml.Linq.XNamespace xmlns = "http://www.w3.org/2005/Atom"; //Atom namespace
            System.Xml.Linq.XNamespace georssns = "http://www.georss.org/georss"; //GeoRSS Namespace

            // time to learn some LINQ
            var posts = (from item in feedXML.Descendants(xmlns + "entry")
                         select new
                                    {
                                        Title = item.Element(xmlns + "title")?.Value,
                                        Published = DateTime.Parse(item.Element(xmlns + "updated")?.Value),
                                        Url = item.Element(xmlns + "link")?.Attribute("href")?.Value,
                                        Description = item.Element(xmlns + "summary")?.Value,
                                        Location = CoordinateGeoRssPoint(item.Element(georssns + "point")),
                                        //Simple GeoRSS <georss:point>X Y</georss.point>
                                    }).ToList();

            var i = 0;
            // order posts by latitude, so they overlap nicely on the map
            foreach (var post in from post in posts orderby post.Location.Y descending select post)
            {
                if (double.IsNaN(post.Location.X) || double.IsNaN(post.Location.Y)) continue;

                // transform wgs to PTVMercator coordinate
                var mapPoint = GeoTransform.WGSToPtvMercator(post.Location);

                // create button and set pin template
                var pin = new Pin
                {
                    // a bug in SL throws an obscure exception if children share the same name        
                    // http://forums.silverlight.net/forums/t/134299.aspx
                    // the name is needed in XAML for data binding, so just create a unique name at runtime
                    Name = "pin" + (i++),
                    // set render transform for power-law scaling
                    RenderTransform = adjustTransform,
                    // scale around lower right
                    RenderTransformOrigin = new System.Windows.Point(1, 1)
                };

                // set size by magnitude
                double magnitude = MagnitudeFromTitle(post.Title);
                pin.Height = magnitude * 10;
                pin.Width = magnitude * 10;

                // calculate a value between 0 and 1 and use it for a blend color
                double relativeDanger = Math.Max(0, Math.Min(1, (magnitude - 2.5) / 4));
                pin.Color = Colors.Red;
                pin.Color = ColorBlend.Danger.GetColor((float)relativeDanger);

                // set tool tip information
                ToolTipService.SetToolTip(pin, post.Title);

                // set position and add to canvas (invert y-ordinate)
                // set lower right (pin-tip) as position
                SetLeft(pin, mapPoint.X - pin.Width);
                SetTop(pin, -(mapPoint.Y + pin.Height));
                Children.Add(pin);
            }
        }
        #endregion

        /// <summary> Adjust the transformation for logarithmic scaling. </summary>
        private void AdjustTransform()
        {
            // if the factor is 1.0 the elements have pixel size
            // if the factor is 0.0 the elements have Mercator size
            // for a factor between the elements are scaled with a logarithmic multiplicator
            const double logicalScaleFactor = .25;

            adjustTransform.ScaleX = 10 * Math.Pow(MapView.CurrentScale, 1.0 - logicalScaleFactor);
            adjustTransform.ScaleY = 10 * Math.Pow(MapView.CurrentScale, 1.0 - logicalScaleFactor);
        }

        /// <summary> Helper method to retrieve the earthquake magnitude from a string. </summary>
        /// <param name="title"> The title of the earthquake. </param>
        /// <returns> The magnitude of the earthquake. </returns>
        private static double MagnitudeFromTitle(string title)
        {
            try
            {
                const string pattern = @"^M (?<number>[0-9].[0-9]),*";

                var numberMatch = Regex.Match(title, pattern);
                string number = numberMatch.Groups["number"].Value;

                return Convert.ToDouble(number, NumberFormatInfo.InvariantInfo);
            }
            catch { return 0.1; }
        }

        /// <summary> Retrieves the coordinate of a GeoRSS point from a XML element. </summary>
        /// <param name="elm"> The XML element containing the coordinate. </param>
        /// <returns> A point containing the coordinate. </returns>
        private static System.Windows.Point CoordinateGeoRssPoint(System.Xml.Linq.XElement elm)
        {
            var emptyPoint = new System.Windows.Point(Double.NaN, double.NaN);

            if (elm == null) return emptyPoint;
            string val = elm.Value;
            string[] vals = val.Split(' ');
            if (vals.Length != 2) return emptyPoint;

            double x, y;
            return (double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                    double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out y)) ? new System.Windows.Point(x, y) : emptyPoint;
        }
        #endregion
    }
}
