using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using Screen = System.Windows.Forms.Screen;
using Rectangle = System.Drawing.Rectangle;
using Bitmap = System.Drawing.Bitmap;

namespace ScreenToGifGUI
{
    /// <summary>
    /// Interaction logic for MaskWindow.xaml
    /// </summary>
    public partial class MaskWindow : Window
    {
        public delegate void ScreenShotHandler(Rectangle rect);
        public delegate void SetBorderHandler(Rectangle rect);
        public event ScreenShotHandler ScreenShotCallback;
        public event SetBorderHandler SetBorderCallback;

        private Point _selectStartPoint;
        private double _x, _y, _width, _height;
        private Rectangle _screenArea;

        public MaskWindow(Bitmap fullScreenShot)
        {
            InitializeComponent();
            selectBorder.Visibility = Visibility.Hidden;
            toolboxPanel.Visibility = Visibility.Hidden;
            
            MemoryStream ms = new MemoryStream();
            fullScreenShot.Save(ms, ImageFormat.Bmp);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            backgroudImage.Source = bi;
            foreach (Screen screen in Screen.AllScreens)
            {
                _screenArea = Rectangle.Union(_screenArea, screen.Bounds);
            }
        }

        private void UpdateSelectBorder()
        {
            selectBorder.Margin = new Thickness(_x, _y, 0, 0);
            selectBorder.Width = _width;
            selectBorder.Height = _height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            toolboxPanel.Visibility = Visibility.Hidden;
            _selectStartPoint = e.GetPosition(this);
            selectBorder.Visibility = Visibility.Visible;
            _x = _selectStartPoint.X;
            _y = _selectStartPoint.Y;
            _width = _height = 0;
            UpdateSelectBorder();
        }

        private void screenShotButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (ScreenShotCallback != null)
            {
                ScreenShotCallback(new Rectangle((int)_x, (int)_y, (int)_width, (int)_height));
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (SetBorderCallback != null)
            {
                SetBorderCallback(new Rectangle((int)_x, (int)_y, (int)_width, (int)_height));
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            backgroudImage.Source = null;
            //GC.Collect();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (pos.X + toolboxPanel.Width > _screenArea.Width)
            {
                pos.X = _screenArea.Width - toolboxPanel.Width;
            }
            if (pos.Y + toolboxPanel.Height > _screenArea.Height)
            {
                pos.Y = _screenArea.Height - toolboxPanel.Height;
            }
            toolboxPanel.Margin = new Thickness(pos.X, pos.Y, 0, 0);
            toolboxPanel.Visibility = Visibility.Visible;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                if (current.X > _selectStartPoint.X)
                {
                    _width = current.X - _selectStartPoint.X;
                }
                else
                {
                    _x = current.X;
                    _width = _selectStartPoint.X - current.X;
                }
                if (current.Y > _selectStartPoint.Y)
                {
                    _height = current.Y - _selectStartPoint.Y;
                }
                else
                {
                    _y = current.Y;
                    _height = _selectStartPoint.Y - current.Y;
                }
                UpdateSelectBorder();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
