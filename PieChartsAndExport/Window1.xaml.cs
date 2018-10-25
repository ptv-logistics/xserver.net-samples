using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using System.Windows.Controls.DataVisualization.Charting;
using Ptv.XServer.Controls.Map;
using System.IO;

namespace PieCharts
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            AddPieCharts(myLayer);
        }

        // Shows how to add an arbitrary controls to the map
        // As sample the pie charts series of Wpf toolkit is used
        // http://wpf.codeplex.com/releases/view/40535
        public void AddPieCharts(ShapeLayer layer)
        {
            // our demo data
            var stores = new List<Store>{
            new Store
            {
                Name = "KA-Center",
                Latitude = 48.96,
                Longitude = 8.39,
                Sales = new List<Sale>{
                    new Sale{Type = "Food", Amount= 30}, 
                    new Sale { Type = "Non Food", Amount = 70 }}              
            },
            new Store
            {
                Name = "KA-North",
                Latitude = 49.04,
                Longitude = 8.41, 
                Sales = new List<Sale>{
                    new Sale{Type = "Food", Amount = 40},
                    new Sale { Type = "Non Food", Amount = 50 },
                    new Sale { Type = "Pet Food", Amount = 10 }}                
            }};

            foreach (var store in stores)
            {
                // initialize a pie chart for each element
                var chart = new Chart();
                chart.BeginInit();

                chart.Width = 300;
                chart.Height = 250;
                chart.Background = new SolidColorBrush(Color.FromArgb(192, 255, 255, 255));
                var pieSeries = new PieSeries();
                chart.Title = store.Name;
                pieSeries.IndependentValuePath = "Type";
                pieSeries.DependentValuePath = "Amount";
                pieSeries.ItemsSource = store.Sales;
                pieSeries.IsSelectionEnabled = true;
                chart.Series.Add(pieSeries);

                chart.EndInit();

                // Add to map
                ShapeCanvas.SetLocation(chart, new Point(store.Longitude, store.Latitude));
                ShapeCanvas.SetAnchor(chart, LocationAnchor.Center);
                ShapeCanvas.SetScale(chart, 2);
                ShapeCanvas.SetScaleFactor(chart, .25); // adopt the element to the scale factor
                layer.Shapes.Add(chart);
            }
        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);
            PrintMap(mapView, false, "Test");  
        }

        private void printScaling_Click(object sender, RoutedEventArgs e)
        {
            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);
            PrintMap(mapView, true, "Test");  
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);

            // export the map content
            var bytes = ExportMap(mapView, new Size(600, 400), true);
            
            // get unique file name
            var fileName = Path.GetTempPath() + "\\Img" + Guid.NewGuid() + ".jpg";

            // save to disk
            File.WriteAllBytes(fileName, bytes);

            // open the image
            System.Diagnostics.Process.Start(fileName);
        }

        private void exportCustom_Click(object sender, RoutedEventArgs e)
        {
            // export width/height
            const int width = 600;
            const int height = 400;

            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);

            // export the map content
            var bytes = ExportMap(mapView, new Size(width, height), true);

            // get unique file name
            var fileName = Path.GetTempPath() + "\\Img" + Guid.NewGuid() + ".jpg";

            // create a gdi image and graphics from the bitmap
            using(var image = System.Drawing.Image.FromStream(new MemoryStream(bytes)))
            using (var graphics = System.Drawing.Graphics.FromImage(image))
            {
                // put a custom title on the image
                using (var font = new System.Drawing.Font("Arial", 16))
                {
                    const string text = "Hello!\nThis is a custom title.";
                    var textSize = graphics.MeasureString(text, font);

                    // draw a box @top/center with border 4 px
                    const int borderSize = 4;
                    graphics.FillRectangle(System.Drawing.Brushes.White, width / 2 - textSize.Width / 2 - borderSize, 0, textSize.Width + 8, textSize.Height + 8);
                    graphics.DrawRectangle(System.Drawing.Pens.Black, width / 2 - textSize.Width / 2 - 4, 0, textSize.Width + 8, textSize.Height + 8);

                    // draw text
                    graphics.DrawString(text, font, System.Drawing.Brushes.Black, new System.Drawing.PointF(width / 2 - textSize.Width / 2, 4));
                }

                // save image to disk
                image.Save(fileName);
            }

            // open the image
            System.Diagnostics.Process.Start(fileName);
        }

        /// <summary>
        /// Exports a map to a byte array
        /// </summary>
        /// <param name="mapView">the map view containing the content</param>
        /// <param name="sz">the size of the output image</param>
        /// <param name="useScaling">true, if the contend should be scaled to fit the image size:
        /// false, if the map scale shouldn't be changed.</param>
        /// <returns>The (jpg) image as byte array</returns>
        public byte[] ExportMap(MapView mapView, Size sz, bool useScaling)
        {
            Transform oldTransform = null;

            if (useScaling)
            {
                // Set the transform object for scaling.
                double scale = Math.Min(sz.Width / mapView.ActualWidth, sz.Height / mapView.ActualHeight);
                oldTransform = mapView.LayoutTransform;
                mapView.LayoutTransform = new ScaleTransform(scale, scale);
            }

            // Set the size.
            var oldSize = new Size(mapView.ActualWidth, mapView.ActualHeight);
            mapView.Measure(sz);
            mapView.Arrange(new Rect(new Point(0, 0), sz));

            // Print.
            mapView.Printing = true;
            mapView.UpdateLayout();

            var renderTargetBitmap = new RenderTargetBitmap((int)sz.Width, (int)sz.Height, 96d, 96d, PixelFormats.Default);
            renderTargetBitmap.Render(mapView);

            mapView.Printing = false;

            // Reset the old values.
            if (useScaling)
                mapView.LayoutTransform = oldTransform;
            mapView.Measure(oldSize);
            mapView.Arrange(new Rect(new Point(0, 0), oldSize));

            // encode image
            var jpegBitmapEncoder = new JpegBitmapEncoder();
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            var ms = new MemoryStream();
            jpegBitmapEncoder.Save(ms);
            ms.Close();
            return ms.ToArray();
        }


        /// <summary>
        /// Default print method of the map control,
        /// Exports a map to a byte array
        /// </summary>
        /// <param name="mapView">the map view containing the content</param>
        /// <param name="useScaling">true, if the contend should be scaled to fit the image size:
        /// false, if the map scale shouldn't be changed.</param>
        /// <param name="description"></param>
        public void PrintMap(MapView mapView, bool useScaling, string description)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == false)
                return;

            // Initialize variables.
            var printCapabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            Transform oldTransform = null;

            if (useScaling)
            {
                // Set the transform object for scaling.
                double scale = Math.Min(printCapabilities.PageImageableArea.ExtentWidth / mapView.ActualWidth,
                                               printCapabilities.PageImageableArea.ExtentHeight / mapView.ActualHeight);
                oldTransform = mapView.LayoutTransform;
                mapView.LayoutTransform = new ScaleTransform(scale, scale);
            }

            // Set the size.
            var oldSize = new Size(mapView.ActualWidth, mapView.ActualHeight);
            var size = new Size(printCapabilities.PageImageableArea.ExtentWidth, printCapabilities.PageImageableArea.ExtentHeight);
            mapView.Measure(size);
            mapView.Arrange(new Rect(new Point(printCapabilities.PageImageableArea.OriginWidth, printCapabilities.PageImageableArea.OriginHeight), size));

            // Print.
            mapView.Printing = true;
            mapView.UpdateLayout();
            printDialog.PrintVisual(mapView, description);
            mapView.Printing = false;

            // Reset the old values.
            if (useScaling)
                mapView.LayoutTransform = oldTransform;
            mapView.Measure(oldSize);
            mapView.Arrange(new Rect(new Point(0, 0), oldSize));
        }
    }

    public class Sale
    {
        public string Type { get; set; }
        public int Amount { get; set; }
    }

    public class Store
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<Sale> Sales { get; set; }
    }
}
