using Ptv.XServer.Controls.Map;
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

            Map1.XMapStyle = "silkysand";
            Map2.XMapStyle = "gravelpit";
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Content.ToString())
            {
                case "Standard": SetProfile(Map0, null); break;
                case "Sandbox": SetProfile(Map0, "sandbox"); break;
                case "Silkysand": SetProfile(Map0, "silkysand"); break;
                default: SetProfile(Map0, "gravelpit"); break;
            }
        }

        private static void SetProfile(IMap map, string mapProfile)
        {
            map.XMapStyle = mapProfile;
        }
    }
}
