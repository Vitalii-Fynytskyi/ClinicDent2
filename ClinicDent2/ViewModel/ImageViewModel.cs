using ClinicDent2.Commands;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicDent2.ViewModel
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        public RelayCommand ImageClickedCommand { get; set; }
        public RelayCommand CopyImageCommand { get; set; }
        public RelayCommand DeleteImageFromStageCommand { get; set; }
        public RelayCommand DeleteImageCommand { get; set; }
        public RelayCommand RenameImageCommand { get; set; }
        public RelayCommand ShowReferencesCommand { get; set; }
        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                    photoflowOwner?.NotifyPropertyChanged(nameof(photoflowOwner.IsAnyImageSelected));
                }
            }
        }
        public int Id
        {
            get
            {
                return image.Id;
            }
            set
            {
                image.Id = value;
                OnPropertyChanged();
            }
        }
        public string FileName
        {
            get
            {
                return image.FileName;
            }
            set
            {
                if(image.FileName != value)
                {
                    image.FileName = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte[] CompressedBytes
        {
            get
            {
                return image.CompressedBytes;
            }
            set
            {
                if(image.CompressedBytes != value)
                {
                    image.CompressedBytes = value;
                    OnPropertyChanged();
                }
            }
        }
        private async void ImageClicked(object arg)
        {
            if(image.OriginalBytes == null)
            {
                try
                {
                    image.OriginalBytes = await HttpService.GetImageOriginalBytes(image.Id);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Не вдалось завантажити повне зображення:{ex.Message}", "Помилка");
                    return;
                }
            }
            Options.MainWindow.mainMenu.createFullSizeImageControl(image.OriginalBytes);
        }
        private async void CopyImage(object arg)
        {
            if (image.OriginalBytes == null)
            {
                try
                {
                    image.OriginalBytes = await HttpService.GetImageOriginalBytes(image.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалось завантажити повне зображення:{ex.Message}", "Помилка");
                    return;
                }
            }
            MemoryStream ms = new MemoryStream(image.OriginalBytes);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            Clipboard.SetImage(bitmapImage);
        }
        private void DeleteImageFromStage(object arg)
        {
            if (ownerStage == null) { return; }
            try
            {
                HttpService.RemoveImageFromStage(image.Id, ownerStage.Stage.Id);
            }
            catch
            {
                MessageBox.Show("Помилка", "Не вдалось видалити зображення");
                return;
            }
            ownerStage.Images.Remove(this);
        }
        private void DeleteImage(object arg)
        {
            try
            {
                HttpService.DeleteImage(image.Id);
            }
            catch(Exception e)
            {
                MessageBox.Show($"Не вдалось видалити зображення: {e.Message}", "Помилка");
                return;
            }
            if(photoflowOwner != null)
            {
                photoflowOwner.ImageViewModels.Remove(this);
            }
        }
        private void RenameImage(object arg)
        {
            WindowChangeImageName windowChangeImageName = new WindowChangeImageName(this);
            windowChangeImageName.ShowDialog();
        }
        private void ShowReferencesToPatients(object arg)
        {
            PatientsViewModel patientsViewModel = new PatientsViewModel(PatientListMode.SearchPatientsWithImage, image.Id);
            PatientsView patientsView = new PatientsView();
            patientsView.PatientsViewModel= patientsViewModel;
            WindowContainer windowContainer=new WindowContainer();
            windowContainer.Title = ScreenNames.PATIENTS_WITH_REQUESTED_IMAGE;
            windowContainer.Content = patientsView;
            windowContainer.SizeToContent = SizeToContent.WidthAndHeight;
            windowContainer.Show();
        }
        public Image image;
        StageViewModel ownerStage;
        PhotoflowViewModel photoflowOwner;
        public ImageViewModel(Image imageToSet, StageViewModel ownerToSet=null, PhotoflowViewModel photoflowViewModelToSet = null)
        {
            ownerStage = ownerToSet;
            photoflowOwner = photoflowViewModelToSet;
            image = imageToSet;
            ImageClickedCommand = new RelayCommand(ImageClicked);
            CopyImageCommand = new RelayCommand(CopyImage);
            DeleteImageCommand = new RelayCommand(DeleteImage);
            DeleteImageFromStageCommand = new RelayCommand(DeleteImageFromStage);
            RenameImageCommand = new RelayCommand(RenameImage);
            ShowReferencesCommand = new RelayCommand(ShowReferencesToPatients);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
