using System.Windows;
using System.Windows.Controls;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.TileProviders;

namespace CustomBgProfiles
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            ((Map1.Layers["Background"] as TiledLayer).TiledProvider as XMapTiledProvider).CustomProfile = "ajax-bg-greenzones";
            ((Map2.Layers["Background"] as TiledLayer).TiledProvider as XMapTiledProvider).CustomProfile = "ajax-bg-sandbox";
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Content.ToString())
            {
                case "Standard":
                    ((Map0.Layers["Background"] as TiledLayer).TiledProvider as XMapTiledProvider).CustomProfile = null; // uses ajax-bg
                    (Map0.Layers["Background"] as TiledLayer).Refresh();
                    break;
                case "Greenzones":
                    ((Map0.Layers["Background"] as TiledLayer).TiledProvider as XMapTiledProvider).CustomProfile = "ajax-bg-greenzones";
                    (Map0.Layers["Background"] as TiledLayer).Refresh();
                    break;
                default:
                    ((Map0.Layers["Background"] as TiledLayer).TiledProvider as XMapTiledProvider).CustomProfile = "ajax-bg-sandbox";
                    (Map0.Layers["Background"] as TiledLayer).Refresh();
                    break;
            }
        }
    }
}
