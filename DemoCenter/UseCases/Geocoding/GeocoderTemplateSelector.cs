// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows.Controls;
using System.Windows;

namespace Ptv.XServer.Demo.Geocoding
{
    /// <summary> Selector of the data template which is used for the AutoCompleteTextBox. Either a wait message or the
    /// geocoding results can be displayed there. </summary>
    public class GeocoderTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window wnd = Application.Current.MainWindow;
            return wnd?.FindResource((item is string) ? "WaitTemplate" : "GeocoderSuggestionTemplate") as DataTemplate;
        }
    }
}
