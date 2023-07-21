using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicDent2
{
    /// <summary>
    /// Логика взаимодействия для FullSizeImageControl.xaml
    /// </summary>
    public partial class FullSizeImageControl : Window
    {
        ScaleTransform scaleTransform;
        TranslateTransform translateTransform;
        TransformGroup transformGroup;
        Point start;
        Point origin;
        ImageSource imageSource;
        double zoomStep = 0.2;
        public FullSizeImageControl(byte[] imageSourceToSet)
        {
            MemoryStream ms = new MemoryStream(imageSourceToSet);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();

            imageSource = bitmapImage;
            InitializeComponent();
            initializeTransforms();
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            grid.Height = SystemParameters.WorkArea.Height;
            grid.Width = SystemParameters.WorkArea.Width;
            imageControl.Source = imageSource;
        }

        private void initializeTransforms()
        {
            imageControl.RenderTransformOrigin = new Point(0.5, 0.5);

            scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
            scaleTransform.CenterX = 0.5;
            scaleTransform.CenterY = 0.5;

            translateTransform = new TranslateTransform(0, 0);

            transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            imageControl.RenderTransform = transformGroup;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? zoomStep : -zoomStep;
            scaleTransform.ScaleX += zoom;
            scaleTransform.ScaleY += zoom;
        }

        private void grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            imageControl.CaptureMouse();
            start = e.GetPosition(border);
            origin = new Point(translateTransform.X, translateTransform.Y);
        }

        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (imageControl.IsMouseCaptured)
            {
                Vector v = start - e.GetPosition(border);
                translateTransform.X = origin.X - v.X;
                translateTransform.Y = origin.Y - v.Y;
            }
        }

        private void grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageControl.ReleaseMouseCapture();
        }

        private void fullSizeImageControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus || e.Key == Key.Add)
            {
                scaleTransform.ScaleX += zoomStep;
                scaleTransform.ScaleY += zoomStep;
            }
            else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
            {
                scaleTransform.ScaleX -= zoomStep;
                scaleTransform.ScaleY -= zoomStep;
            }
        }
    }
}
