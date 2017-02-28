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
using System.Windows.Shapes;

namespace ScreenToGifGUI.UserControls
{
    /// <summary>
    /// Interaction logic for PreviewImage.xaml
    /// </summary>
    public partial class PreviewImage : UserControl
    {
        public delegate void OnClickHandler(object sender, RoutedEventArgs e);
        public event OnClickHandler Click;

        public BitmapImage MainImage
        {
            get { return (BitmapImage)GetValue(MainImageProperty); }
            set { SetValue(MainImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainImageProperty =
            DependencyProperty.Register("MainImage", typeof(BitmapImage), typeof(PreviewImage), new PropertyMetadata(null));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(PreviewImage), new PropertyMetadata(false));



        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            set { SetValue(IsDeletedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDeleted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(PreviewImage), new PropertyMetadata(false));



        public PreviewImage()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }
    }
}
