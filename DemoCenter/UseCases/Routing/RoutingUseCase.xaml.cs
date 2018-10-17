// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Demo.XrouteService;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Demo.Tools;
using Ptv.XServer.Controls.Map.Tools;


namespace Ptv.XServer.Demo.Routing
{
    /// <summary> <para>Demonstrates the calculation and display of a route.</para>
    /// <para>See the <conceptualLink target="e7c12cc4-3819-470e-867a-b521bee43cf0"/> topic for an example.</para> </summary>
    public partial class RoutingUseCase
    {
        #region private variables
        /// <summary> The WpfMap on which the route calculation is to be displayed. </summary>
        private readonly WpfMap _wpfMap;
        /// <summary> Collection of way points. </summary>
        private readonly ObservableCollection<PlainPoint> wayPoints = new ObservableCollection<PlainPoint>();
        /// <summary> Point where the user ha clicked with the mouse. </summary>
        private PlainPoint clickPoint;
        /// <summary> Flag showing if the routing layer has already been disposed. </summary>
        private bool disposed;
        /// <summary> Displays routes. </summary>
        private readonly BaseLayer routingLayer;
        /// <summary> Displays way points of a route. </summary>
        private readonly BaseLayer wayPointLayer;
        #endregion

        #region constructor
        /// <summary> Initializes a new instance of the <see cref="RoutingUseCase"/> class. Adds two way points and calculates the route. </summary>
        /// <param name="wpfMap"> The map on which the route calculation is to be displayed. </param>
        public RoutingUseCase(WpfMap wpfMap)
        {
            InitializeComponent();

            // save the map
            _wpfMap = wpfMap;

            #region doc:register mouse handler
            _wpfMap.MouseRightButtonDown += wpfMap_MapMouseRightButtonDown;
            ContextMenuService.SetContextMenu(_wpfMap, cm);
            #endregion //doc:register mouse handler

            #region doc:Add ShapeLayers
            routingLayer = new ShapeLayer("Routing") { SpatialReferenceId = "PTV_MERCATOR" };
            wayPointLayer = new ShapeLayer("WayPoints") { SpatialReferenceId = "PTV_MERCATOR" };

            wayPoints.CollectionChanged += points_CollectionChanged;

            // add before labels (if available)
            var idx = _wpfMap.Layers.IndexOf(_wpfMap.Layers["Labels"]);
            if (idx < 0)
            {
                _wpfMap.Layers.Add(routingLayer);
                _wpfMap.Layers.Add(wayPointLayer);
            }
            else
            {
                _wpfMap.Layers.Insert(idx, routingLayer);
                _wpfMap.Layers.Add(wayPointLayer);
            }
            #endregion //doc:Add ShapeLayers

            if (XServerUrl.IsDecartaBackend(XServerUrl.Complete(Properties.Settings.Default.XUrl, "XRoute"))) return;

            // insert way points of Karlsruhe-Berlin
            clickPoint = new PlainPoint { x = 934448.8, y = 6269219.7 };
            SetStart_Click(null, null);
            clickPoint = new PlainPoint { x = 1491097.3, y = 6888163.5 };
            SetEnd_Click(null, null);

            // calculate the route
            CalculateRoute();
        }
        #endregion

        #region doc:removal
        /// <summary> Removes the route from display. </summary>
        public void Remove()
        {
            wayPoints.CollectionChanged -= points_CollectionChanged;
            _wpfMap.MouseRightButtonDown -= wpfMap_MapMouseRightButtonDown;
            ContextMenuService.SetContextMenu(_wpfMap, null);

            _wpfMap.Layers.Remove(_wpfMap.Layers["Routing"]);
            _wpfMap.Layers.Remove(_wpfMap.Layers["WayPoints"]);

            disposed = true;
        }
        #endregion

