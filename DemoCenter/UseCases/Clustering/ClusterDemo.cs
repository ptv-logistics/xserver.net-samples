// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;


namespace Ptv.XServer.Demo.Clustering
{
    /// <summary> Stores a post of a web forum. This is used to store the earthquake messages. </summary>
    public class Post
    {
        /// <summary> Gets or sets the title of the post. </summary>
        public String Title {get;set;}

        /// <summary> Gets or sets the date when the post has been published. </summary>
        public DateTime Published { get; set; }

        /// <summary> Gets or sets the url of the post. </summary>
        public String Url { get; set; }

        /// <summary> Gets or sets the description of the post. </summary>
        public String Description { get; set; }

        /// <summary> Gets or sets the location of the post. </summary>
        public Point Location { get; set; }

        /// <summary> Gets or sets the transformed location of the post in Mercator coordinates. </summary>
        public Point TransformedLocation { get; set; }
    }

    /// <summary> <para>Show clustered earthquakes on the map with balls. The size of the balls depends on the cluster size.</para>
    /// <para>See the <conceptualLink target="4926f311-8333-4b18-b509-70c1d876d5eb"/> topic for an example.</para> </summary>
    public class LocalizedWikiDemo : WorldCanvas
    {
        #region private variables
        /// <summary> Clusterer for managing the earthquake posts. </summary>
        private readonly TileBasedPointClusterer<Post> clusterer;
        
        /// <summary> Transformation object for scaling the elements. </summary>
        private readonly ScaleTransform adjustTransform;

        /// <summary> Logical scale factor to define whether the objects should be displayed in a fix size or in
        /// adapted size when zooming in and out the map. If the factor is 1.0, the elements have pixel size. If the
        /// factor is 0.0, the elements have Mercator size. If the factor lies between 0.0 and 1.0, the elements are
        /// scaled with a logarithmic multiplier. </summary>
        private readonly double logicalScaleFactor;
        #endregion

        #region doc:constructor
        /// <summary> Initializes a new instance of the <see cref="LocalizedWikiDemo"/> class. Reads in the earthquakes
        /// from a file and displays them on the map. </summary>
        /// <param name="mapView"> The map to be used for display. </param>
        /// <param name="clusterer"> The clusterer containing the clusters. </param>
        public LocalizedWikiDemo(MapView mapView, TileBasedPointClusterer<Post> clusterer)
            : base(mapView)
        {
            adjustTransform = new ScaleTransform();
            this.clusterer = clusterer;
            
            logicalScaleFactor = 0.65;
            UpdateBalls();
        }

        #endregion // doc:constructor

        #region public methods

        /// <summary> Adjust the transformation for logarithmic scaling. </summary>
        public void AdjustTransform()
        {
            adjustTransform.ScaleX = 10 * Math.Pow(MapView.CurrentScale, logicalScaleFactor);
            adjustTransform.ScaleY = 10 * Math.Pow(MapView.CurrentScale, logicalScaleFactor);
        }

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            // update the balls when ending the transition
            if (updateMode == UpdateMode.EndTransition)
                UpdateBalls();

            // always update the scale
            AdjustTransform();
        }
        #endregion

        #region helper methods

        #region doc:map_Ball_ViewportChanged handler
        /// <summary> Event handler for a change of the map viewport. Renders a ball for each of the earthquake
        /// clusters in the current map display section. </summary>
        private void UpdateBalls()
        {
            Children.Clear();

            MapRectangle rect = MapView.FinalEnvelope;
            var clusters = clusterer.GetClusters(rect.West, rect.South, rect.East, rect.North, (int)MapView.FinalZoom + 1);
            foreach (var cluster in clusters)
            {
                var toolTip = "";
                var point = new Point(cluster.CentroidX, cluster.CentroidY);

                if (cluster.Tags.Count <= 25)
                {
                    toolTip = cluster.Tags.Aggregate(toolTip, (current, post) => current + (post.Title + " " + post.Location.Y + " / " + post.Location.X + "\n"));
                    toolTip = toolTip.Substring(0, toolTip.Length - 1);
                }
                else
                {
                    toolTip = cluster.Tags.Count + " objects.";
                }

                renderBall(toolTip, point, cluster.Tags.Count);
            }
        }
        #endregion // doc:map_Ball_ViewportChanged handler

        #region doc:renderBall method

        /// <summary> Renders a ball for a certain earthquake cluster. </summary>
        /// <param name="toolTip"> Tooltip to be shown for the cluster. </param>
        /// <param name="location"> Location of the cluster. </param>
        /// <param name="clusterSize"> Cluster size e.g. number of objects contained in the cluster. </param>
        private void renderBall(string toolTip, Point location, int clusterSize)
        {
            // create button and set pin template
            var ball = new Ball
            {
                // scale around center
                RenderTransformOrigin = new Point(.5, .5),

                // set render transform for power-law scaling
                RenderTransform = adjustTransform,
            };

            // calculate a value between 0 and 1 and use it for a blend color
            double log10 = Math.Log10(clusterSize);
            //double relativeDanger = Math.Max(0, Math.Min(1, (magnitude - 2.5) / 4));
            double relativeDanger = Math.Max(0, Math.Min(1, (log10) / 4));//Math.Max(0, Math.Min(1, ((double)clusterSize) / 50));
            ball.Color = Colors.Red;
            ball.Color = GeoRSS.ColorBlend.Danger.GetColor((float)relativeDanger);

            // set size by magnitude
            ball.Height = Math.Max(3, relativeDanger * 50);//System.Math.Min(System.Math.Max(clusterSize + 3, 5), 50);
            ball.Width = ball.Height;//System.Math.Min(System.Math.Max(clusterSize + 3, 5), 50);

            // set tool tip information
            ToolTipService.SetToolTip(ball, toolTip);

            // set position and add to canvas (invert y-ordinate)
            // set lower right (pin-tip) as position
            SetLeft(ball, location.X - ball.Width / 2);
            SetTop(ball, -(location.Y + ball.Height / 2));
            Children.Add(ball);
        }
        #endregion // doc:renderBall method

        #region doc:ReadCSVFile method
        /// <summary> Reads the earthquakes from a csv file and adds them to a clusterer. The clusterer then groups
        /// them to clusters. </summary>
        /// <returns> Documentation in progress... </returns>
        public static TileBasedPointClusterer<Post> ReadCSVFile()
        {
            var clusterer = new TileBasedPointClusterer<Post>(MapView.LogicalSize, 0, 19);
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\ClusterData") + @"\wikilocations.csv";
            using (var reader = new CsvFileReader(filePath, (char) 0x09))
            {
                var row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    if (row.Count < 3)
                        continue;

                    double x, y;
                    bool parsed = Double.TryParse(row[2], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    x = parsed ? x : Double.NaN;
                    parsed = Double.TryParse(row[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    y = parsed ? y : Double.NaN;
                    var post = new Post {Title = row[0], Location = new Point(x, y)};
                    post.TransformedLocation = GeoTransform.WGSToPtvMercator(post.Location);
                    clusterer.AddPoint(post.TransformedLocation.X, post.TransformedLocation.Y, 1, post);
                }
            }

            #region doc:clusterer.Cluster method call
            clusterer.Cluster();
            return clusterer;
            #endregion //doc:clusterer.Cluster method call
        }
        #endregion //doc:ReadCSVFile method
        #endregion

    }
}
