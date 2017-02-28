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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
