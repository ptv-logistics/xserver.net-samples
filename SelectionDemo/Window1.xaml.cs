using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using SilverMap.UseCases.SharpMap;

namespace LogoDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            Map.Loaded += Map_Loaded;
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            var myLayer = new ShapeLayer("MyLayer");
            Map.Layers.Add(myLayer);

            // get the map view containing the content
            var mapView = Ptv.XServer.Controls.Map.Tools.MapElementExtensions.FindChild<MapView>(Map);
            
            // add interactor to the map view for the shape collection
            selectInteractor = new SelectInteractor(mapView, myLayer.Shapes);

            // if the collection changes display it
            selectInteractor.SelectedElements.CollectionChanged += SelectedElements_CollectionChanged;

            AddShapes(myLayer);
        }

        private SelectInteractor selectInteractor;

        private void SelectedElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            listView.Items.Clear();
            foreach (var x in selectInteractor.SelectedElements)
            {
                listView.Items.Add(x.Name);
            }
        }

        /// <summary>
        /// Load local image files and add it to the map
        /// See also http://www.i-programmer.info/programming/wpf-workings/520-bitmapimage-and-local-files-.html
        /// </summary>
        /// <param name="layer"></param>
        public void AddShapes(ShapeLayer layer)
        {
            // add ptv logo
            var ptvLogo = new Image();
            var src = new BitmapImage();
            src.BeginInit();
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.UriSource = new Uri("Icons\\Logo_PTV_Screen-mini.tif", UriKind.Relative);
            src.EndInit();
            ptvLogo.Source = src;
            ShapeCanvas.SetLocation(ptvLogo, new Point(8.428253, 49.013432));
            ptvLogo.Name = "PTV";
 
            // optional: set explicit size and scale factor
            ptvLogo.Height = 100;
            ShapeCanvas.SetScaleFactor(ptvLogo, .1);

            layer.Shapes.Add(ptvLogo);

            // add cas logo
            var casLogo = new Image();
            src = new BitmapImage();
            src.BeginInit();
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.UriSource = new Uri("Icons\\logoCAS-Software-AG-DE.gif", UriKind.Relative);
            src.EndInit();
            casLogo.Source = src;
            casLogo.Name = "CAS";
            ShapeCanvas.SetLocation(casLogo, new Point(8.439220, 49.021664));

            // optional: set explicit size and scale factor
            casLogo.Height = 100;
            ShapeCanvas.SetScaleFactor(casLogo, .1);

            layer.Shapes.Add(casLogo);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            selectInteractor.SelectedElements.Clear();
        }
    }
}
