// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Specialized;
using SharpMap.Data.Providers;
using SharpMap.Geometries;
using SharpMap.Data;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;

namespace Ptv.XServer.Demo.ShapeFile
{
    /// <summary>
    /// This class is responsible for highlighting selected shapes.
    /// </summary>
    public class ShapeSelectionCanvas : WorldCanvas
    {
        #region private variables
        /// <summary>Holds the selected geometries.</summary>
        private readonly ObservableCollection<System.Windows.Media.Geometry> geometries;
        /// <summary>The shape file used to read data for selection highlighting.</summary>
        private readonly string shapeFilePath;
        #endregion

        /// <summary> Initializes a new instance of the <see cref="ShapeSelectionCanvas"/> class. Creates a new
        /// instance of the given shape file. </summary>
        /// <param name="mapView"> The map. </param>
        /// <param name="shapeFilePath"> The absolute path to the shape file. </param>
        /// <param name="geometries"> Holds the geometries to highlight. </param>
        public ShapeSelectionCanvas(MapView mapView, string shapeFilePath, ObservableCollection<System.Windows.Media.Geometry> geometries)
            : base(mapView)
        {
            this.geometries = geometries;
            this.shapeFilePath = shapeFilePath;
            geometries.CollectionChanged += geometries_CollectionChanged;

            if (MapView.Name == "Map") // only select on main map
                MapView.MouseDown += map_MouseDown;
        }

        #region disposal
        /// <inheritdoc/>
        public override void Dispose()
        {
            geometries.CollectionChanged -= geometries_CollectionChanged;

            if (MapView.Name == "Map")
                MapView.MouseDown -= map_MouseDown;

            base.Dispose();
        }
        #endregion

        #region event handling
        /// <summary>  Notifies about a change of the selection set.  </summary>
        /// <param name="sender"> The sender of the CollectionChanged event. </param>
        /// <param name="e"> The event parameters. </param>
        private void geometries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelection();
        }

        /// <summary> Event handler for MouseDown event.  </summary>
        /// <param name="sender"> The sender of the MouseDown event. </param>
        /// <param name="e"> The event parameters. </param>
        #region doc:map_MouseDown handler
        private void map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed))
                return;

            var canvasPoint = e.GetPosition(this);
            System.Windows.Point wgsPoint = CanvasToGeo(canvasPoint);
            var ds = new FeatureDataSet();

            geometries.Clear();
            using (var shpFile = new SharpMap.Data.Providers.ShapeFile(shapeFilePath))
            {
                shpFile.Open();
                shpFile.ExecuteIntersectionQuery(new BoundingBox(wgsPoint.X, wgsPoint.Y, wgsPoint.X, wgsPoint.Y), ds);

                //if selected any object
                if (ds.Tables[0].Rows.Count <= 0) return;

                using (var geomProvider = new GeometryProvider(ds.Tables[0]))
                {
                    geomProvider.Open();
                    foreach (var geoItem in geomProvider.Geometries)
                    {
                        var geometry = WkbToWpf.Parse(geoItem.AsBinary(), GeoToCanvas);
                        if (geometry.FillContains(canvasPoint))
                            geometries.Add(geometry);
                    }
                }
            }
        }
        #endregion //doc:map_MouseDown handler
        #endregion

        #region update methods
        /// <summary> Updates the selected objects set. </summary>
        #region doc:UpdateSelection method
        private void UpdateSelection()
        {
            Children.Clear();

            foreach (var geometry in geometries)
                Children.Add(new System.Windows.Shapes.Path { Fill = System.Windows.Media.Brushes.Blue, Data = geometry });
        }
        #endregion //doc:UpdateSelection method

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            if (updateMode == UpdateMode.Refresh)
                UpdateSelection();
        }
        #endregion
    }
}
