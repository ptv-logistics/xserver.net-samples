using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;
using System.Windows;
using System.Windows.Controls;

namespace CustomPanAndZoom
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        PanAndZoom customPanAndZoom = new CustomPanAndZoom.PanAndZoom();
        public Window1()
        {
            InitializeComponent();

            // get the map container grid
            var grid = MapElementExtensions.FindChild<Grid>(Map);
            // get the old interactor
            var pz = Map.FindRelative<Ptv.XServer.Controls.Map.Gadgets.PanAndZoom>();
            // exchenage the interactor
            grid.Children.Remove(pz);
            grid.Children.Add(customPanAndZoom);

            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        void Map_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Selection_Changed(object sender, RoutedEventArgs e)
        {
            switch (dragMode.SelectedIndex)
            {
                case 0:
                    customPanAndZoom.MouseDragMode = DragMode.SelectOnShift;
                    break;
                case 1:
                    customPanAndZoom.MouseDragMode = DragMode.Pan;
                    break;
                case 2:
                    customPanAndZoom.MouseDragMode = DragMode.Select;
                    break;
                case 3:
                    customPanAndZoom.MouseDragMode = DragMode.None;
                    break;
            }
        }

        private void DoubleClickSelect_OnChecked(object sender, RoutedEventArgs e)
        {
            this.customPanAndZoom.ZoomOnDoubleClick = doubleClickSelect.IsChecked?? false;
        }
    }
}
