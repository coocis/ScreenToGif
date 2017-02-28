using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using ScreenToGifGUI.ViewModels;
using ScreenToGifGUI.UserControls;
using System.IO;
using System.Threading;
using ScreenToGif;

namespace ScreenToGifGUI
{
    /// <summary>
    /// Interaction logic for ModifyWindow.xaml
    /// </summary>
    public partial class ModifyWindow : Window
    {
        private ModifyWindowViewModel _viewModel;
        private PreviewImage _selectedImage;
        private int _selectedImageIndex;
        private List<PreviewImage> _previewImages = new List<PreviewImage>();
        private List<byte[]> _imagesByte = new List<byte[]>();
        private List<bool> _isDeleteds;
        private int _fps;

        private bool _isPlayReverse;
        private bool _isPlayStop = true;

        public ModifyWindow(List<byte[]> images, int fps)
        {
            InitializeComponent();
            _viewModel = DataContext as ModifyWindowViewModel;
            _viewModel.Images = new ObservableCollection<BitmapImage>();
            _imagesByte = images;
            _fps = fps;
            _isDeleteds = new List<bool>(new bool[images.Count]);
            images.ForEach(img =>
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(img);
                bi.EndInit();
                _viewModel.Images.Add(bi);
            });
        }

        private void PlayTheGifFromCurrent()
        {
            while (!_isPlayStop)
            {
                int nextIndex = _selectedImageIndex;
                PreviewImage next = null;
                while (next == null || _isDeleteds[_selectedImageIndex])
                {
                    if (!_isPlayReverse)
                    {
                        nextIndex = (nextIndex + 1) % imageItems.Items.Count;
                    }
                    if (_isPlayReverse)
                    {
                        nextIndex = (nextIndex - 1 + imageItems.Items.Count) % imageItems.Items.Count;
                    }
                    next = _previewImages[nextIndex];
                }
                Dispatcher.Invoke(new Action(
                    () =>
                    PreviewImage_Click(next, new RoutedEventArgs())
                    ));
                Thread.Sleep(1000 / _fps);
            }
        }
        
        private void PreviewImage_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedImage != null)
            {
                _selectedImage.IsSelected = false;
            }
            _selectedImage = sender as PreviewImage;
            _selectedImage.IsSelected = true;
            _selectedImageIndex = imageItems.Items.IndexOf(_selectedImage.MainImage);

            double p = (_selectedImage.Width + _selectedImage.Margin.Right) * _selectedImageIndex ;
            p -= imagesScrollViewer.ActualWidth / 2 - _selectedImage.Width / 2;
            imagesScrollViewer.ScrollToHorizontalOffset(p);
            _viewModel.MainImage = _selectedImage.MainImage;
        }

        private void DeleteImage(int index)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (_isPlayStop)
                {
                    _isPlayReverse = false;
                    _isPlayStop = false;
                    Thread t = new Thread(PlayTheGifFromCurrent);
                    t.Start();
                }
            }
            if (e.Key == Key.Left)
            {
                if (_isPlayStop)
                {
                    _isPlayReverse = true;
                    _isPlayStop = false;
                    Thread t = new Thread(PlayTheGifFromCurrent);
                    t.Start();
                }
            }
            if (e.Key == Key.Delete)
            {
                _previewImages[_selectedImageIndex].IsDeleted =
                    !_previewImages[_selectedImageIndex].IsDeleted;
                _isDeleteds[_selectedImageIndex] = 
                    _previewImages[_selectedImageIndex].IsDeleted;
            }
            e.Handled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                _isPlayStop = true;
            }
            e.Handled = true;
        }

        private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
        {
            _previewImages.Add(sender as PreviewImage);
            if (_previewImages.Count == 1)
            {
                PreviewImage_Click(_previewImages[0], new RoutedEventArgs());
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            STGProcessor stg = new STGProcessor();
            for (int i = 0, j = 0; i < _previewImages.Count; i++)
            {
                if (_previewImages[i].IsDeleted)
                {
                    _imagesByte.RemoveAt(i - j);
                    j++;
                }
            }
            stg.Fps = _fps;
            stg.GifFileName = "gif";
            stg.Jpgs = _imagesByte;
            stg.Width = 250;
            stg.JpegsToGif();
        }
    }
}