        #region helper methods
        /// <summary> Adds a pin to the map for each way point. </summary>
        #region doc:update the way point pins
        private void UpdateWayPointPins()
        {
            // already disposed
            if (disposed)
                return;

            (wayPointLayer as ShapeLayer)?.Shapes.Clear();

            foreach (var wayPoint in wayPoints)
            {
                var pin = new Pin();

                if (wayPoints.IndexOf(wayPoint) == 0)
                    pin.Color = Colors.Green;
                else if (wayPoints.IndexOf(wayPoint) == wayPoints.Count - 1)
                    pin.Color = Colors.Red;
                else
                    pin.Color = Colors.Blue;
                
                #region doc:waypoint context menu
                var cmWayPoints = new ContextMenu();
                var item = new MenuItem {Header = "Remove", CommandParameter = wayPoint};
                item.Click += RemoveItemClick;
                cmWayPoints.Items.Add(item);
                item = new MenuItem {Header = "Move up", CommandParameter = wayPoint};
                item.Click += MoveUpItemClick;
                cmWayPoints.Items.Add(item);
                item = new MenuItem {Header = "Move down", CommandParameter = wayPoint};
                item.Click += MoveDownItemClick;
                cmWayPoints.Items.Add(item);
                ContextMenuService.SetContextMenu(pin, cmWayPoints);
                #endregion

                pin.Width = 40;
                pin.Height = 40;

                // set tool tip information
                ToolTipService.SetToolTip(pin, wayPoints.IndexOf(wayPoint) == 0 ? "Start" : wayPoints.IndexOf(wayPoint) == 0 ? "End" : "Waypoint " + (wayPoints.IndexOf(wayPoint) + 1));

                pin.Name = "Waypoint" + (wayPoints.IndexOf(wayPoint) + 1);
                
                Panel.SetZIndex(pin, -1);

                ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
                ShapeCanvas.SetLocation(pin, new System.Windows.Point(wayPoint.x, wayPoint.y));

                (wayPointLayer as ShapeLayer)?.Shapes.Add(pin);
            }

            wayPointLayer.Refresh();
        }
        #endregion //doc:update the way point pins

        /// <summary> Calculates the route between the given way points. </summary>
        private void CalculateRoute()
        {
            switch (wayPoints.Count)
            {
                case 0: Dispatcher.BeginInvoke(new Action<string>(DisplayError), Properties.Resources.ErrorNoWaypointsSet); return;
                case 1: Dispatcher.BeginInvoke(new Action<string>(DisplayError), Properties.Resources.ErrorNoDestWayPoint); return;
            }

            // reset context menu
            ContextMenuService.SetContextMenu(_wpfMap, null);

            #region doc:call xroute
            // build xServer wayPoints array from wayPoints
            var selectedWayPoints = wayPoints.Select(p => new WaypointDesc
            {
                wrappedCoords = new[] { new XrouteService.Point { point = p } },
                linkType = LinkType.NEXT_SEGMENT,
                fuzzyRadius = 10000
            }).ToArray();

            Dispatcher.Invoke(delegate { Mouse.OverrideCursor = Cursors.Wait; });

            XRouteWS xRoute = XServerClientFactory.CreateXRouteClient(Properties.Settings.Default.XUrl);

            try
            {
                xRoute.BegincalculateRoute(new calculateRouteRequest
                {
                    ArrayOfWaypointDesc_1 = selectedWayPoints,
                    ResultListOptions_4 = new ResultListOptions { polygon = true }
                }, Invoke, xRoute);
            }
            catch (EntryPointNotFoundException)
            {
                Dispatcher.BeginInvoke(new Action<string>(DisplayError), Properties.Resources.ErrorRouteEndpointNotFound);
            }
            #endregion //doc:call xroute
        }

        /// <summary> Invokes showing the routing result on the GUI. </summary>
        /// <param name="result"> The result of the route calculation. </param>
        #region doc:evaluate response
        private void Invoke(IAsyncResult result)
        {
            try
            {
                // not the UI thread!
                calculateRouteResponse response = (result.AsyncState as XRouteWS)?.EndcalculateRoute(result);

                Dispatcher.BeginInvoke(new Action<Route>(DisplayRouteInMap), response?.result);
                Dispatcher.BeginInvoke(new Action(delegate { ClearRouteItem.IsEnabled = true; }));
            }
            catch (EntryPointNotFoundException)
            {
                Dispatcher.BeginInvoke(new Action<string>(DisplayError), Properties.Resources.ErrorRouteEndpointNotFound);
            }
            catch (System.ServiceModel.FaultException<XRouteException> e)
            {
                XRouteException ex = e.Detail;
                var s = ex.stackElement;
                Dispatcher.Invoke(delegate { Mouse.OverrideCursor = null; });

                Dispatcher.BeginInvoke(new Action<string>(DisplayError),
                    (s.errorKey == null || s.errorKey.Equals("2530")) ? Properties.Resources.ErrorRouteCalculationDefault : s.message);

                return;
            }
            catch (Exception)
            {
                Dispatcher.Invoke(delegate { Mouse.OverrideCursor = null; });
                Dispatcher.BeginInvoke(new Action<string>(DisplayError), Properties.Resources.ErrorRouteCalculationDefault);
                return;
            }

            Dispatcher.Invoke(delegate { Mouse.OverrideCursor = null; });
        }
        #endregion //doc:evaluate response

