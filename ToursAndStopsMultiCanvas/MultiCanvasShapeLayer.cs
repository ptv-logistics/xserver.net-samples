using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;

namespace ToursAndStops
{
    /// <summary><para> This class represents a layer containing shape objects. </para>
    /// <para> See the <conceptualLink target="06a654f3-afbd-4f00-9c8e-36997e2a3951"/> topic for an example. </para></summary>
    public class MultiCanvasShapeLayer : BaseLayer
    {
        #region private variables
        /// <summary> Collections of shape elements contained in this layer which are rendered with category content. </summary>
        private ObservableCollection<FrameworkElement> shapes = new ObservableCollection<FrameworkElement>();

        /// <summary> Collections of shape elements contained in this layer which are rendered on top of category content. </summary>
        private ObservableCollection<FrameworkElement> topShapes = new ObservableCollection<FrameworkElement>();
        #endregion

        #region public variables

        /// <summary> Gets or sets the spatial reference number as a string. The spatial reference number defines the
        /// coordinate system to which the coordinates of the shapes belong. </summary>
        public string SpatialReferenceId { get; set; }

        /// <summary> Gets the collection of shapes to be displayed by this layer which are rendered with category content. </summary>
        public ObservableCollection<FrameworkElement> Shapes
        {
            get { return this.shapes; }
        }

        /// <summary> Gets the collection of shapes to be displayed by this layer which are rendered on top of category content. </summary>
        public ObservableCollection<FrameworkElement> TopShapes
        {
            get { return this.topShapes; }
        }
        #endregion

        /// <summary> Gets or sets the update strategy for shapes when the map viewport changes. If lazy update is activated,
        /// the shapes are only updated at the end of the viewport transition. </summary>
        public bool LazyUpdate { get; set; }

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="ShapeLayer"/> class. By default, the spatial reference system is set to "EPSG:4326". </summary>
        /// <param name="name"> Name of the layer. </param>
        public MultiCanvasShapeLayer(string name)
            : base(name)
        {
            this.SpatialReferenceId = "EPSG:4326";

            this.CanvasCategories = new CanvasCategory[] { CanvasCategory.Content, CanvasCategory.Content + 1 };
            this.CanvasFactories = new CanvasFactoryDelegate[] {  
             (map) => (map.Name == "Map") ? new ShapeCanvas(map, shapes, SpatialReferenceId, LazyUpdate) : null,
             (map) => (map.Name == "Map") ? new ShapeCanvas(map, topShapes, SpatialReferenceId, LazyUpdate) : null};

        }
        #endregion
    }
}
