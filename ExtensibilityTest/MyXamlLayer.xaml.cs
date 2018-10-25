//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Input;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> Interaction logic for MyXamlLayer.xaml </summary>
    public partial class MapXaml
    {
        #region constructor
        /// <summary>  </summary>
        public MapXaml()
        {
            InitializeComponent();
        }
        #endregion

        #region event handling
        private void MapPolygon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hallo");
        }
        #endregion
    }
}
