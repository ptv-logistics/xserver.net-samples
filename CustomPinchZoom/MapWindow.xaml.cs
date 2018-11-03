using System.Diagnostics;
using Ptv.XServer.Controls.Map.Tools;
using System.Windows;
using System.Windows.Controls;

namespace CustomPinchZoom
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MapWindow
    {
        private readonly PanAndZoom customPanAndZoom = new PanAndZoom();

        public MapWindow()
        {
            InitializeComponent();

            // set map parameters
            Map.XMapStyle = "silkysand";
            Map.XMapUrl = "https://api-test.cloud.ptvgroup.com/xmap/ws/XMap";
            Map.XMapCredentials = "xtok:BB2A4CCB-65D9-4783-BCA6-529AD7A6F4C4"; // just a test-token here. Use your own token

            // get the map container grid
            var grid = MapElementExtensions.FindChild<Grid>(Map);

            // get the old interactor
            var pz = Map.FindRelative<Ptv.XServer.Controls.Map.Gadgets.PanAndZoom>();

            // exchange the interactor
            grid.Children.Remove(pz);
            grid.Children.Add(customPanAndZoom);

            Map.FitInWindow = true;

            // check token
            if (!string.IsNullOrWhiteSpace(Map.XMapCredentials) && !Map.XMapCredentials.Contains("insert")) return;

            MessageBox.Show("Please adopt the Map.XMapCredentials parameter in MapWindow.xaml.cs and provide a valid token.");
            Process.GetCurrentProcess().Kill();
        }
    }
}
