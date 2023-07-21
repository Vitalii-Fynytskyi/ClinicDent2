using ClinicDent2.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2.View
{
    /// <summary>
    /// Interaction logic for SelectImagesView.xaml
    /// </summary>
    public partial class SelectImagesView : UserControl
    {
        PhotoflowViewModel photoflowViewModel;
        StageViewModel stageViewModel;
        public SelectImagesView(StageViewModel stageViewModelToSet)
        {
            photoflowViewModel = new PhotoflowViewModel();
            DataContext = photoflowViewModel;
            this.stageViewModel = stageViewModelToSet;
            InitializeComponent();
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            ImageViewModel[] selectedImages = photoflowViewModel.ImageViewModels.Where(i => i.IsSelected == true).ToArray();
            stageViewModel.AttachImages(selectedImages);
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Close();
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