        /// <summary> Shows an error in a message box. </summary>
        /// <param name="errorMessage"> The error message. </param>
        private void DisplayError(string errorMessage)
        {
            ContextMenuService.SetContextMenu(_wpfMap, cm);
            MessageBox.Show(errorMessage);
        }

        /// <summary> Initializes the user interface for showing the route on screen. </summary>
        /// <param name="route"> The calculated route.</param>
        #region doc:display result
        private void DisplayRouteInMap(Route route)
        {
            ContextMenuService.SetContextMenu(_wpfMap, cm);
       
            // already disposed
            if (disposed) return;

            (routingLayer as ShapeLayer)?.Shapes.Clear();

            if (route == null) return;

            var points = new PointCollection();
            foreach (PlainPoint p in route.polygon.lineString.wrappedPoints)
                points.Add(new System.Windows.Point(p.x, p.y));

            new UseCases.RoutePolyline(routingLayer as ShapeLayer)
            {
                Points = points,
                ToolTip = $"{route.info.distance / 1000.0:0,0.0}km\n{TimeSpan.FromSeconds(route.info.time)}",
                Color = Colors.Blue
            };

            routingLayer.Refresh();
            wayPointLayer.Refresh();

            ZoomToRoute(route);
        }
        #endregion //doc:display result


        /// <summary>
        /// Refreshes the UI elements of the route.
        /// </summary>
        private void RefreshRouteUIElements()
        {
            DisplayRouteInMap(null);
            UpdateWaypointMenuItems();
            AddBetweenItem.IsEnabled = wayPoints.Count > 1;
            CalculateRouteItem.IsEnabled = wayPoints.Count > 1;
        }

        /// <summary>Sets the map view to the extend of the route. </summary>
        /// <param name="route"> The route object which should be centered in the map view. </param>
        private void ZoomToRoute(Route route)
        {
            var winPoints = from plainPoint in route.polygon.lineString.wrappedPoints 
                            select new System.Windows.Point(plainPoint.x, plainPoint.y);

            _wpfMap.SetEnvelope(new MapRectangle(winPoints).Inflate(1.2), "PTV_MERCATOR");
        }
        #endregion

        #region event handling
        #region doc:waypoint menu event handling
        /// <summary> Event handler for a click on a way point. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void RemoveItemClick(object sender, RoutedEventArgs e)
        {
            wayPoints.Remove((sender as MenuItem)?.CommandParameter as PlainPoint);
            RefreshRouteUIElements();
        }

        /// <summary> Event handler for a click on a way point. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void MoveUpItemClick(object sender, RoutedEventArgs e)
        {
            int index = wayPoints.IndexOf((sender as MenuItem)?.CommandParameter as PlainPoint);
            if (index <= 0) return;

            wayPoints.RemoveAt(index);
            wayPoints.Insert(index - 1, (sender as MenuItem)?.CommandParameter as PlainPoint);
            RefreshRouteUIElements();
        }

        /// <summary> Event handler for a click on a way point. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void MoveDownItemClick(object sender, RoutedEventArgs e)
        {
            int index = wayPoints.IndexOf((sender as MenuItem)?.CommandParameter as PlainPoint);
            if (index >= wayPoints.Count - 1) return;

            wayPoints.RemoveAt(index);
            wayPoints.Insert(index + 1, (sender as MenuItem)?.CommandParameter as PlainPoint);
            RefreshRouteUIElements();
        }

        #endregion

        /// <summary> Event handler for changing the collection of way points. </summary>
        /// <param name="sender"> The sender of the CollectionChanged event. </param>
        /// <param name="e"> The event parameters. </param>
        private void points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateWayPointPins();
        }

        ///// <summary> Event handler for clicking with the right mouse on a way point pin. </summary>
        ///// <param name="sender"> The sender of the MouseRightButtonDown event. </param>
        ///// <param name="e"> The event parameters. </param>
        //private void pin_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    selectedElement = sender as FrameworkElement;
        //    e.Handled = false;
        //}

