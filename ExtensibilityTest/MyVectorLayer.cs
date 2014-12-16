//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Canvases;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class MyVectorCanvas : VectorCanvas
    {
        #region constructor
        /// <summary>  </summary>
        /// <param name="mapControl"></param>
        public MyVectorCanvas(MapView mapView)
            : base(mapView)
        {
            var myPolygon = new MapPolygon()
            {
                Points = new PointCollection{ new Point(20,40), new Point(20, 50), new Point(30,50), new Point(30,40) }
            };
            myPolygon.Fill = new SolidColorBrush(Colors.Blue);
            myPolygon.Stroke = new SolidColorBrush(Colors.Black);
            myPolygon.InvariantStrokeThickness = 3;
            Children.Add(myPolygon);

            //// http://msdn.microsoft.com/en-us/library/system.windows.media.animation.coloranimation.aspx
            SolidColorBrush myAnimatedBrush = new SolidColorBrush();
            myAnimatedBrush.Color = Colors.Blue;
            myPolygon.Fill = myAnimatedBrush;
            MapView.RegisterName("MyAnimatedBrush", myAnimatedBrush);
            ColorAnimation mouseEnterColorAnimation = new ColorAnimation();
            mouseEnterColorAnimation.From = Colors.Blue;
            mouseEnterColorAnimation.To = Colors.Red;
            mouseEnterColorAnimation.Duration = TimeSpan.FromMilliseconds(250);
            mouseEnterColorAnimation.AutoReverse = true;
            Storyboard.SetTargetName(mouseEnterColorAnimation, "MyAnimatedBrush");
            Storyboard.SetTargetProperty(mouseEnterColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
            Storyboard mouseEnterStoryboard = new Storyboard();
            mouseEnterStoryboard.Children.Add(mouseEnterColorAnimation);
            myPolygon.MouseEnter += delegate(object msender, MouseEventArgs args)
            {
                mouseEnterStoryboard.Begin(MapView);
            };
        }
        #endregion

        #region public methods
        /// <summary> Could also add Shapes dynamically </summary>
        /// <param name="updateMode"></param>
        public override void Update(UpdateMode updateMode)
        {
            base.Update(updateMode);
        }

        /// <summary>  </summary>
        public override void Dispose()
        {
            MapView.UnregisterName("MyAnimatedBrush");
            base.Dispose();
        }
        #endregion
    }
}