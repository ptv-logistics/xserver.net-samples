using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace MemoryDemo
{
    public partial class MapForm : Form
    {
        /// <summary>
        /// The owner form
        /// </summary>
        private ApplicationForm owner;

        /// <summary>
        /// The layer to add our objects
        /// </summary>
        private ShapeLayer layer;

        public MapForm(ApplicationForm owner)
        {
            // set the owner form
            this.owner = owner;

            InitializeComponent();

            // initialize the map
            formsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            formsMap1.XMapCredentials = "xtok:9358789A-A8CF-4CA8-AC99-1C0C4AC07F1E";

            // This event-listener will create a reference from the map-control to the form.
            // So if the objects of the map aren't freed properly, the form will also leak memory.
            formsMap1.ViewportEndChanged += formsMap1_ViewportEndChanged;

            // go to Karlsruhe
            formsMap1.SetMapLocation(new Point(8.5, 49), 8);

            // add a new Shape Layer
            layer = new ShapeLayer("MyShapes");
            formsMap1.Layers.Add(layer);
           
            // this will create a memory leak if not properly deatch on finalization!
            owner.AddTruckButton.Click += button2_Click;
        }

        void button2_Click(object sender, EventArgs e)
        {
            // create a random geo-coordinate
            var rand = new Random();
            var lat = rand.NextDouble() + 48.5;
            var lon = rand.NextDouble() + 8;

            // create  a truck marker
            var marker = new Truck
            {
                Color = Color.FromRgb((byte)(rand.NextDouble() * 256), (byte)(rand.NextDouble() * 256), (byte)(rand.NextDouble() * 256)),
                Width = 20 + rand.NextDouble() * 20,
                ToolTip = "Hello Map"
            };

            // set position and add to map
            ShapeCanvas.SetLocation(marker, new Point(lon, lat));
            layer.Shapes.Add(marker);

            // set the location as center
            formsMap1.SetMapLocation(new Point(lon, lat), 10);
        }

        public void FixMemoryLeak()
        {
            // Detaching the event will free the references to the dialog from long-living owner-object.
            owner.AddTruckButton.Click -= button2_Click;
        }

        void formsMap1_ViewportEndChanged(object sender, EventArgs e)
        {
        }
    }
}
