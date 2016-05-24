using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Demo.MapMarket;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Windows;

namespace Donuts
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCustomLayer();
        }

        // a return a brush depending on the item
        public System.Drawing.Brush GetBrush(GeoItem item)
        {
            var colors = new[] { System.Drawing.Color.Blue, System.Drawing.Color.Green, System.Drawing.Color.Red };

            return new HatchBrush(HatchStyle.DiagonalCross, colors[((int)item.Id) % colors.Length], System.Drawing.Color.White);
        }

        TileRenderer tileRenderer;
        BaseLayer layer;
        DonutProvider provider;

        public void InitializeCustomLayer()
        {
             provider = new DonutProvider();

            // the theme maps buying power to colors
            var theme = new GdiTheme
            {
                Mapping = geoItem => new GdiStyle
                { 
                    Fill = GetBrush(geoItem),
                    Outline = System.Drawing.Pens.Black,
                }
            };

            // the renderer for the polygon tiles
            tileRenderer = new TileRenderer
            {
                Provider = provider,
                Theme = theme,
                CacheId = "Donuts" + Guid.NewGuid()
            };

            // the collection of selected elements
            var selectedRegions = new ObservableCollection<System.Windows.Media.Geometry>();

            // insert layer with two canvases
            layer = new BaseLayer("CustomShapes")
            {
                CanvasCategories = new[] { CanvasCategory.Content, CanvasCategory.SelectedObjects },
                CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]
                {
                    m => new TiledCanvas(m, tileRenderer) { IsTransparentLayer = true },
                    m => new SelectionCanvas(m, provider, selectedRegions)
                },
                Opacity = .8,
            };

            Map.Layers.InsertBefore(layer, "Labels");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            tileRenderer.CacheId = "Donuts" + Guid.NewGuid();
            provider.UpdateShapes();
            layer.Refresh();
        }
    }
}
