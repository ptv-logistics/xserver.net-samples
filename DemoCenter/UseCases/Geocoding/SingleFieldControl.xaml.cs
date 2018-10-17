// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ptv.XServer.Controls.Map;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> <para>Control which offers a single field geocoding. The search address is entered as a single string
    /// containing all information.</para>
    /// <para>See the <conceptualLink target="fe48cb51-c6ce-487e-b4c0-168537c184c3"/> topic for an example.</para> </summary>
    public partial class SingleFieldControl
    {
        /// <summary> Initializes a new instance of the <see cref="SingleFieldControl"/> class. </summary>
        public SingleFieldControl()
        {
            InitializeComponent();
        }

        /// <summary> Event handler for a click on the locate button. Starts a single field geocoding request. </summary>
        /// <param name="sender"> Sender of the Click event. </param>
        /// <param name="e"> Event parameters. </param>
        private void LocateButton_Click(object sender, RoutedEventArgs e)
        {
            var geocoder = Resources["Geocoder"] as GeocoderDemo;
            geocoder?.LocateSingleField(locatebox.Text);
        }

        /// <summary> Event handler for arbitrary events. Using this handler prevents the events from bubbling up to
        /// parent objects. The event is marked as handled here and thus it is stopped and not forwarded to parent
        /// events. </summary>
        /// <param name="sender"> Sender of the event. </param>
        /// <param name="e"> Event parameters. </param>
        private void PreventBubblingUpHandler(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary> Event handler for a change of the control visibility property. Adds or removes the geocoding
        /// layer to / from the map. </summary>
        /// <param name="sender"> Sender of the IsVisibleChanged event. </param>
        /// <param name="e"> Event parameters. </param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(Resources["Geocoder"] is GeocoderDemo geocoder) || !(sender is UserControl userControl)) return;

            switch (userControl.Visibility)
            {
                case Visibility.Collapsed:
                {
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                        geocoder.Remove(mainWindow.FindName("wpfMap") as WpfMap, true);
                    break;
                }
                case Visibility.Visible:
                {
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                        geocoder.AddTo(mainWindow.FindName("wpfMap") as WpfMap, "SingleFieldGC", true);
                    break;
                }
            }
        }

        /// <summary> Event handler for a key down action on the locate box. Starts locating the address when pressing "enter". </summary>
        /// <param name="sender"> Sender of the KeyDown event. </param>
        /// <param name="e"> Event parameters. </param>
        private void locatebox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Enter) || (locatebox.Text != locatebox.SelectedText)) return;

            locateButton.Focus();
            LocateButton_Click(this, null);
        }
    }
}
