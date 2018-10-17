// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ptv.XServer.Controls.Map;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> <para>Control which offers a multi field geocoding. The geocoding request address is entered field by field.</para>
    /// <para>See the <conceptualLink target="fe48cb51-c6ce-487e-b4c0-168537c184c3"/> topic for an example.</para> </summary>
    public partial class MultiFieldControl
    {
        /// <summary> Initializes a new instance of the <see cref="MultiFieldControl"/> class. </summary>
        public MultiFieldControl()
        {
            InitializeComponent();
        }

        /// <summary> Event handler for a click on the locate button. Starts a multi field geocoding with the given
        /// field entries. </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void LocateButton_Click(object sender, RoutedEventArgs e)
        {
            var source = FindResource("MultiFieldDataSource") as MultiFieldData;
            (FindResource("Geocoder") as GeocoderDemo)?.LocateMultiField(source);
        }

        /// <summary> Event handler for a change of the control visibility property. Adds or removes the geocoding
        /// layer to / from the map. </summary>
        /// <param name="sender"> Sender of the IsVisibleChanged event. </param>
        /// <param name="e"> Event parameters. </param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((!(Resources["Geocoder"] is GeocoderDemo geocoder)) || !(sender is UserControl userControl)) return;

            switch (userControl.Visibility)
            {
                case Visibility.Collapsed:
                {
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                        geocoder.Remove(mainWindow.FindName("wpfMap") as WpfMap, false);
                    break;
                }
                case Visibility.Visible:
                {
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                        geocoder.AddTo(mainWindow.FindName("wpfMap") as WpfMap, "MultiFieldGC", false);
                    break;
                }
            }
        }

        /// <summary> Event handler for a key down in one of the text boxes. Starts geocoding when "enter" is pressed. </summary>
        /// <param name="sender"> Sender of the KeyDown event. </param>
        /// <param name="e"> Event handler. </param>
        private void box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var textBox = sender as TextBox;
            if (textBox == countrybox || textBox == statebox || textBox == countrycodebox || textBox == citybox || textBox == streetbox)
            {
                locateButton.Focus();
                LocateButton_Click(this, null);
            }
        }
    }
}
