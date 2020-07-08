using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Localization;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.UseCases.FeatureLayer;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FeatureLayers
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            referenceTime.Value = DateTime.Now;

            // for on-premis: Map.XMapUrl = "http://127.0.0.1:50010/xmap/ws/XMap");
            Map.XMapUrl = "api-test";
            Map.XMapCredentials = "Insert your xToken here";
            Map.XMapStyle = "gravelpit";

            InitFeatureLayers();

            Map.SetMapLocation(new Point(-74.005833, 40.712778), 14);
        }

        private FeatureLayerPresenter flPresenter;
        private void InitFeatureLayers()
        {
            flPresenter = new FeatureLayerPresenter(Map)
            {
                ReferenceTime = referenceTime.Value,
                UseTrafficIncidents = true,
                UseRestrictionZones = false,
                UseTruckAttributes = false,
                UseSpeedPatterns = false,
                TiLanguage = "EN"
            };

            flPresenter.RefreshMap();
        }

        private void referenceTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (flPresenter == null)
                return;

            flPresenter.ReferenceTime = referenceTime.Value;

            flPresenter.RefreshMap();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (flPresenter == null)
                return;

            flPresenter.UseTrafficIncidents = trafficIncidents.IsChecked.Value;
            flPresenter.UseRestrictionZones = restrictionZones.IsChecked.Value;
            flPresenter.UseTruckAttributes = truckAttributes.IsChecked.Value;
            flPresenter.UseSpeedPatterns = speedPatterns.IsChecked.Value;

            flPresenter.RefreshMap();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (flPresenter == null)
                return;

            flPresenter.TiLanguage = (string)((ComboBoxItem)e.AddedItems[0]).Content;

            flPresenter.RefreshMap();
        }
    }
}
