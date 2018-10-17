// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo
{
    /// <summary>
    /// Simple, topmost splash window, replacing System.Windows.SplashScreen (see remarks).
    /// </summary>
    /// <remarks>This is a replacement for the built-in System.Windows.SplashScreen class,
    /// which doesn't keep the splash window topmost (it flashes when other windows are 
    /// opened). Starting with .NET 4.0, SplashScreen.Show offers a second parameter named 
    /// "topMost" which probably solves this issue. However, as long as xServer .NET is 
    /// based on .NET 3.5, SplashWindow provides a valid workaround.
    /// </remarks>
    public class SplashWindow
    {
        /// <summary>
        /// window displaying the splash image
        /// </summary>
        private Window _splashWindow;

        /// <summary>
        /// splash image to be displayed
        /// </summary>
        private readonly BitmapImage _splashImage;

        /// <summary>
        /// fade out duration
        /// </summary>
        private TimeSpan _fadeOutDuration;

        /// <summary>
        /// state handling
        /// </summary>
        private int _state;

        /// <summary>
        /// Creates a SplashWindow.
        /// </summary>
        /// <param name="resourceName">The name of the resource 
        /// containing the splash image to be displayed</param>
        public SplashWindow(string resourceName)
        {
            _splashImage = ResourceHelper.LoadBitmapFromResource(resourceName);

            if (_splashImage == null)
                throw new NullReferenceException("failed to load splash imge");

            if (_splashImage.PixelHeight < 32 || _splashImage.PixelWidth < 32)
                throw new ArgumentException("splash image is invalid");
        }

        /// <summary>
        /// Shows the splash.
        /// </summary>
        public void Show()
        {
            if (Interlocked.Increment(ref _state) != 1)
                throw new ProtocolException("Show() is to be called only one");

            Action createAndShowSplashWindow = CreateAndShowSplashWindow;

            createAndShowSplashWindow.RunAsync(true, true);
        }

        /// <summary>
        /// Creates and shows the splash splashWindow.
        /// </summary>
        private void CreateAndShowSplashWindow()
        {
            _splashWindow = new Window
            {
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Width = _splashImage.PixelWidth + 30,
                Height = _splashImage.PixelHeight + 30,
                BorderThickness = new Thickness(0),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent),
                Topmost = true,
                Opacity = 0,

                Content = new Rectangle
                {
                    Width = _splashImage.PixelWidth,
                    Height = _splashImage.PixelHeight,

                    Fill = new ImageBrush
                    {
                        ImageSource = _splashImage,
                        Stretch = Stretch.Uniform
                    },

                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Opacity = 0.5,
                        Direction = -45,
                        ShadowDepth = 10,
                        BlurRadius = 10,
                    }
                }
            };

            _splashWindow.Closing += Closing;
            _splashWindow.Show();

            _splashWindow.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));

            System.Windows.Threading.Dispatcher.Run();
        }

        /// <summary>
        /// Handles the closing event, possibly starting a fade 
        /// out animation ultimately closing the window.
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void Closing(object sender, CancelEventArgs e)
        {
            // avoid second call in any case
            _splashWindow.Closing -= Closing;

            // animated fade out only if _fadeOutDuration defines a valid time span
            // when animated, this first close operation is to be cancelled
            e.Cancel = _fadeOutDuration.TotalMilliseconds >= 10;

            if (!e.Cancel)
            {
                // _fadeOutDuration was not valid, so we're done
                _state = 0;
            }
            else
            {
                // _fadeOutDuration was valid; start a fade out animation
                // that finally closes the window when the animation completes

                var anim = new DoubleAnimation(0, _fadeOutDuration);

                anim.Completed += (_, __) =>
                {
                    _state = 0;
                    _splashWindow.Close();
                };

                _splashWindow.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }

        /// <summary>
        /// Closes the splash window.
        /// </summary>
        /// <param name="mainWindow">The window to focus after the splash window was closed.</param>
        private void InternalClose(Window mainWindow)
        {
            // Close the splash, then activate and focus the main window.

            _splashWindow.Close();

            mainWindow?.Invoke(() =>
            {
                mainWindow.Activate();
                mainWindow.Focus();
            });
        }

        /// <summary>
        /// Closes the splash window.
        /// </summary>
        /// <param name="mainWindow">The window to focus after the splash window was closed.</param>
        /// <param name="delay">The amount of time to delay the close operation.</param>
        /// <param name="fadeOutDuration">The duration of the fade out animation.</param>
        private void InternalClose(Window mainWindow, TimeSpan delay, TimeSpan fadeOutDuration)
        {
            // store fadeOutDuration in a member, it is used 
            // in the Closing event handler later on.

            _fadeOutDuration = fadeOutDuration;

            // CloseSplashWindow is executed on a sperate thread. 
            // Wait some time (> delay), then close the splash window. 

            Thread.Sleep((int)delay.TotalMilliseconds);
            _splashWindow.Invoke(() => InternalClose(mainWindow));
        }


        /// <summary>
        /// Starts a thread that closes the splash asynchronously.
        /// </summary>
        /// <param name="mainWindow">The window to focus after the splash window was closed.</param>
        /// <param name="delay">The amount of time to delay the close operation.</param>
        /// <param name="fadeOutDuration">The duration of the fade out animation.</param>
        public void Close(Window mainWindow, TimeSpan delay, TimeSpan fadeOutDuration)
        {
            if (Interlocked.Increment(ref _state) != 2)
                throw new ProtocolException("Close(...) is to be called after Show() and only one");

            Action closeAction = () => 
                InternalClose(mainWindow, delay, fadeOutDuration);

            closeAction.RunAsync();
        }
    }

    /// <summary>
    /// Extension methods
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Simplifies Dispatcher - see remarks.
        /// </summary>
        /// <param name="wnd">The window whos dispatcher is to be used.</param>
        /// <param name="action">The action to be invoked.</param>
        /// <remarks>This simple extension method simplifies Dispatcher.Invoke calls 
        /// by elmininating some necessary typecasts.</remarks>
        public static void Invoke(this Window wnd, Action action)
        {
            wnd.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Runs a given action in a separate thread.
        /// </summary>
        /// <param name="a">Action to run async.</param>
        /// <param name="STA">If set to true, set the threads apartment state to STA.</param>
        /// <param name="background">If set to true, sets the threads IsBackground flag.</param>
        public static void RunAsync(this Action a, bool? STA = null, bool? background = null)
        {
            var th = new Thread(new ThreadStart(a));

            if (STA.GetValueOrDefault(false))
                th.SetApartmentState(ApartmentState.STA);

            if (background.HasValue)
                th.IsBackground = background.Value;

            th.Start();
        }
    }
}
