// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Ptv.XServer.Demo.UseCases
{
    /// <summary>
    /// Control to show up a intro frame.
    /// </summary>
    public partial class IntroControl
    {
        public IntroControl()
        {
            InitializeComponent();
        }

        public Action Forwarded;
        public Action<bool> Skipped;
        public Action Backwarded;

        public bool BackButton
        {
            get { return (BackIcon.Visibility == System.Windows.Visibility.Visible); }
            set { BackIcon.Visibility = BackLabel.Visibility = BackRect.Visibility = BackEllipse.Visibility = 
                (value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden; }
        }

        public string Title
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }

        public string Text
        {
            get { return TextLabel.Text; }
            set { TextLabel.Text = value; }
        }

        public string NextButtonText
        {
            get { return NextLabel.Content.ToString(); }
            set { NextLabel.Content = value; }
        }

        private void Skip_MouseEnter(object sender, MouseEventArgs e)
        {
            SkipEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
            SkipLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
        }

        private void Skip_MouseLeave(object sender, MouseEventArgs e)
        {
            SkipEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
            SkipLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            BackEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
            BackLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            BackEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
            BackLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
        }

        private void Next_MouseEnter(object sender, MouseEventArgs e)
        {
            NextEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
            NextLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
        }

        private void Next_MouseLeave(object sender, MouseEventArgs e)
        {
            NextEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
            NextLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
        }

        private void Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetSkipGrid(false);
            Forwarded();
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetSkipGrid(false);
            Backwarded();
        }

        private void Skip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetSkipGrid(true);
        }

        private void SetSkipGrid(bool visible)
        {
            SkipIcon.Visibility = SkipLabel.Visibility = SkipRect.Visibility = SkipEllipse.Visibility 
                = (!visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            SkipGrid.Visibility = (visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        private void SkipYes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Skipped(true);
        }

        private void SkipYes_MouseEnter(object sender, MouseEventArgs e)
        {
            SkipYes.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
        }

        private void SkipYes_MouseLeave(object sender, MouseEventArgs e)
        {
            SkipYes.Foreground = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
        }

        private void SkipNo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Skipped(false);
        }

        private void SkipNo_MouseEnter(object sender, MouseEventArgs e)
        {
            SkipNo.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 74, 77));
        }

        private void SkipNo_MouseLeave(object sender, MouseEventArgs e)
        {
            SkipNo.Foreground = new SolidColorBrush(Color.FromArgb(255, 115, 115, 116));
        }
    }
}
