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
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.IO;
using ScreenToGif;
using ScreenToGifGUI.ViewModels;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Keys = System.Windows.Forms.Keys;

namespace ScreenToGifGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private STGProcessor _stg;
        private Rectangle _targetBorder;
        private Bitmap _currentScreenShot;
        private IntPtr _hwnd;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = DataContext as MainWindowViewModel;
            _viewModel.Fps = 10;
            _viewModel.HasMouse = false;
            _stg = new STGProcessor();
            
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(_hwnd);
            //if (hWndSource != null)
                hWndSource.AddHook(WndProc);

            List<Keys> validKeys = new DataProvider().GetValidValuesFromKeys().ToList();
            setAreaHotkeyComboBox.SelectedIndex = validKeys.IndexOf(Keys.X);
            recordHotkeyComboBox.SelectedIndex = validKeys.IndexOf(Keys.C);
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

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*JPG文件(*.jpg)|*.jpg";
            sfd.AddExtension = false;
            sfd.FileName = "screenshot";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bitmap.Save(sfd.FileName, ImageFormat.Jpeg);
            }
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
            _currentScreenShot = _stg.ScreenShot_Full(_viewModel.HasMouse);
            MaskWindow mw = new MaskWindow(new Bitmap(_currentScreenShot));
            mw.ScreenShotCallback += ScreenShot;
            mw.SetAreaCallback += SetBorder;
            mw.Show();
        }

        private void StartRecord()
        {
            if (_targetBorder == default(Rectangle))
            {
                return;
            }
            _stg.TargetArea = _targetBorder;
            _stg.Fps = _viewModel.Fps;
            _stg.HasMouse = _viewModel.HasMouse;
            _stg.StartRecord();
        }

        private void StopRecord()
        {
            _stg.StopRecord();
            ModifyWindow mw = new ModifyWindow(_stg.Jpgs, _stg.Fps, _targetBorder.Width, _targetBorder.Height);
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

        private void openGifButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*GIF文件(*.gif)|*.gif";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                STGProcessor stg = new STGProcessor();
                stg.GifFileName = ofd.FileName;
                stg.GifToJpgs();
                ModifyWindow mw = new ModifyWindow(stg.Jpgs, stg.Fps, stg.Width, stg.Height);
                mw.Show();
            }
        }

        private void setAreaHotkeyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalHotKey.Unregister(_hwnd, ShowMaskWindow);
            bool success = false;
            success = GlobalHotKey.Register(_hwnd,
                GlobalHotKey.Modifier.Control,
                GlobalHotKey.Modifier.Alt,
                (Keys)Enum.Parse(typeof(Keys), e.AddedItems[0].ToString(), true),
                ShowMaskWindow);
            if (!success)
            {
                MessageBox.Show("This hotkey has been occupied! Please select another one.");
            }
        }

        private void recordHotkeyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalHotKey.Unregister(_hwnd, StartOrStopRecord);
            bool success = false;
            success = GlobalHotKey.Register(_hwnd,
                GlobalHotKey.Modifier.Control,
                GlobalHotKey.Modifier.Alt,
                (Keys)Enum.Parse(typeof(Keys), e.AddedItems[0].ToString(), true),
                StartOrStopRecord);
            if (!success)
            {
                MessageBox.Show("This hotkey has been occupied! Please select another one.");
            }
        }
    }
}
