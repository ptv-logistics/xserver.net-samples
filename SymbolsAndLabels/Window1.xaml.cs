using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;

namespace SymbolsAndLabels
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

            AddPinWithLabel(myLayer);
        }

        /// <summary>
        /// The arrow demo is uses an adaption of Charles Petzold's WPF arrow class 
        /// http://charlespetzold.com/blog/2007/04/191200.html to be used as custom MapShape
        /// </summary>
        /// <param name="layer"></param>
        public void AddPinWithLabel(ShapeLayer layer)
        {
            // text and symbol as two shapes
            Control pin = new Pin();
            pin.Width = pin.Height = 30;
            ShapeCanvas.SetLocation(pin, new Point(8.4, 49));
            ShapeCanvas.SetAnchor(pin, LocationAnchor.RightBottom);
            layer.Shapes.Add(pin);

            var textBlock = new TextBlock
            {
                Text = "Hello",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.Black)
            };
            ShapeCanvas.SetLocation(textBlock, new Point(8.4, 49));
            ShapeCanvas.SetAnchor(textBlock, LocationAnchor.LeftTop);
            layer.Shapes.Add(textBlock);


            // text with symbol in a view box
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            var viewBox = new Viewbox { Stretch = Stretch.Uniform };
            pin = new Cube();
            viewBox.Child = pin;
            Grid.SetRow(viewBox, 0);
            grid.Children.Add(viewBox);

            viewBox = new Viewbox { Stretch = Stretch.Uniform };
            textBlock = new TextBlock
            {
                Text = "Hello",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.Black)
            };
            viewBox.Child = textBlock;
            Grid.SetRow(viewBox, 1);
            grid.Children.Add(viewBox);

            ShapeCanvas.SetLocation(grid, new Point(8.5, 49));
            ShapeCanvas.SetScaleFactor(grid, .1);
            layer.Shapes.Add(grid);
        }
    }
}
