//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class Location
    {
        /// <summary> Latitude of the center. </summary>
        public double Latitude { get; set; }

        /// <summary> Longitude of the center. </summary>
        public double Longitude { get; set; }

        /// <summary> Radius of the location. </summary>
        public double Radius { get; set; }

        /// <summary> Descriptive text. </summary>
        public string Description { get; set; }
    }

    ///// <summary>  </summary>
    //public class MyPainter : CanvasPainter
    //{
    //    #region private variables
    //    /// <summary>  </summary>
    //    private List<Location> locations;
    //    #endregion

    //    #region constructor
    //    /// <summary> Constructor of the MyPainter class. </summary>
    //    /// <param name="locations"> Locations to be displayed. </param>
    //    public MyPainter(List<Location> locations)
    //    {
    //        this.locations = locations;
    //    }
    //    #endregion

    //    #region event handling
    //    /// <summary> Event handler for entering with the mouse in an ellipse. </summary>
    //    /// <param name="sender"> Sender of the MouseEnter event. </param>
    //    /// <param name="e"> Event parameters. </param>
    //    private void ellipse_MouseEnter(object sender, MouseEventArgs e)
    //    {
    //        Ellipse ellipse = (Ellipse)sender;

    //        if (ellipse.FindName("Scale") != null)
    //            ellipse.UnregisterName("Scale");

    //        double scale = 2;
    //        var sc = new ScaleTransform { CenterX = ellipse.Width / 2, CenterY = ellipse.Height / 2 };
    //        ellipse.RenderTransform = sc;
    //        var sb = new Storyboard();
    //        var animation1 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(250), To = scale, AutoReverse = true };
    //        var animation2 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(250), To = scale, AutoReverse = true };
    //        sb.Children.Add(animation1);
    //        sb.Children.Add(animation2);
    //        ellipse.RegisterName("Scale", sc);
    //        Storyboard.SetTargetName(animation1, "Scale");
    //        Storyboard.SetTargetName(animation2, "Scale");
    //        Storyboard.SetTargetProperty(animation1, new PropertyPath(ScaleTransform.ScaleXProperty));
    //        Storyboard.SetTargetProperty(animation2, new PropertyPath(ScaleTransform.ScaleYProperty));
    //        sb.Begin(ellipse);
    //    }
    //    #endregion

    //    #region public methods
    //    /// <summary>  </summary>
    //    public override CanvasCategory[] Categories
    //    {
    //        get { return new CanvasCategory[] { CanvasCategory.Content, CanvasCategory.ContentLabels }; }
    //    }

    //    /// <summary>  </summary>
    //    /// <param name="canvasNumber"></param>
    //    /// <param name="canvas"></param>
    //    /// <param name="updateMode"></param>
    //    public override void UpdateCanvas(int canvasNumber, MapCanvas canvas, UpdateMode updateMode)
    //    {
    //        switch (canvasNumber)
    //        {
    //            case 0:
    //                {
    //                    if (updateMode == UpdateMode.Refresh)
    //                    {
    //                        canvas.Children.Clear();

    //                        foreach (var location in locations)
    //                        {                   
    //                            // calculate ptv location in canvas units
    //                            Point canvasPoint = canvas.GeoToCanvas(new Point(location.Longitude, location.Latitude));

    //                            // we want to display a circle with a radius of 250 meters around the ptv location
    //                            // calculate the corrected distance which takes the mercator projection into account
    //                            double radius = location.Radius; // radius in meters
    //                            double cosB = Math.Cos((location.Latitude / 360.0) * (2 * Math.PI)); // factor depends on latitude
    //                            double ellipseSize = Math.Abs(1.0 / cosB * radius) * 2; // size mercator units

    //                            // Create the ellipse and insert it to our canvas
    //                            Ellipse ellipse = new Ellipse
    //                            {
    //                                Width = ellipseSize,
    //                                Height = ellipseSize,
    //                                Fill = new SolidColorBrush(Color.FromArgb(192, 0, 0, 255)),
    //                                Stroke = new SolidColorBrush(Colors.Black),
    //                                StrokeThickness = 20
    //                            };

    //                            // set position and add to map
    //                            Canvas.SetLeft(ellipse, canvasPoint.X - ellipseSize / 2);
    //                            Canvas.SetTop(ellipse, canvasPoint.Y - ellipseSize / 2);

    //                            // add ellipse to canvas
    //                            canvas.Children.Add(ellipse);

    //                            // set tool tip
    //                            ToolTipService.SetToolTip(ellipse, location.Description);

    //                            // gradient fill
    //                            var g = new GradientStopCollection
    //                            {
    //                                new GradientStop {Color = Color.FromArgb(192, 0, 0, 255), Offset = 0},
    //                                new GradientStop {Color = Color.FromArgb(192, 255, 255, 255), Offset = 1}
    //                            };
    //                            ellipse.Fill = new LinearGradientBrush(g, 45);

    //                            // mouse-over effect
    //                            ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);

    //                            // dropshadow
    //                            ellipse.Effect = new System.Windows.Media.Effects.DropShadowEffect
    //                            {
    //                                Color = Colors.Black,
    //                                ShadowDepth = 15,
    //                                Opacity = .8,
    //                                BlurRadius = 3,
    //                                Direction = 45,
    //                            };
    //                        }
    //                    }
    //                }
    //                break;
    //            case 1:
    //                {
    //                    if (updateMode == UpdateMode.Refresh)
    //                    {
    //                        SetText(canvas);
    //                    }
    //                    else if (updateMode == UpdateMode.WhileTransition)
    //                    {
    //                        UpdateTextScales(canvas);
    //                    }
    //                }
    //                break;
    //        }
    //    }

    //    /// <summary>  </summary>
    //    /// <param name="canvas"></param>
    //    public void SetText(MapCanvas canvas)
    //    {
    //        foreach (var location in locations)
    //        {
    //            // calculate ptv location in ptv canvas units
    //            Point canvasPoint = canvas.GeoToCanvas(new Point(location.Longitude, location.Latitude));

    //            TextBox textBox = new TextBox();
    //            textBox.Text = "PTV Group";
    //            Canvas.SetLeft(textBox, canvasPoint.X);
    //            Canvas.SetTop(textBox, canvasPoint.Y);
    //            canvas.Children.Add(textBox);
    //            textBox.RenderTransform = new ScaleTransform(canvas.Map.CurrentScale, canvas.Map.CurrentScale);
    //        }
    //    }

    //    /// <summary>  </summary>
    //    /// <param name="canvas"></param>
    //    public void UpdateTextScales(MapCanvas canvas)
    //    {
    //        foreach (TextBox textBox in canvas.Children)
    //            textBox.RenderTransform = new ScaleTransform(canvas.Map.CurrentScale, canvas.Map.CurrentScale);
    //    }
    //    #endregion
    //}
}
