//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Controls.Map.Canvases;
using System.Linq;
using Nts = GisSharpBlog.NetTopologySuite;

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
        private readonly ObservableCollection<Path> geometries;
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="SelectionCanvas"/> class. </summary>
        /// <param name="mapView"> The parent map content. </param>
        /// <param name="provider"> The provider to query the geo data source. </param>
        /// <param name="geometries"> All selected geometries. </param>
        public SelectionCanvas(MapView mapView, IGeoProvider provider, ObservableCollection<Path> geometries)
            : base(mapView)
        {
            this.provider = provider;
            this.geometries = geometries;

            geometries.CollectionChanged += geometries_CollectionChanged;

            if (MapView.Name == "Map") // only select on main map
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
            var geoPoint = CanvasToGeo(e.GetPosition(this));

            // ctrl-key adds the selected polygon
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                geometries.Clear();

            // query the topmost item for a point
            var geoItem = provider.QueryPoint(geoPoint.X, geoPoint.Y, null).LastOrDefault();
            if(geoItem.Wkb != null)
            {
                // parse the geometry from the wkb to a WPF path
                // transform to canvas coordinates
                var shape = WkbToWpf.Parse(geoItem.Wkb, GeoToCanvas);

                geometries.Add(new Path { Fill = Brushes.White, Data = shape });

                // sample how draw the polygon inverted to fade out the background
                // http://xserver.ptvgroup.com/forum/viewtopic.php?f=14&t=469
                // read the polygon
                var polygon = new Nts.IO.WKBReader().Read(geoItem.Wkb);
                // create a rectangle for the whole world
                var worldRect = Nts.Geometries.Geometry.DefaultFactory.CreatePolygon(
                Nts.Geometries.Geometry.DefaultFactory.CreateLinearRing(new Nts.Geometries.Coordinate[] {
                     new Nts.Geometries.Coordinate(-180, 85),
                     new Nts.Geometries.Coordinate(180, 85),
                     new Nts.Geometries.Coordinate(180, -85),
                     new Nts.Geometries.Coordinate(-180, -85),
                     new Nts.Geometries.Coordinate(-180, 85)}), null);
                // now substract the polygon from the "world" polygon
                var invertedPolygon = worldRect.Difference(polygon);
                // serialize to wkb
                var wkbInverted = new Nts.IO.WKBWriter().Write(invertedPolygon);
                var invertedShape = WkbToWpf.Parse(wkbInverted, GeoToCanvas);
                geometries.Add(new Path { Fill = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64)), Data = invertedShape });
            }
        }
        #endregion

        #region update methods
        /// <summary> Updates the selected items. </summary>
        private void UpdateSelection()
        {
            Children.Clear();

            foreach (var geometry in geometries)
                Children.Add(geometry);
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
