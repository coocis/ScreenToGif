using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScreenToGifGUI.ViewModels
{
    class ModifyWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BitmapImage> _images;
        private BitmapImage _mainImage;
        private string _totalSize;
        private int _width;
        private int _height;
        private double _ratio;
        private bool _isRespectRatio;
        private bool _isReverse;
        private int _fps;

        public ObservableCollection<BitmapImage> Images
        {
            get
            {
                return _images;
            }

            set
            {
                _images = value;
                OnPropertyChanged("Images");
            }
        }

        public BitmapImage MainImage
        {
            get
            {
                return _mainImage;
            }

            set
            {
                _mainImage = value;
                OnPropertyChanged("MainImage");
            }
        }

        /// <summary>
        /// 目前该项计算出的结果是压缩前的图片总大小，而非压缩后的gif的大小
        /// </summary>
        public string TotalSize
        {
            get
            {
                return _totalSize;
            }

            set
            {
                _totalSize = value;
                OnPropertyChanged("TotalSize");
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
                OnPropertyChanged("Width");
                if (IsRespectRatio)
                {
                    _height = (int)((double)_width / Ratio);
                    OnPropertyChanged("Height");
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
                OnPropertyChanged("Height");
                if (IsRespectRatio)
                {
                    _width = (int)((double)_height * Ratio);
                    OnPropertyChanged("Width");
                }
            }
        }

        public double Ratio
        {
            get
            {
                return _ratio;
            }

            set
            {
                _ratio = value;
                OnPropertyChanged("Ratio");
            }
        }

        public bool IsRespectRatio
        {
            get
            {
                return _isRespectRatio;
            }

            set
            {
                _isRespectRatio = value;
                OnPropertyChanged("IsRespectRatio");
            }
        }

        public bool IsReverse
        {
            get
            {
                return _isReverse;
            }

            set
            {
                _isReverse = value;
                OnPropertyChanged("IsReverse");
            }
        }

        public int Fps
        {
            get
            {
                return _fps;
            }

            set
            {
                _fps = value;
                OnPropertyChanged("Fps");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
