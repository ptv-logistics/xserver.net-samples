// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using Ptv.XServer.Demo.Properties;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo
{
    /// <summary> Interaction logic for App.xaml. </summary>
    public partial class App
    {
        /// <summary>
        /// When accessing PTV xServer in the cloud, inject the default token
        /// when no user or password is provided through the configuration file.
        /// </summary>
        private static void SetDefaultPtvXServerInternetToken()
        {
            if (XServerUrl.IsXServerInternet(XServerUrl.Complete(Settings.Default.XUrl, "XMap")) && !String.IsNullOrEmpty(DefaultXServerInternetToken.Value) && String.IsNullOrEmpty(Settings.Default.XToken))
                Settings.Default["XToken"] = DefaultXServerInternetToken.Value; // overwrite settings with default access token
        }
         
        /// <summary>
        /// Static application initializer.
        /// </summary>
        static App()
        {
            SetDefaultPtvXServerInternetToken();
        }
    }
}
