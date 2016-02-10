using System;
using System.Threading.Tasks;
using Ptv.XServer.Controls.Map;
using System.Windows.Threading;

namespace BasemapTooltips
{
    public class BaseMapToolTips
    {
        private WpfMap map;
        private System.Windows.Controls.ToolTip tooltip;
        private DispatcherTimer toolTipTimer;

        /// <summary>
        /// The delagate for reverse locating
        /// </summary>
        public Func<double, double, LocationTooltipInfo> ReverseLocatingFunc;

        /// <summary>
        /// Constructs a new Tooltips plugin
        /// </summary>
        /// <param name="map">The map control</param>
        /// <param name="delay">The delay for displaying tool tips</param>
        public BaseMapToolTips(WpfMap map, int delay = 1000)
        {
            this.map = map;
            map.MouseMove += Map_MouseMove;

            toolTipTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, delay) };
            toolTipTimer.Tick += toolTipTimer_Tick;
        }

        async void toolTipTimer_Tick(object sender, EventArgs ea)
        {
            toolTipTimer.Stop();

            var e = toolTipTimer.Tag as System.Windows.Input.MouseEventArgs;

            var geo = map.MouseToGeo(e);
            if (tooltip == null)
                tooltip = new System.Windows.Controls.ToolTip();

            var result = await Task.Run(() => ReverseLocatingFunc(geo.X, geo.Y));

            if (result != null)
            {
                string toolTipString = result.Country;

                if (map.Zoom > 4 && !string.IsNullOrEmpty(result.City))
                    toolTipString += "\n" + result.City;

                if(map.Zoom > 10 && !string.IsNullOrEmpty(result.Street))
                    toolTipString += "\n" + result.Street;

                if (!string.IsNullOrEmpty(toolTipString))
                {
                    tooltip.Content = toolTipString;
                    tooltip.IsOpen = true;                    
                }
            }
        }

        void Map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (tooltip != null)
            {
                tooltip.IsOpen = false;
            }

            toolTipTimer.Stop();
            toolTipTimer.Start();
            toolTipTimer.Tag = e;
        }
    }

    /// <summary>
    /// The struct containing the tooltip information
    /// </summary>
    public class LocationTooltipInfo
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }
}
