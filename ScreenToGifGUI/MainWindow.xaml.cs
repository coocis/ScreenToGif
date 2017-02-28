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
using System.Windows.Navigation;
using System.Drawing;
using Size = System.Drawing.Size;

using ScreenToGif;
using System.Drawing.Imaging;
using System.Windows.Interop;

namespace ScreenToGifGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private STGProcessor _stg;
        private Rectangle _targetBorder;
        private Bitmap _currentScreenShot;
        private IntPtr _hwnd;

        public MainWindow()
        {
            InitializeComponent();
            _stg = new STGProcessor();
            
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(_hwnd);
            //if (hWndSource != null)
                hWndSource.AddHook(WndProc);

            GlobalHotKey.Register(_hwnd,
                GlobalHotKey.Modifier.Control,
                GlobalHotKey.Modifier.Alt,
                System.Windows.Forms.Keys.X,
                ShowMaskWindow);
            GlobalHotKey.Register(_hwnd,
                GlobalHotKey.Modifier.Control,
                GlobalHotKey.Modifier.Alt,
                System.Windows.Forms.Keys.C,
                StartOrStopRecord);
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            return GlobalHotKey.CheckHotKeyMessage(msg, wideParam);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GlobalHotKey.Unregister(_hwnd, ShowMaskWindow);
            GlobalHotKey.Unregister(_hwnd, StartOrStopRecord);
            if (_stg.IsRecording)
            {
                StopRecord();
            }
        }

        private void ScreenShot(Rectangle rect)
        {
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height, 
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(_currentScreenShot,
                new Rectangle(0, 0, rect.Width, rect.Height),
                rect,
                GraphicsUnit.Pixel);
            g.Dispose();
            bitmap.Save("screenshot.jpg", ImageFormat.Jpeg);
        }

        private void SetBorder(Rectangle rect)
        {
            _targetBorder = rect;
        }

        private void ShowMaskWindow()
        {
            _targetBorder = new Rectangle(0, 0,
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight);
            _currentScreenShot = _stg.ScreenShot_Full(false);
            MaskWindow mw = new MaskWindow(_currentScreenShot);
            mw.ScreenShotCallback += ScreenShot;
            mw.SetBorderCallback += SetBorder;
            mw.Show();
        }

        private void StartRecord()
        {
            _stg.TargetRect = _targetBorder;
            _stg.StartRecord();
        }

        private void StopRecord()
        {
            _stg.StopRecord();
            ModifyWindow mw = new ModifyWindow(_stg.Jpgs, _stg.Fps);
            mw.Show();
        }

        private void StartOrStopRecord()
        {
            if (_stg.IsRecording)
            {
                StopRecord();
            }
            else
            {
                StartRecord();
            }
        }
    }
}
