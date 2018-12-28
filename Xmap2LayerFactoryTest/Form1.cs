using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Layers.Xmap2;
using Ptv.XServer.Controls.Map.TileProviders;

namespace XMap2FactoryTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var center = new Point(8.4044, 49.01405);
            formsMap.SetMapLocation(center, 15);
            formsMap.MaxZoom = 22;

            formsMap.XMapUrl = "xserver2-europe-eu-test;version=2.8";
            formsMap.XMapCredentials = "Insert your xToken here";

            InitializeXMap2();

            formsMap.Layers.InsertBefore(CreateShapeLayer(center), "Labels"); // Custom shape layer in-between background and labels

            formsMap.ToolTipManagement.CreateCustomizedToolTipsFunc = CreateOwnToolTip;
            formsMap.ToolTipManagement.DestroyCustomizedToolTipsFunc = DestroyOwnToolTip;
        }

        private void InitializeXMap2()
        {
            DemonstrationHowToModifyXServerRequestBeforeItIsSent();

            DemonstrationHowToGetDetailedInformationOfXServerException();

            var layerFactory = formsMap.Xmap2LayerFactory;
            if (layerFactory == null)
                return; // An invalid xServer2-URL was specified, the following XServer2 preparations cannot be executed.

            PrepareMapStyles(layerFactory);
            PrepareLanguages(layerFactory);

            new FeatureLayerSetter(this);
        }

        private void DemonstrationHowToModifyXServerRequestBeforeItIsSent()
        {
            LayerFactory.ModifyRequest = request =>
            {
                request.Headers["Dummy"] = "Test";
                AppendTextBox(request.RequestUri.AbsoluteUri);
                AppendTextBox(request.Headers.ToString());
                return request;
            };
        }

        private void DemonstrationHowToGetDetailedInformationOfXServerException()
        {
            LayerFactory.ReportXServerError = exception =>
            {
                using (var stream = exception.Response.GetResponseStream())
                    if (stream != null)
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            AppendTextBox(reader.ReadToEnd());
            };
        }

        private void PrepareMapStyles(LayerFactory layerFactory)
        {
            foreach (var mapStyle in layerFactory.AvailableMapStyles)
                if (mapStyle.Equals("default")) // if "default" is available then insert it at first position.
                    mapStylesComboBox.Items.Insert(0, mapStyle);
                else
                    mapStylesComboBox.Items.Add(mapStyle);

            mapStylesComboBox.SelectedIndex = Math.Min(0, mapStylesComboBox.Items.Count - 1);

            mapStylesComboBox.SelectedIndexChanged += (_, __) => layerFactory.MapStyle = mapStylesComboBox.Text;
            layerFactory.MapStyle = mapStylesComboBox.Text;
        }

        private void PrepareLanguages(LayerFactory layerFactory)
        {
            mapLanguageTextBox.Leave += (_, __) => layerFactory.MapLanguage = mapLanguageTextBox.Text;
            layerFactory.MapLanguage = mapLanguageTextBox.Text;

            trafficIncidentsLanguageTextBox.Leave += (_, __) => layerFactory.UserLanguage = trafficIncidentsLanguageTextBox.Text;
            layerFactory.UserLanguage = trafficIncidentsLanguageTextBox.Text;
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
                BeginInvoke(new Action<string>(AppendTextBox), value);
            else
                requestLoggingTextBox.AppendText(value);
        }

        private void CreateOwnToolTip(List<IMapObject> toolTipMapObjects)
        {
            var stackPanel = new StackPanel();

            foreach (var item in toolTipMapObjects.Select((toolTipMapObject, index) => new { index, toolTipMapObject }))
            {
                var label = new System.Windows.Controls.Label
                {
                    Margin = new Thickness(1),
                    Foreground = new SolidColorBrush(Colors.Blue),
                    Content = item.toolTipMapObject.ToString()
                };

                stackPanel.Children.Add(item.index == 0 
                    ? (UIElement) label 
                    : new Border
                    {
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        BorderBrush = new SolidColorBrush(Colors.White),
                        Child = label
                    });
            }

            // create and show tool tip
            customizedToolTip = new System.Windows.Controls.ToolTip
            {
                Content = stackPanel,
                IsOpen = true
            };
        }

        private void DestroyOwnToolTip(List<IMapObject> toolTipMapObjects)
        {
            if (customizedToolTip == null) return;

            // close tool tip
            customizedToolTip.IsOpen = false;
            customizedToolTip = null;
        }

        private System.Windows.Controls.ToolTip customizedToolTip;

        private static ILayer CreateShapeLayer(Point center)
        {
            var shapeLayer = new ShapeLayer("Shape layer") {Copyright = "DDS"};
            AddCircle(shapeLayer, center, 500);
            return shapeLayer;
        }

        private static void AddCircle(ShapeLayer layer, Point center, double radius)
        {
            // calculate the size in mercator units
            var cosB = Math.Cos(center.Y * Math.PI / 180.0); // factor depends on latitude
            var ellipseSize = Math.Abs(1.0 / cosB * radius) * 2; // size mercator units

            var ellipse = new Ellipse
            {
                Width = ellipseSize,
                Height = ellipseSize,
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
                Stroke = new SolidColorBrush(Colors.LightBlue),
                StrokeThickness = 25
            };

            ShapeCanvas.SetScaleFactor(ellipse, 1); // scale linear
            ShapeCanvas.SetLocation(ellipse, center);
            layer.Shapes.Add(ellipse);
        }
    }
}

