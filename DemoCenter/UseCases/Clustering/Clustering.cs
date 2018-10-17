// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Demo.Clustering;

namespace Ptv.XServer.Demo.UseCases.Clustering
{
    /// <summary>
    /// Provides the functionality to add a clustering Layer to a WpfMap.
    /// </summary>
    public class ClusteringUseCase : UseCase
    {
        /// <summary>
        /// UI element which should be disabled because an asynchronous task is started by activating it.
        /// Otherwise inconsistencies may occur due to multiple starts and stops of this use case, resulting
        /// in multiple indeterminated calls of the Enable() method.
        /// </summary>
        public CheckBox CheckBoxToDisable;

        /// <summary>
        /// Adds a Clustering Layer to the specified WpfMap.
        /// </summary>
        protected override async void Enable()
        {
            if (wpfMap.Layers.Contains(wpfMap.Layers["Clustering"]))
                return;

            if (CheckBoxToDisable != null)
                CheckBoxToDisable.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.AppStarting;

            #region doc:add clustering layer
            // Start the longtime reading of geo data in a separate thread.
            var clusters = await Task.Factory.StartNew(LocalizedWikiDemo.ReadCSVFile);

            // Creates a clustering layer.
            var clusteringLayer = new BaseLayer("Clustering")
            {
                CanvasCategories = new[] { CanvasCategory.Content },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[] { map => new LocalizedWikiDemo(map, clusters) }
            };

            // Adds the created clustering layer to the WpfMap.
            wpfMap.Layers.Add(clusteringLayer);
            #endregion

            #region doc:zoom clustering layer
            wpfMap.SetMapLocation(new Point(8.4, 49), 10);
            #endregion

            Mouse.OverrideCursor = null;
            if (CheckBoxToDisable != null)
                CheckBoxToDisable.IsEnabled = true;
        }

        /// <summary>
        /// Deletes the Clustering layer from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["Clustering"]);
        }
    }
}
