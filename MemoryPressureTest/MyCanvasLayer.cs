//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Canvases;


namespace MemoryPressureTest
{
    /// <summary>  </summary>
    public class MyCanvasLayer : WorldCanvas
    {
        #region private variables
        /// <summary>  </summary>
        private ScaleTransform adjustTransform = new ScaleTransform();
        #endregion

        #region public variables
        /// <summary> Data source. </summary>
        public List<LatLon> Locations { get; set; }
        /// <summary>  </summary>
        public int SymbolSize = 10;
        /// <summary>  </summary>
        public Color SymbolColor = Colors.Blue;
        #endregion

        #region constructor
        /// <summary>  </summary>
        /// <param name="mapView"></param>
        public MyCanvasLayer(MapView mapView)
            : base(mapView)
        { }
        #endregion

        #region event handling
        /// <summary> Event handler for entering an ellipse with the mouse. </summary>
        /// <param name="sender"> Sender of the MouseEnter event. </param>
        /// <param name="e"> Event parameters. </param>
        private void ellipse_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Colors.Red);
        }

        /// <summary> Event handler for leaving an ellipse with the mouse. </summary>
        /// <param name="sender"> Sender of the MouseLeave event. </param>
        /// <param name="e"> Event parameters. </param>
        private void ellipse_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(SymbolColor);
        }
        #endregion

        #region public methods
        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            switch (updateMode)
            {
                case UpdateMode.Refresh:
                    Children.Clear();

                    foreach (var location in Locations)
                    {
                        // calculate ptv location in canvas units
                        Point canvasPoint = GeoToCanvas(new Point(location.Longitude, location.Latitude));

                        // Create the ellipse and insert it to our canvas
                        Ellipse ellipse = new Ellipse
                        {
                            Width = SymbolSize,
                            Height = SymbolSize,
                            Fill = new SolidColorBrush(SymbolColor),
                            Stroke = new SolidColorBrush(Colors.Black),
                        };

                        // set position and add to map
                        Canvas.SetLeft(ellipse, canvasPoint.X - SymbolSize / 2);
                        Canvas.SetTop(ellipse, canvasPoint.Y - SymbolSize / 2);

                        ellipse.RenderTransform = adjustTransform;
                        ellipse.RenderTransformOrigin = new Point(0, 0);

                        // higlight ellipse (only for main map)
                        if (MapView.Name == "Map")
                        {
                            ellipse.MouseEnter += new System.Windows.Input.MouseEventHandler(ellipse_MouseEnter);
                            ellipse.MouseLeave += new System.Windows.Input.MouseEventHandler(ellipse_MouseLeave);
                        }

                        // add ellipse to canvas
                        this.Children.Add(ellipse);
                    }

                    goto case UpdateMode.WhileTransition;
                case UpdateMode.WhileTransition:
                    adjustTransform.ScaleX = this.MapView.CurrentScale;
                    adjustTransform.ScaleY = this.MapView.CurrentScale;
                    break;
            }
        }
        #endregion
    }
}
