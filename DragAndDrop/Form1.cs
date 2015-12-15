using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using Point = System.Windows.Point;

namespace FormsMapCS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // our test data
            var vehicles = new Vehicle[]
            {
                new Vehicle {Longitude = 8.3, Latitude = 49, Id = "1"},
                new Vehicle {Longitude = 8.4, Latitude = 49.1, Id = "1"},
                new Vehicle {Longitude = 8.5, Latitude = 49, Id = "1"}
            };

            // the drag&drop events for the grid
            vehicleDataGridView.DragEnter += vehicleDataGridView_DragOver;
            vehicleDataGridView.DragOver += vehicleDataGridView_DragOver;
            vehicleDataGridView.DragDrop += vehicleDataGridView_DragDrop;
            vehicleDataGridView.CellMouseDown += vehicleDataGridView_CellMouseDown;

            // bind the grid to our data
            vehicleBindingSource.DataSource = vehicles;

            // initialize base map (for xServer internet)
            formsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            formsMap1.XMapCredentials = "xtok:870241687284044";

            // go to Karlsruhe
            formsMap1.SetMapLocation(new Point(8.4, 49.05), 10);

            // add a new Shape Layer
            var layer = new ShapeLayer("MyShapes");
            formsMap1.Layers.Add(layer);

            // add items for all elements
            foreach (var vehicle in vehicles)
            {
                // create  a truck marker
                var marker = new Truck
                {
                    Color = Colors.Brown,
                    Width = 50,
                    ToolTip = "Hello Map",
                    AllowDrop = true
                };

                // events for drag&drop
                marker.MouseDown += Marker_MouseDown;
                marker.DragEnter += marker_DragOver;
                marker.DragOver += marker_DragOver;
                marker.Drop += marker_Drop;
                
                // set the vehicle data as tag
                marker.Tag = vehicle;

                // set position and add to map
                ShapeCanvas.SetLocation(marker, new Point(vehicle.Longitude, vehicle.Latitude));
                layer.Shapes.Add(marker);
            }
        }

        void vehicleDataGridView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Vehicle)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // the drop action goes here    
        }

        void marker_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Vehicle)))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            // the drop action goes here        
        }

        void vehicleDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            vehicleDataGridView.DoDragDrop(vehicleDataGridView.Rows[e.RowIndex].DataBoundItem,  DragDropEffects.Link);
        }

        void marker_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof (Vehicle)))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }

            var vehicle = e.Data.GetData(typeof(Vehicle));

            // now check if you're allowed to drop here
            e.Effects = System.Windows.DragDropEffects.Link;    
        }

        void vehicleDataGridView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Vehicle)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var vehicle = e.Data.GetData(typeof(Vehicle));

            // now check if you're allowed to drop here
            e.Effect = DragDropEffects.Link;
        }

        private void Marker_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DoDragDrop((sender as FrameworkElement).Tag, System.Windows.Forms.DragDropEffects.Link);
        }
    }

    [Serializable]
    public class Vehicle
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Id { get; set; }
    }
}
