using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using System.Windows;
using System.Windows.Controls;

namespace CustomBgProfiles
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            SetProfile(Map1, "sandbox");
            SetProfile(Map2, "silkysand");
            SetProfile(Map2, "gravelpit");
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton)?.Content.ToString())
            {
                case "Standard": SetProfile(Map0, null); break;
                case "Sandbox": SetProfile(Map0, "sandbox"); break;
                case "Silkysand": SetProfile(Map0, "silkysand"); break;
                default: SetProfile(Map0, "gravelpit"); break;
            }
        }

        private static void SetProfile(IMap map, string mapProfile)
        {
            if (map.Layers["Background"] is TiledLayer tiledLayer)
            {
                ((XMapTiledProvider) tiledLayer.TiledProvider).CustomProfile = string.IsNullOrEmpty(mapProfile) ? null : mapProfile + "-bg";
                tiledLayer.Refresh();
            }
            if (map.Layers["Labels"] is UntiledLayer untiledLayer)
            {
                ((XMapTiledProvider) untiledLayer.UntiledProvider).CustomProfile = string.IsNullOrEmpty(mapProfile) ? null : mapProfile + "-fg";
                untiledLayer.Refresh();
            }
        }
    }
}
