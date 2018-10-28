//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class ShapeLayer : BaseLayer
    {
        #region public variables
        /// <summary>  </summary>
        public Type VectorLayerType { get; set; }
        #endregion

        #region constructor
        /// <summary>  </summary>
        /// <param name="name"></param>
        public ShapeLayer(string name)
            : base(name)
        {
            InitializeFactory(CanvasCategory.Content, 
            map => new VectorCanvas(map, (UIElement)Activator.CreateInstance(VectorLayerType)));
        }
        #endregion
    }

    /// <summary>  </summary>
    public class VectorCanvas : WorldCanvas
    {
        #region constructor
        /// <summary>  </summary>
        /// <param name="mapView"></param>
        public VectorCanvas(MapView mapView)
            : base(mapView)
        { }

        /// <summary>  </summary>
        /// <param name="mapView"></param>
        /// <param name="xaml"></param>
        public VectorCanvas(MapView mapView, UIElement xaml)
            : base(mapView)
        {
            Children.Add(xaml);
        }
        #endregion

        #region public methods
        /// <summary>  </summary>
        public override void InitializeTransform()
        {
            const double mercatorSize = 360;
            var translateTransform = new TranslateTransform(MapView.ReferenceSize / 2 - 180, MapView.ReferenceSize / 2 - 180);
            var zoomTransform = new ScaleTransform(MapView.ZoomAdjust * MapView.ReferenceSize / mercatorSize, MapView.ZoomAdjust * MapView.ReferenceSize / mercatorSize, MapView.ReferenceSize / 2, MapView.ReferenceSize / 2);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(zoomTransform);

            zoomTransform.Freeze();
            translateTransform.Freeze();
            transformGroup.Freeze();

            RenderTransform = transformGroup;
        }

        /// <summary>  </summary>
        /// <param name="updateMode"></param>
        public override void Update(UpdateMode updateMode)
        {
            UpdateScales(this);
        }

        /// <summary>  </summary>
        /// <param name="element"></param>
        public void UpdateScales(object element)
        {
            if (element is Panel)
            {
                foreach (var child in ((Panel) element).Children)
                    UpdateScales(child);
            }
            else if (element is ContentControl)
            {
                UpdateScales(((ContentControl) element).Content);
            }
            else if (element is MapShape)
            {
                var mapShape = (MapShape) element;
                mapShape.StrokeThickness = mapShape.InvariantStrokeThickness * MapView.CurrentScale / 20015087.0 * 180;
            }
        }
        #endregion
    }
}
