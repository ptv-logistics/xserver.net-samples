// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Demo.Tools;

namespace Ptv.XServer.Demo.UseCases.ShapeFile
{
    /// <summary>
    /// Provides the functionality to add a Data Manager Layer to a WPF map.
    /// </summary>
    public class DataManagerUseCase : UseCase
    {
        /// <summary>
        /// Tries to create a Data Manager layer to the map.
        /// </summary>
        protected override void Enable()
        {
            if (wpfMap.Layers.Contains(wpfMap.Layers["POS"]) || !ManagedAuthentication.IsOk)
                return;

            wpfMap.InsertDataManagerLayer(ManagedAuthentication.XMapMetaInfo, "POS", "t_f07ef3f0_ce7a_4913_90ea_b072ec07e6ff", "Points Of Sales", 10, true);

            wpfMap.SetMapLocation(new System.Windows.Point(8.4, 49), 12);
        }

        /// <summary>
        /// Deletes the layer of the ShapeFile from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["POS"]);
        }
    }
}
