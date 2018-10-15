//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> Interaction logic for App.xaml </summary>
    public partial class App : Application
    {
        public App()
        {
            // TODO refactor for InfiniteZoom
            Ptv.XServer.Controls.Map.GlobalOptions.InfiniteZoom = false;
        }
    }
}
