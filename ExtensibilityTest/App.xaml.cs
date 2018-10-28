//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> Interaction logic for App.xaml </summary>
    public partial class App
    {
        public App()
        {
            // TODO refactor for InfiniteZoom
            Controls.Map.GlobalOptions.InfiniteZoom = false;
        }
    }
}
