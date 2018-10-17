// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;


namespace Ptv.XServer.Demo.MapMarket
{
    /// <summary> <para>A map canvas which adds select-interaction to the layer.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    public class SelectionCanvas : WorldCanvas
    {
        #region private variables
        
        /// <summary> The provider to query the geo data source. </summary>
        private readonly IGeoProvider provider;

        /// <summary> All selected geometries. </summary>
        private readonly ObservableCollection<Geometry> geometries;
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="SelectionCanvas"/> class. </summary>
        /// <param name="mapView"> The parent map content. </param>
        /// <param name="provider"> The provider to query the geo data source. </param>
        /// <param name="geometries"> All selected geometries. </param>
        public SelectionCanvas(MapView mapView, IGeoProvider provider, ObservableCollection<Geometry> geometries) 
            : base(mapView)
        {
            this.provider = provider;
            this.geometries = geometries;
     
            geometries.CollectionChanged += geometries_CollectionChanged;

            if(MapView.Name == "Map") // only select on main map
                MapView.MouseDown += map_MouseDown;
        }
        #endregion

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

        /// <summary> Notifies about a change of the selection set. </summary>
        /// <param name="sender"> The sender of the CollectionChanged event. </param>
        /// <param name="e"> The event parameters. </param>
        private void geometries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelection();
        }

        /// <summary> Handles the click on the map. </summary>
        /// <param name="sender"> The sender of the MouseDown event. </param>
        /// <param name="e"> The event parameters. </param>
        private void map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed))
                return;
            
            // get clicked coordinate and transform to PTV Mercator
            var canvasPoint = e.GetPosition(this);
            var mercatorPoint = CanvasToPtvMercator(canvasPoint);

            // ctrl-key adds the selected polygon
            if(!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))  
                geometries.Clear();
            
            // query the items for a point (which is a rectangle of size 0)
            foreach (var geoItem in provider.QueryBBox(mercatorPoint.X, mercatorPoint.Y, mercatorPoint.X, mercatorPoint.Y, null))
            {
                // parse the geometry from the wkb to a WPF path
                // transform to canvas coordinates
                var geometry = WkbToWpf.Parse(geoItem.Wkb, PtvMercatorToCanvas);

                // The result set conains all geometries whose *envelope* contains the point.
                // Check for exact containment
                if (geometry.FillContains(canvasPoint))
                    geometries.Add(geometry);
            }
        }
        #endregion

        #region update methods
        /// <summary> Updates the selected items. </summary>
        private void UpdateSelection()
        {
            Children.Clear();

            foreach (var geometry in geometries)
                Children.Add(new Path { Fill = Brushes.Blue, Data = geometry });
        }

        /// <inheritdoc/>
        public override void Update(UpdateMode updateMode)
        {
            if (updateMode == UpdateMode.Refresh)
                UpdateSelection();
        }
        #endregion
    }
}
