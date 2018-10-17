// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.CoordinateSystems;
using SharpMap.Layers;
using System.IO;
using SharpMap.Rendering.Thematics;
using SharpMap.Geometries;
using System.Drawing.Imaging;
using SharpMap.Styles;
using System.Globalization;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Canvases;

namespace Ptv.XServer.Demo.ShapeFile
{
    /// <summary> <para>Contains extension methods for adding a layer which works with shape files to the map.</para>
    /// <para>See the <conceptualLink target="427ab62e-f02d-4e92-9c26-31e0f89d49c5"/> topic for an example.</para> </summary>
    public static class SharpMapExtensions
    {
        /// <summary> <para>Adds a shape layer with selection logic to the map. The shapes are managed by SharpMap.</para>
        /// <para>See the <conceptualLink target="427ab62e-f02d-4e92-9c26-31e0f89d49c5"/> topic for an example.</para> </summary>
        /// <param name="wpfMap">The map to add the layer to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="shapeFilePath">The full qualified path to the shape file.</param>
        /// <param name="idx">The index where to add the layer in the hierarchy.</param>
        /// <param name="isBaseMapLayer">Specifies if the layer is a base map layer.</param>
        /// <param name="opacity">The initial opacity.</param>
        /// <param name="icon">The icon used in the layers control.</param>
        #region doc:AddShapeLayer method
        public static void AddShapeLayer(this WpfMap wpfMap, string name, string shapeFilePath, int idx, bool isBaseMapLayer, double opacity, System.Windows.Media.Imaging.BitmapImage icon)
        {
            // the collection of selected elements
            var selectedRegions = new System.Collections.ObjectModel.ObservableCollection<System.Windows.Media.Geometry>();

            // add a layer which uses SharpMap (by using SharpMapTiledProvider and ShapeSelectionCanvas)
            #region doc:create TiledLayer
            var sharpMapLayer = new TiledLayer(name)
            {
                // Create canvas categories. First one for the ordinary tile content. Second one for the selection canvas.
                CanvasCategories = new[] { CanvasCategory.Content, CanvasCategory.SelectedObjects },
                // Create delegates for the content canvas and the selection canvas.
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                    {
                        m => new TiledCanvas(m, new SharpMapTiledProvider(shapeFilePath)) { IsTransparentLayer = true },
                        m => new ShapeSelectionCanvas(m, shapeFilePath, selectedRegions)
                    },
                // Set some more initial values...
                Opacity = opacity,
                IsBaseMapLayer = isBaseMapLayer,
                Icon = icon
            };
            #endregion // doc:create TiledLayer

            wpfMap.Layers.Insert(idx, sharpMapLayer);
        }
        #endregion //doc:AddShapeLayer method

        /// <summary>
        /// Extension method which removes a shape layer from the map.
        /// </summary>
        /// <param name="wpfMap">The map to remove the layer from.</param>
        /// <param name="name">The name of the layer to be removed.</param>
        public static void RemoveShapeLayer(this WpfMap wpfMap, string name)
        {
            wpfMap.Layers.Remove(wpfMap.Layers[name]);
        }
    }

    /// <summary> <para>Demonstrates how SharpMap can be used to implement a layer which uses shape files as data source.</para>
    /// <para>See the <conceptualLink target="427ab62e-f02d-4e92-9c26-31e0f89d49c5"/> topic for an example.</para> </summary>
    public class SharpMapTiledProvider : ITiledProvider
    {
        /// <summary> Set earth radius according PTV, but the radius doesn't matter for tiles you could also use the
        /// Bing/Google radius 6378137 or any arbitrary value. </summary>
        public const double EarthRadius = 6371000.0;

        #region static methods
        /// <summary> Calculates a Mercator bounding box for a tile key. </summary>
        /// <param name="tileX"> The tile x coordinate in PTV-internal format. </param>
        /// <param name="tileY"> The tile y coordinate in PTV-internal format. </param>
        /// <param name="zoom"> The zoom level. </param>
        /// <returns> A bounding box in Mercator format which corresponds to the given tile coordinates and zoom level. </returns>
        public static BoundingBox TileToMercatorAtZoom(int tileX, int tileY, int zoom)
        {
            const double earthCircum = EarthRadius * 2.0 * Math.PI;
            const double earthHalfCircum = earthCircum / 2;
            double arc = earthCircum / (1 << zoom);

            return new BoundingBox(
                (tileX * arc) - earthHalfCircum, earthHalfCircum - ((tileY + 1) * arc),
                ((tileX + 1) * arc) - earthHalfCircum, earthHalfCircum - (tileY * arc));
        }

