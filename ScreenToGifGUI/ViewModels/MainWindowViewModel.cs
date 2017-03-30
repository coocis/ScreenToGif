using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenToGifGUI.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _fps;
        private bool _hasMouse;

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

        public bool HasMouse
        {
            get
            {
                return _hasMouse;
            }

            set
            {
                _hasMouse = value;
                OnPropertyChanged("HasMouse");
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
