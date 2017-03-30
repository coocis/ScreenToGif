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
using System.Text.RegularExpressions;
using ImageMagick;

namespace ScreenToGif
{
    public class STGProcessor
    {
        private int _fps = 15;
        private int _width = 250;
        private int _height = 200;
        private bool _hasMouse = false;
        private Rectangle _targetArea;
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
                    Height = (int)((double)Width * ((double)_targetArea.Height / (double)_targetArea.Width));
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

        public Rectangle TargetArea
        {
            get
            {
                return _targetArea;
            }

            set
            {
                _targetArea = value;
                if (_isPreserveAspectRatio)
                {
                    Height = (int)((double)Width * ((double)_targetArea.Height / (double)_targetArea.Width));
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
                TargetArea = Rectangle.Union(TargetArea, screen.Bounds);
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
        }

        public void JpgsToGif()
        {
            if (!Directory.Exists(@"temp"))
            {
                Directory.CreateDirectory(@"temp");
            }
            MagickNET.SetTempDirectory(@"temp");
            using (MagickImageCollection mic = new MagickImageCollection())
            {
                foreach (var image in Jpgs)
                {
                    MagickImage mi = new MagickImage(image);
                    mi.Resize(Width, Height);
                    mi.AnimationDelay = 100 / Fps;
                    mic.Add(mi);
                }
                mic.Write(GifFileName);
            }
        }

        /* FFMPEG ver.
        public void JpgsToGif()
        {
            File.Delete(GifFileName);
            //_startInfo.Arguments = string.Format("-f image2pipe -framerate {0} -i - -vf scale={1}*{2} -pix_fmt rgb48 {3}",
            _startInfo.Arguments = string.Format("-f image2pipe -framerate {0} -i - -vf scale={1}*{2},format=rgb24 {3}",
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
        */

        public void GifToJpgs()
        {
            GetGifInfo(GifFileName);
            _startInfo.RedirectStandardOutput = true;
            //_startInfo.Arguments = string.Format("-f image2pipe -i - -f image2pipe -");
            _startInfo.Arguments = string.Format("-i {0} -f image2pipe -", GifFileName);
            _ffmpeg.Start();

            Jpgs = new List<byte[]>();
            BinaryReader br = new BinaryReader(_ffmpeg.StandardOutput.BaseStream);
            byte[] bs = new byte[4096];
            List<byte> all = new List<byte>();
            int i;
            while ((i = br.Read(bs, 0 , bs.Length)) > 0)
            {
                all.AddRange(bs);
            }
            br.Close();
            Jpgs = SeperateBytesToJpgs(all);

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
                bmp = ScreenShot(TargetArea, HasMouse);
                ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                Jpgs.Add(ms.ToArray());
                bmp.Dispose();

                i++;
                Thread.Sleep(1000 / Fps);
            }
        }

        private List<byte[]> SeperateBytesToJpgs(List<byte> data)
        {
            List<byte[]> results = new List<byte[]>();
            byte[] head = new byte[] { 0xff, 0xd8 };
            int startIndex = 0;
            int matchIndex = 0;
            //不匹配第一个JPG的开头
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i] == head[matchIndex])
                {
                    matchIndex++;
                    if (matchIndex == head.Length)
                    {
                        results.Add(data.GetRange(startIndex, i - head.Length - startIndex).ToArray());
                        startIndex = i - (head.Length - 1);
                        matchIndex = 0;
                    }
                }
                else
                {
                    i = i - matchIndex;
                    matchIndex = 0;
                }
            }
            return results;
        }

        private void GetGifInfo(string gifPath)
        {
            _startInfo.Arguments = string.Format("-i {0}", gifPath);
            _ffmpeg.Start();
            string output = _ffmpeg.StandardError.ReadToEnd();
            _ffmpeg.WaitForExit();

            Match match = Regex.Match(output, "[0-9]+x[0-9]+");
            Width = int.Parse(match.Value.Split('x')[0]);
            Height = int.Parse(match.Value.Split('x')[1]);
            match = Regex.Match(output, "[0-9.]+ fps");
            Fps = (int)Math.Round(double.Parse(match.Value.Split(' ')[0]));
        }
    }
}