        /// <summary>
        /// Creates a transformer which transforms coordinates from the given source coordinate system to Mercator.
        /// </summary>
        /// <param name="source">The source coordinate system.</param>
        /// <returns>The coordinate transformer.</returns>
        public static ICoordinateTransformation TransformToMercator(ICoordinateSystem source)
        {
            var csFactory = new CoordinateSystemFactory();

            var parameters = new List<ProjectionParameter>
            { 
                new ProjectionParameter("latitude_of_origin", 0), new ProjectionParameter("central_meridian", 0),
                new ProjectionParameter("false_easting", 0), new ProjectionParameter("false_northing", 0),
                new ProjectionParameter("semi_major", EarthRadius), new ProjectionParameter("semi_minor", EarthRadius)
            };

            var projection = csFactory.CreateProjection("Mercator", "Mercator_2SP", parameters);

            var coordSystem = csFactory.CreateProjectedCoordinateSystem(
                "Mercator", source as IGeographicCoordinateSystem, projection, LinearUnit.Metre,
                 new AxisInfo("East", AxisOrientationEnum.East),
                 new AxisInfo("North", AxisOrientationEnum.North));

            return new CoordinateTransformationFactory().CreateFromCoordinateSystems(source, coordSystem);
        }
        #endregion

        /// <summary>The data source containing the shape files.</summary>
        private readonly string shapeFile;

        /// <summary> Initializes a new instance of the <see cref="SharpMapTiledProvider"/> class. The SharpMap tiled
        /// provider reads the shapes from the given shape file path. </summary>
        /// <param name="shapeFile"> The full qualified path to the shape file. </param>
        public SharpMapTiledProvider(string shapeFile)
        {
            this.shapeFile = shapeFile;
        }

        #region doc:GetPopDensStyle method
        /// <summary> Demonstrates the use of dynamic styles (themes) for vector layers. In this case we 
        /// use the population density to color the shape to draw.</summary>
        /// <param name="pixelSize"> The pixel size of the map. </param>
        /// <param name="row"> The currently processed data row. </param>
        /// <returns> A VectorStyle which is used to style the shape to draw. </returns>
        private VectorStyle GetPopDensStyle(double pixelSize, DataRow row)
        {
            float scale;
            
            try
            {
                // colorize the polygon according to buying power;
                var pop = Convert.ToDouble(row["POP2005"], NumberFormatInfo.InvariantInfo);
                var area = Convert.ToDouble(row["AREA"], NumberFormatInfo.InvariantInfo);
                // compute a scale [0..1] for the population density
                scale = (float)((area > 0) ? Math.Min(1.0, Math.Sqrt(pop / area) / 70) : -1.0f);
            }
            catch (Exception)
            {
                scale = -1.0f;
            }

            var fillColor = scale < 0 ? Color.Gray : ColorBlend.ThreeColors(Color.Green, Color.Yellow, Color.Red).GetColor(scale);

            // make fill color alpha-transparent
            fillColor = Color.FromArgb(180, fillColor.R, fillColor.G, fillColor.B);

            // set the border width depending on the map scale
            var pen = new Pen(Brushes.Black, (int)(50.0 / pixelSize)) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };

            return new VectorStyle { Outline = pen, EnableOutline = true, Fill = new SolidBrush(fillColor) };
        }
        #endregion //doc:GetPopDensStyle method

        #region Implementation of ITiledProvider
        #region doc:GetImageStream method
        /// <inheritdoc/>
        public Stream GetImageStream(int x, int y, int zoom)
        {
            
            // create a transparent sharpmap map with a size of 256x256
            using (var sharpMap = new SharpMap.Map(new Size(256, 256)) { BackColor = Color.Transparent })
            {
                // the map contains only one layer
                var countries = new VectorLayer("WorldCountries")
                {
                    // set tranform to WGS84->Spherical_Mercator
                    CoordinateTransformation = TransformToMercator(GeographicCoordinateSystem.WGS84),

                    // set the sharpmap provider for shape files as data source
                    DataSource = new SharpMap.Data.Providers.ShapeFile(shapeFile),

                    // use a dynamic style for thematic mapping
                    // the lambda also takes the map instance into account (to scale the border width)
                    Theme = new CustomTheme(row => GetPopDensStyle(sharpMap.PixelSize, row))
                };

                // add the layer to the map
                sharpMap.Layers.Add(countries);

                // calculate the bbox for the tile key and zoom the map 
                sharpMap.ZoomToBox(TileToMercatorAtZoom(x, y, zoom));

                // render the map image
                using (var img = sharpMap.GetMap())
                {
                    // stream the image to the client
                    var memoryStream = new MemoryStream();
                            
                    // Saving a PNG image requires a seekable stream, first save to memory stream 
                    // http://forums.asp.net/p/975883/3646110.aspx#1291641
                    img.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }
        }
        #endregion // doc:GetImageStream method

        /// <inheritdoc/>
        public string CacheId => "SharpMapTiledProvider";

        /// <inheritdoc/>
        public int MaxZoom => 19;

        /// <inheritdoc/>
        public int MinZoom => 0;

        #endregion
    }
}
