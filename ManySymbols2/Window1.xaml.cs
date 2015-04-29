using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Printing;
using System.IO;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Tools;
using Ptv.XServer.Demo.Clustering;
using System.Globalization;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Canvases;

namespace ManySymbols
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

            this.Map.SetMapLocation(new Point(8.4, 49), 10);

        }

        /// <summary> Reads the earthquakes from a csv file and adds them to a clusterer. The clusterer then groups
        /// them to clusters. </summary>
        /// <returns> Documentation in progress... </returns>
        public static IEnumerable<Location> ReadCSVFile()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + @"wikilocations25000.csv";
            using (var reader = new CsvFileReader(filePath, (char)0x09))
            {
                var row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    if (row.Count < 3)
                        continue;

                    double x, y;
                    bool parsed = Double.TryParse(row[2], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                    x = parsed ? x : Double.NaN;
                    parsed = Double.TryParse(row[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                    y = parsed ? y : Double.NaN;
                    var post = new Location { Title = row[0], Coordinate = new Point(x, y) };
                    yield return post;
                }
            }
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            // read data - list of locations
            var dataSource = ReadCSVFile().ToList();

            // define some style (i.e. symbol type dependant on location
            Func<Location, Symbol> style = location =>
            {
                // use different colors
                var color = (location.Title.Length < 10) ? Colors.Red : (location.Title.Length < 20) ? Colors.Green : Colors.Blue;
                return new Symbol {Type = 1, Color = color, Size = 16, Tooltip = location.Title};
            };

            // create a clustering layer.
            var symbolLayer = new BaseLayer("Symbols")
            {
                CanvasCategories = new[] {CanvasCategory.Content},
                CanvasFactories =
                    new BaseLayer.CanvasFactoryDelegate[]
                    {map => (map.Name == "Map") ? new LocationCanvas(map){dataSource = dataSource, MapStyle = style, MinZoom = 6} : null} // only add at main map view
            };

            // add to map
            Map.Layers.Add(symbolLayer);
        }
    }
}
