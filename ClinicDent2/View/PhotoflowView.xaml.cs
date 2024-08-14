using ClinicDent2.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2.View
{
    /// <summary>
    /// Interaction logic for PhotoflowView.xaml
    /// </summary>
    public partial class PhotoflowView : UserControl
    {
        PhotoflowViewModel photoflowViewModel;
        public PhotoflowView()
        {
            photoflowViewModel= new PhotoflowViewModel();
            DataContext= photoflowViewModel;
            InitializeComponent();
        }
        private void buttonAddPhotos_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.tif, *.tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|All files(*.*)|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                photoflowViewModel.AddPhotos(openFileDialog.FileNames);
            }
        }
        private void PageButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int buttonPage = (int)button.Content;
            int selectedPage = photoflowViewModel.SelectedPage;
            if (buttonPage == selectedPage)
            {
                button.Background = Brushes.Blue;
                button.Foreground = Brushes.White;
            }
            else
            {
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Black;
            }
        }
    }
}
