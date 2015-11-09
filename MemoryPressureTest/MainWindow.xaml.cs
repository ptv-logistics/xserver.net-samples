//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using System;
using System.Runtime.InteropServices;


namespace MemoryPressureTest
{
    /// <summary> Interaction logic for MainWindow.xaml </summary>
    public partial class MainWindow : Window
    {
        private static IntPtr largeUnmanagedHeap;

        #region constructor
        /// <summary> Constructor of the main window. </summary>
        public MainWindow()
        {
            // simulate a large pile (1.3 GB) of unmanaged memory
            // we're running in 32-Bit and only have 2GB per-process
            // so there isn't much left
            largeUnmanagedHeap = Marshal.AllocHGlobal(1300000000);

            // Enable the incrementation of memory pressure for bitmap images
            // This is default for 32-Bit but disabled for 64-Bit applications
            // Must be called before the first initialization of the map control!
            Ptv.XServer.Controls.Map.GlobalOptions.MemoryPressureMode = MemoryPressureMode.Enable;
            
            // decrease the tile cache, the default is 512
            // Must be called before the first initialization of the map control!
            Ptv.XServer.Controls.Map.GlobalOptions.TileCacheSize = 128;

            // now initialize the map
            InitializeComponent();
        }
        #endregion

        #region event handling
        /// <summary> Event handler for a successful loading of the window. </summary>
        /// <param name="sender"> Sender of this event. </param>
        /// <param name="e"> Event parameters. </param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // add xmap base layers
            mapControl1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            mapControl1.XMapCredentials = "xtok:145080320094443";

            // goto KA
            mapControl1.SetMapLocation(new Point(10, 50), 6);

            // basic canvas layer
            mapControl1.Layers.Add(
                new BaseLayer("CanvasLayer")
                {
                    CanvasCategories = new [] { CanvasCategory.Content },
                    CanvasFactories = new BaseLayer.CanvasFactoryDelegate[] { (mapControl) => new MyCanvasLayer(mapControl) { Locations = MyOverlayRenderer.CreateTestLocations(100), SymbolColor = Colors.Yellow } }
                });

        }
        #endregion
    }
}
