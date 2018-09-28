using System;
using System.Diagnostics;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;
using System.Windows;
using System.Windows.Controls;
using Ptv.XServer.Controls.Map.Gadgets;

namespace CustomPinchZoom
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        readonly PanAndZoom customPanAndZoom = new PanAndZoom();

        public MapWindow()
        {
            InitializeComponent();

            // set map parameters
            Map.XMapStyle = "silkysand";
            Map.XMapUrl = "https://api-test.cloud.ptvgroup.com/xmap/ws/XMap";
            Map.XMapCredentials = "xtok:<insert-your-token-here>";

            // get the map container grid
            var grid = MapElementExtensions.FindChild<Grid>(Map);

            // get the old interactor
            var pz = Map.FindRelative<Ptv.XServer.Controls.Map.Gadgets.PanAndZoom>();

            // exchange the interactor
            grid.Children.Remove(pz);
            grid.Children.Add(customPanAndZoom);

            Map.FitInWindow = true;

            // check token
            if (string.IsNullOrWhiteSpace(Map.XMapCredentials) || Map.XMapCredentials.Contains("insert"))
            {
                MessageBox.Show("Please adopt the Map.XMapCredentials parameter in MapWindow.xaml.cs and provide a valid token.");
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