        /// <summary> Event handler for a right click in the map. </summary>
        /// <param name="sender"> The sender of the MouseRightButtonDown event. </param>
        /// <param name="e"> The event parameters. </param>
        #region doc:save the click point
        private void wpfMap_MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = _wpfMap.MouseToGeo(e, "PTV_MERCATOR");
            clickPoint = new PlainPoint { x = p.X, y = p.Y };

            e.Handled = false;
        }
        #endregion //doc:save the click point

        /// <summary>
        /// Determines the index of the last static menu item of the route context menu.
        /// </summary>
        /// <returns>The index of the last static menu item of the route context menu.</returns>
        private int GetIndexOfLastStaticContextMenuItem()
        {
            return cm.Items.IndexOf(cm.Items.OfType<MenuItem>().ToList().First(item => item.Header.Equals("Clear Route")));
        }

        /// <summary>
        /// Handler for clicking an item in the waypoint list of the route context menu. This handler
        /// sets the map center to the clicked item.
        /// </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void WaypointClicked(object sender, RoutedEventArgs e)
        {
            int firstWaypointItemIndex = GetIndexOfLastStaticContextMenuItem() + 2;
            int wayPointIndex = cm.Items.IndexOf(sender) - firstWaypointItemIndex;

            var point = wayPoints[wayPointIndex];
            _wpfMap.SetMapLocation(new System.Windows.Point(point.x, point.y),
                _wpfMap.Zoom, "PTV_MERCATOR");
        }

        /// <summary>
        /// Updates the list of waypoints displayed in the route context menu.
        /// </summary>
        private void UpdateWaypointMenuItems()
        {
            int insertIndex = GetIndexOfLastStaticContextMenuItem();

            if (wayPoints.Count > 0 && cm.Items.Count == insertIndex + 1)
            {
                cm.Items.Add(new Separator());
            }
            else if (wayPoints.Count == 0 && cm.Items.Count > insertIndex + 2)
            {
                while (cm.Items.Count > insertIndex + 1)
                {
                    cm.Items.RemoveAt(cm.Items.Count - 1);
                }
                return;
            }

            while (cm.Items.Count > insertIndex + 2)
            {
                cm.Items.RemoveAt(cm.Items.Count - 1);
            }

            var items = wayPoints.Select(wayPoint => new MenuItem
            {
                Icon = new Pin { Color = (wayPoints.IndexOf(wayPoint) == 0 ? Colors.Green : (wayPoints.IndexOf(wayPoint) == wayPoints.Count - 1) ? Colors.Red : Colors.Blue) },
                Header = (wayPoints.IndexOf(wayPoint) == 0 ? "Start" : (wayPoints.IndexOf(wayPoint) == wayPoints.Count - 1) ? "End" : "Waypoint " + (wayPoints.IndexOf(wayPoint) + 1))
            }).ToArray();

            foreach (var item in items)
            {
                item.Click += WaypointClicked;
                cm.Items.Add(item);
            }
        }

        #region doc:menu item clicks
        /// <summary> Event handler for a click on the set start menu entry. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void SetStart_Click(object sender, RoutedEventArgs e)
        {
            wayPoints.Insert(0, clickPoint);
            RefreshRouteUIElements();
        }

        /// <summary> Event handler for a click on the set end menu entry. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void SetEnd_Click(object sender, RoutedEventArgs e)
        {
            wayPoints.Add(clickPoint);
            RefreshRouteUIElements();
        }

        /// <summary> Event handler for a click on the calculate route menu entry. </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void CalcRoute_Click(object sender, RoutedEventArgs e)
        {
            CalculateRoute();
        }

        /// <summary>
        /// Handler for clearing the route.
        /// </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void ClearRouteItem_Click(object sender, RoutedEventArgs e)
        {
            wayPoints.Clear();
            RefreshRouteUIElements();
            ClearRouteItem.IsEnabled = false;
        }

        /// <summary>
        /// Handler for adding a waypoint between start and end. The waypoint will be added directly
        /// before the end waypoint.
        /// </summary>
        /// <param name="sender"> The sender of the Click event. </param>
        /// <param name="e"> The event parameters. </param>
        private void AddBetween_Click_1(object sender, RoutedEventArgs e)
        {
            wayPoints.Insert(wayPoints.Count - 1, clickPoint);
            RefreshRouteUIElements();
        }
        #endregion //doc:menu item clicks
        #endregion
    }
}
