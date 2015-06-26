using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System;
using System.Windows.Forms;
using System.Windows.Media;
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
            // initialize base map (for xServer internet)
            formsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            formsMap1.XMapCredentials = "xtok:576140558287692";

            // go to Karlsruhe
            formsMap1.SetMapLocation(new Point(8.4, 49), 16);
            
            // add a new Shape Layer
            var layer = new ShapeLayer("MyShapes");
            formsMap1.Layers.Add(layer);

            // create  a truck marker
            var marker = new Truck
            {
                Color = Colors.Brown,
                Width = 50,
                ToolTip = "Hello Map"
            };
            
            // set position and add to map
            ShapeCanvas.SetLocation(marker, new Point(8.4, 49));
            layer.Shapes.Add(marker);
        }
    }
}
