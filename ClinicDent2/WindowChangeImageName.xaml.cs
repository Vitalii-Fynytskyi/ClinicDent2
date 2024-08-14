using ClinicDent2.ViewModel;
using System;
using System.Windows;
using ClinicDentClientCommon.Services;

namespace ClinicDent2
{
    public partial class WindowChangeImageName : Window
    {
        ImageViewModel image;
        public WindowChangeImageName(ImageViewModel imageToSet)
        {
            image = imageToSet;
            InitializeComponent();
            DataContext = image;
        }

        private void buttonConfirmRename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpService.RenameImage(image.Id, textBoxName.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось переіменувати зображення: {ex.Message}", "Помилка");
                return;
            }
            image.FileName = textBoxName.Text;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
