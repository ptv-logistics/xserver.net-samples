using System.Globalization;
using System.Windows;

namespace CustomLocalizer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            // Test: Set explicit culture
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("de"); 

            // Set Custom Localizer
            Ptv.XServer.Controls.Map.Localization.MapLocalizer.Active = new CustomMapLocalizer();

            InitializeComponent();
        }
    }
}
