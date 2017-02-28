using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace ScreenToGif
{
    public class STGProcessor
    {
        private int _fps = 15;
        private int _width = 250;
        private int _height = 200;
        private bool _hasMouse = false;
        private Rectangle _targetRect;
        private List<byte[]> _jpgs = new List<byte[]>();
        private string _gifFileName = "gif";
        private bool _isPreserveAspectRatio = true;
        private bool _isRecording;

        private Process _ffmpeg = new Process();
        private ProcessStartInfo _startInfo = new ProcessStartInfo();
        private Thread _recordThread;
        private string _tempFilesDirectory = "temp";

        public int Fps
        {
            get
            {
                return _fps;
            }

            set
            {
                _fps = value;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
                if (_isPreserveAspectRatio)
                {
                    Height = (int)((double)Width * ((double)_targetRect.Height / (double)_targetRect.Width));
                }
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        public bool HasMouse
        {
            get
            {
                return _hasMouse;
            }

            set
            {
                _hasMouse = value;
            }
        }

        public Rectangle TargetRect
        {
            get
            {
                return _targetRect;
            }

            set
            {
                _targetRect = value;
                if (_isPreserveAspectRatio)
                {
                    Height = (int)((double)Width * ((double)_targetRect.Height / (double)_targetRect.Width));
                }
            }
        }

        public List<byte[]> Jpgs
        {
            get
            {
                return _jpgs;
            }

            set
            {
                _jpgs = value;
            }
        }

        public string GifFileName
        {
            get
            {
                return _gifFileName;
            }

            set
            {
                _gifFileName = value;
            }
        }

        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }

            private set
            {
                _isRecording = value;
            }
        }

        public STGProcessor()
        {
            _ffmpeg.StartInfo = _startInfo;
            _startInfo.FileName = "ffmpeg.exe";
            _startInfo.UseShellExecute = false;
            _startInfo.CreateNoWindow = true;
            _startInfo.RedirectStandardOutput = false;
            _startInfo.RedirectStandardError = true;
            _startInfo.RedirectStandardInput = true;
            IsRecording = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                TargetRect = Rectangle.Union(TargetRect, screen.Bounds);
            }
        } 

        ~STGProcessor()
        {
            if (_recordThread != null)
            {
                _recordThread.Abort();
            }
        }

        public Bitmap ScreenShot(Rectangle rect, bool hasMouse = true)
        {
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }
            if (hasMouse)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    Rectangle cursor = new Rectangle(Control.MousePosition, new Size(18, 13));
                    Cursors.Default.Draw(g, cursor);
                }
            }

            return bmp;
        }

        public Bitmap ScreenShot_Full(bool hasMouse = true)
        {
            Rectangle _screenArea = new Rectangle();
            foreach (Screen screen in Screen.AllScreens)
            {
                _screenArea = Rectangle.Union(_screenArea, screen.Bounds);
            }
            return ScreenShot(_screenArea, hasMouse);
        }

        public void StartRecord()
        {
            if (IsRecording)
            {
                return;
            }
            IsRecording = true;
            _recordThread = new Thread(Record);
            _recordThread.Start();
            Jpgs = new List<byte[]>();
        }

        public void StopRecord()
        {
            if (!IsRecording)
            {
                return;
            }
            _recordThread.Abort();
            IsRecording = false;
            //JpegsToGif();
        }

        public void JpegsToGif()
        {
            File.Delete(GifFileName + ".gif");
            _startInfo.Arguments = string.Format("-f image2pipe -framerate {0} -i - -vf scale={1}*{2} {3}.gif",
                Fps,
                Width,
                Height,
                GifFileName);
            _ffmpeg.Start();
            BinaryWriter bw = new BinaryWriter(_ffmpeg.StandardInput.BaseStream);
            for (int i = 0; i < Jpgs.Count; i++)
            {
                bw.Write(Jpgs[i], 0, Jpgs[i].Length);
            }
            bw.Close();
            //输出ffmpeg输出的结果（gif的信息而不是gif文件本身）
            string output = _ffmpeg.StandardError.ReadToEnd();
            StreamWriter fs = new StreamWriter(File.Create("log.txt"));
            fs.Write(output);
            fs.Close();
            _ffmpeg.WaitForExit();
        }

        private void Record()
        {
            Bitmap bmp;
            MemoryStream ms;
            int i = 1;
            Directory.CreateDirectory(_tempFilesDirectory);
            while (IsRecording)
            {
                bmp = ScreenShot(TargetRect, HasMouse);
                ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                Jpgs.Add(ms.ToArray());
                bmp.Dispose();

                i++;
                Thread.Sleep(1000 / Fps);
            }
        }
    }
}
