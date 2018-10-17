// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.ShapeFile;

namespace Ptv.XServer.Demo.UseCases.ShapeFile
{
    /// <summary>
    /// Provides the functionality to add a ShapeFile Layer to a WpfMap.
    /// </summary>
    public class ShapeFileUseCase : UseCase
    {
        /// <summary>
        /// Tries to create a ShapeFile layer with data from a Shapefile to the map.
        /// </summary>
        protected override void Enable()
        {
            if (wpfMap.Layers.Contains(wpfMap.Layers["ShapeFile"]))
                return;
            
            #region doc:load icon
            var icon = ResourceHelper.LoadBitmapFromResource("PTV xServer .NET Demo Center;component/Resources/SharpMapLogo.png");
            #endregion //doc:load icon

            #region doc:locate the shape file
            var shapeFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\ShapeData") +
                @"\world_countries_boundary_file_world_2002.shp";
            #endregion //doc:locate the shape file

            #region doc:insert the new layer
            int idx = wpfMap.Layers.IndexOf(wpfMap.Layers["Background"]) + 1;

            wpfMap.AddShapeLayer("ShapeFile", shapeFilePath, idx, true, .8, icon);
            #endregion //doc:insert the new layer

            #region doc:zoom to a specific location
            // zoom to Karlsruhe
            wpfMap.SetMapLocation(new Point(8.4, 49), 5);
            #endregion //doc:zoom to a specific location
        }

        /// <summary>
        /// Deletes the layer of the ShapeFile from the WpfMap.
        /// </summary>
        protected override void Disable()
        {
            wpfMap.RemoveShapeLayer("ShapeFile");
        }
    }
}
