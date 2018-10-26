using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SymbolsAndLabels
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (true)
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }
    }
}
