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
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers.Untiled;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class MyHybridLayer : WorldCanvas
    {
        #region private variables
        /// <summary> Canvas used for shallow zoom levels. </summary>
        private WorldCanvas bitmapCanvas = null;

        /// <summary> Canvas used for deep zoom levels. </summary>
        private MyDeepZoomCanvas vectorCanvas = null;
        #endregion

        #region public variables
        /// <summary>  </summary>
        public List<LatLon> Locations { get; set; }
        /// <summary>  </summary>
        public int SymbolSize = 10;
        /// <summary>  </summary>
        public System.Windows.Media.Color SymbolColor = System.Windows.Media.Colors.Blue;
        #endregion

        #region constructor
        /// <summary>  </summary>
        /// <param name="mapView"></param>
        public MyHybridLayer(MapView mapView)
            : base(mapView)
        { }
        #endregion

        #region public methods
        /// <summary> Skip transform initialization, transformation is set by child canvases. </summary>
        public override void InitializeTransform() { }

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            // use bitmap rendering vor levels < 10
            if (MapView.FinalZoom < 10)
            {
                if (vectorCanvas != null)
                {
                    this.Children.Remove(vectorCanvas);
                    vectorCanvas = null;
                }

                if (bitmapCanvas == null)
                {
                    bitmapCanvas = new UntiledCanvas(MapView, new MyOverlayRenderer() { 
                        SymbolColor = this.SymbolColor,
                        SymbolSize = this.SymbolSize,
                        Locations = this.Locations }, false);
                    this.Children.Add(bitmapCanvas);
                }

                bitmapCanvas.Update(updateMode);
            }
            else
            {
                if (bitmapCanvas != null)
                {
                    this.Children.Remove(bitmapCanvas);
                    bitmapCanvas = null;
                }

                if (vectorCanvas == null)
                {
                    vectorCanvas = new MyDeepZoomCanvas(MapView)
                    {
                        SymbolColor = this.SymbolColor,
                        SymbolSize = this.SymbolSize,
                        Locations = this.Locations
                    };
                    this.Children.Add(vectorCanvas);
                }

                vectorCanvas.Update(updateMode);
            }
        }
        #endregion
    }

    /// <summary>  </summary>
    public class MyDeepZoomCanvas : WorldCanvas
    {
        #region public variables
        /// <summary>  </summary>
        public int SymbolSize = 10;
        /// <summary>  </summary>
        public Color SymbolColor = Colors.Blue;
        /// <summary>  </summary>
        public List<LatLon> Locations { get; set; }
        #endregion

        #region constructor
        /// <summary>  </summary>
        /// <param name="mapControl"></param>
        public MyDeepZoomCanvas(MapView mapView)
            : base(mapView, false)
        { }
        #endregion

        #region public methods
        /// <summary> Vector-based rendering for deeper zoom levels. </summary>
        /// <param name="updateMode"></param>
        public override void Update(UpdateMode updateMode)
        {
            if (updateMode != UpdateMode.Refresh && updateMode != UpdateMode.BeginTransition)
                return;

            //var rect = Map.GetFinalEnvelopeLatLon();
            //Children.Clear();
            //var ellipseSize = SymbolSize * Map.FinalScale;
            //foreach (var location in Locations)
            //{
            //    var wgsPoint = new Point(location.Longitude, location.Latitude);

            //    if (!rect.Contains(wgsPoint))
            //        continue;

            //    // calculate ptv location in ptv mercator units
            //    Point canvasPoint = GeoToCanvas(wgsPoint);

            //    // Create the ellipse and insert it to our canvas
            //    Ellipse ellipse = new Ellipse
            //    {
            //        Width = ellipseSize,
            //        Height = ellipseSize,
            //        Fill = new SolidColorBrush(SymbolColor),
            //    };

            //    ellipse.ToolTip = "I'm a WPF element";

            //    // set position and add to map
            //    Canvas.SetLeft(ellipse, canvasPoint.X - ellipseSize / 2);
            //    Canvas.SetTop(ellipse, canvasPoint.Y - ellipseSize / 2);

            //    // add ellipse to canvas
            //    this.Children.Add(ellipse);
            //}
        }
        #endregion
    }
}
