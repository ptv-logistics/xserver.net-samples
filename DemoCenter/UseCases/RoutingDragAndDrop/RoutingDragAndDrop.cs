// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Controls.Routing;

namespace Ptv.XServer.Demo.UseCases.RoutingDragAndDrop
{
    class RoutingDragAndDropUseCase : UseCase
    {
        protected override void Enable()
        {
            wpfMap.Layers.Add(new RouteLayer(wpfMap));
        }

        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["Route"]);
        }
    }
}
