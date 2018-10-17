// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System.Windows;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Demo.UseCases;


namespace Ptv.XServer.Demo.MapMarket
{
    /// <summary> <para>The controller class for the Map&amp;Market use case.</para>
    /// <para>See the <conceptualLink target="d705537f-f7fe-435c-bd80-d4d399ee4410"/> topic for an example.</para> </summary>
    class MapMarketController : UseCase
    {
        /// <summary> Adds the layer to the layer collection. </summary>
        protected override void Enable()
        {
            string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // the data is located in an MS Access database
            // note: appication has to run as x64 for JET DBs!
            var connection = new OleDbConnection($@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={baseDir}\Resources\Districts.mdb");
            connection.Open();

            // the provider for map&market regions 
            var provider = new MMProvider
            {
                Connection = connection,
                Table = "KRE",
                IdColumn = "GID",
                GeometryColumn = "WKB_GEOMETRY",
                XMinColumn = "XMIN",
                YMinColumn = "YMIN",
                XMaxColumn = "XMAX",
                YMaxColumn = "YMAX",
            };

            // the palette for the choropleth
            System.Drawing.Color[] palette =
            {
                System.Drawing.Color.Green,
                System.Drawing.Color.LightGreen,
                System.Drawing.Color.Yellow,                               
                System.Drawing.Color.Orange,                                
                System.Drawing.Color.Red,                                
                System.Drawing.Color.DarkRed,                                
                System.Drawing.Color.Purple
            };

            // the theme maps buying power to colors
            var theme = new GdiTheme
            {
                RequiredFields = new[] { "KK_KAT" }, // need to fetch the field kk_kat 
                Mapping = geoItem => new GdiStyle
                {
                    Fill = new System.Drawing.SolidBrush(
                        palette[System.Convert.ToInt16(geoItem.Atributes["KK_KAT"]) - 1]),
                    Outline = System.Drawing.Pens.Black,
                }
            };

            // the renderer for the polygon tiles
            var tileRenderer = new TileRenderer
            {
                Provider = provider,
                Theme = theme,
            };

            // the collection of selected elements
            var selectedRegions = new ObservableCollection<System.Windows.Media.Geometry>();

            // insert layer with two canvases
            var layer = new BaseLayer("MapMarket")
            {
                CanvasCategories = new[] { CanvasCategory.Content, CanvasCategory.SelectedObjects },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                {
                    m => new TiledCanvas(m, tileRenderer) { IsTransparentLayer = true },
                    m => new SelectionCanvas(m, provider, selectedRegions)
                },
                Opacity = .8,
            };

            // insert before label layer, if existent
            if (wpfMap.Layers["Labels"] != null)
                wpfMap.Layers.Insert(wpfMap.Layers.IndexOf(wpfMap.Layers["Labels"]), layer);
            else
                wpfMap.Layers.Add(layer);

            // Set Germany as center
            wpfMap.SetMapLocation(new Point(10.5, 51.5), 6);
        }

        /// <summary> Removes the layer from the layer collection. </summary>
        protected override void Disable()
        {
            wpfMap.Layers.Remove(wpfMap.Layers["MapMarket"]);
        }
    }
}
