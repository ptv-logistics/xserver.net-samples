//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class MyVectorCanvas : VectorCanvas
    {
        #region constructor
        /// <summary>  </summary>
        /// <param name="mapView"></param>
        public MyVectorCanvas(MapView mapView)
            : base(mapView)
        {
            var myPolygon = new MapPolygon
            {
                Points = new PointCollection { new Point(20, 40), new Point(20, 50), new Point(30, 50), new Point(30, 40) },
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Black),
                InvariantStrokeThickness = 3
            };
            Children.Add(myPolygon);

            //// http://msdn.microsoft.com/en-us/library/system.windows.media.animation.coloranimation.aspx
            var myAnimatedBrush = new SolidColorBrush { Color = Colors.Blue };
            myPolygon.Fill = myAnimatedBrush;
            MapView.RegisterName("MyAnimatedBrush", myAnimatedBrush);
            var mouseEnterColorAnimation = new ColorAnimation
            {
                From = Colors.Blue,
                To = Colors.Red,
                Duration = TimeSpan.FromMilliseconds(250),
                AutoReverse = true
            };
            Storyboard.SetTargetName(mouseEnterColorAnimation, "MyAnimatedBrush");
            Storyboard.SetTargetProperty(mouseEnterColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));

            var mouseEnterStoryboard = new Storyboard();
            mouseEnterStoryboard.Children.Add(mouseEnterColorAnimation);
            myPolygon.MouseEnter += delegate
            {
                mouseEnterStoryboard.Begin(MapView);
            };
        }
        #endregion

        #region public methods

        /// <summary>  </summary>
        public override void Dispose()
        {
            MapView.UnregisterName("MyAnimatedBrush");
            base.Dispose();
        }
        #endregion
    }
}