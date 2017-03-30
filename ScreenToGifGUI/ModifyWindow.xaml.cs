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
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
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

        private bool _isPlayReverse;
        private bool _isPlayStop = true;

        public ModifyWindow(List<byte[]> images, int fps, int width, int height)
        {
            InitializeComponent();
            _viewModel = DataContext as ModifyWindowViewModel;
            _viewModel.Images = new ObservableCollection<BitmapImage>();
            _viewModel.Width = width;
            _viewModel.Height = height;
            _viewModel.Ratio = (double)width / (double)height;
            _viewModel.IsRespectRatio = true;
            _viewModel.IsReverse = false;
            _viewModel.Fps = fps;
            _imagesByte = images;
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
                while ((next == null || _isDeleteds[nextIndex]) 
                    && !_isPlayStop)
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
                Thread.Sleep(1000 / _viewModel.Fps);
            }
        }
        
        private void UpdateTotalSize()
        {
            int size = 0;
            for (int i = 0; i < _previewImages.Count; i++)
            {
                if (!_isDeleteds[i])
                {
                    size += _imagesByte[i].Length;
                }
            }
            _viewModel.TotalSize = string.Format("{0:N0} Bytes", size);
        }

        private void DeleteImage(int index)
        {
            _previewImages[index].IsDeleted = true;
            _isDeleteds[index] = true;
        }

        private void RestoreImage(int index)
        {
            _previewImages[index].IsDeleted = false;
            _isDeleteds[index] = false;
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
                e.Handled = true;
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
                e.Handled = true;
            }
            if (e.Key == Key.Delete)
            {
                _previewImages[_selectedImageIndex].IsDeleted =
                    !_previewImages[_selectedImageIndex].IsDeleted;
                _isDeleteds[_selectedImageIndex] = 
                    _previewImages[_selectedImageIndex].IsDeleted;
                e.Handled = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                _isPlayStop = true;
                e.Handled = true;
            }
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
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*GIF文件(*.gif)|*.gif";
            sfd.FileName = "gif";
            sfd.AddExtension = false;
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                stg.Fps = _viewModel.Fps;
                stg.GifFileName = sfd.FileName;
                stg.Jpgs = _imagesByte;
                stg.Width = _viewModel.Width;
                stg.Height = _viewModel.Height;
                if (_viewModel.IsReverse)
                {
                    stg.Jpgs.Reverse();
                }
                stg.JpgsToGif();
                //Close();
            }
        }

        private void deleteAllForwardButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= _selectedImageIndex; i++)
            {
                DeleteImage(i);
            }
        }

        private void deleteAllBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = _selectedImageIndex; i < _previewImages.Count; i++)
            {
                DeleteImage(i);
            }
        }

        private void restoreAllForwardButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= _selectedImageIndex; i++)
            {
                RestoreImage(i);
            }
        }

        private void restoreAllBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = _selectedImageIndex; i < _previewImages.Count; i++)
            {
                RestoreImage(i);
            }
        }
    }
}
