using ClinicDent2.Commands;
using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public enum ImageType
    {
        Undefined=0,
        Regular =1,
        XRay=2,
        All = 3,

    }
    public class PhotoflowViewModel :BaseViewModel
    {
        private ImageType imageType = ImageType.All;
        public ImageType ImageType
        {
            get
            {
                return imageType;
            }
            set
            {
                if (imageType != value)
                {
                    imageType = value;
                    SelectedPage = 1;
                    ReceiveImages();

                }
                NotifyPropertyChanged();
            }
        }
        public List<ImageType> ImageTypes { get; set; } = Enum.GetValues(typeof(ImageType)).Cast<ImageType>().ToList();
        public PhotoflowViewModel()
        {
            PageChangedCommand = new RelayCommand(pageChanged);
            selectedPage = 1;
            visiblePages = new ObservableCollection<int>();
            DoctorViewModels = new ObservableCollection<DoctorViewModel>(Options.AllDoctors.Select(d => new DoctorViewModel(d)));
            SelectedDoctor = DoctorViewModels.FirstOrDefault(d => d.Id == Options.CurrentDoctor.Id);

        }
        public bool IsAnyImageSelected
        {
            get
            {
                return imageViewModels.Any(i => i.IsSelected == true);
            }
        }
        private DoctorViewModel selectedDoctor;
        public DoctorViewModel SelectedDoctor
        {
            get
            {
                return selectedDoctor;
            }
            set
            {
                if(selectedDoctor != value)
                {
                    selectedDoctor= value;
                    SelectedPage = 1;
                    NotifyPropertyChanged(nameof(SelectedDoctor));
                    NotifyPropertyChanged(nameof(CanAddPhotos));
                    ReceiveImages();
                }
            }
        }
        public bool CanAddPhotos
        {
            get
            {
                return SelectedDoctor.Id == Options.CurrentDoctor.Id;
            }
        }
        public void AddPhotos(string[] fileNames)
        {
            foreach (string imagePath in fileNames)
            {
                Model.Image image = new Model.Image();

                image.OriginalBytes = File.ReadAllBytes(imagePath);
                image.FileName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                image.DoctorId = Options.CurrentDoctor.Id;
                try
                {
                    image = HttpService.PostImage(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалось додати нове зображення: {ex.Message}", "Помилка");
                    return;
                }
                if (Options.CanDeleteImage)
                    File.Delete(imagePath);
                if(imageType == ImageType.All || (imageType == ImageType.XRay && image.IsXRay == true) || (imageType == ImageType.Regular && image.IsXRay == false) || image.IsXRay==null)
                {
                    ImageViewModels.Insert(0, new ImageViewModel(image, photoflowViewModelToSet: this));
                    if(image.IsXRay == null)
                    {
                        MessageBox.Show("Не вдалось визначити точний тип зображення. Отримано статус Undefined");
                    }
                }
            }
        }
        private ObservableCollection<DoctorViewModel> doctorViewModels;
        public ObservableCollection<DoctorViewModel> DoctorViewModels
        {
            get
            {
                return doctorViewModels;
            }
            set
            {
                if(doctorViewModels!= value)
                {
                    doctorViewModels = value;
                }
            }
        }
        private ObservableCollection<int> visiblePages;
        public ObservableCollection<int> VisiblePages
        {
            get
            {
                return visiblePages;
            }
            set
            {
                if (visiblePages != value)
                {
                    visiblePages = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private ObservableCollection<ImageViewModel> imageViewModels;
        public ObservableCollection<ImageViewModel> ImageViewModels
        {
            get { return imageViewModels; }
            set
            {
                if (imageViewModels != value)
                {
                    imageViewModels = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private int countPages;
        public int CountPages
        {
            get { return countPages; }
            set
            {
                if (countPages != value)
                {
                    countPages = value;
                    if (SelectedPage > CountPages)
                    {
                        SelectedPage = CountPages;
                    }
                    NotifyPropertyChanged();
                }
            }
        }
        private int selectedPage;
        public int SelectedPage
        {
            get { return selectedPage; }
            set
            {
                if (selectedPage != value)
                {
                    selectedPage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RelayCommand PageChangedCommand { get; set; }
        private void pageChanged(object param) //param is page in string format
        {
            SelectedPage = Convert.ToInt32(param.ToString());
            ReceiveImages();
        }
        private void createVisiblePages()
        {
            if (CountPages <= 1)
            {
                VisiblePages.Clear();
                return;
            }
            List<int> pages = new List<int>(15);
            int pageToAdd = SelectedPage;
            pages.Add(pageToAdd);
            pageToAdd--;
            while (pageToAdd > 0 && pageToAdd >= SelectedPage - 7)
            {
                pages.Insert(0, pageToAdd);
                pageToAdd--;
            }
            pageToAdd = SelectedPage + 1;
            while (CountPages >= pageToAdd && pageToAdd <= SelectedPage + 7)
            {
                pages.Add(pageToAdd);
                pageToAdd++;
            }
            VisiblePages = new ObservableCollection<int>(pages);
        }
        public void ReceiveImages()
        {
            ImagesToClient imagesToClient= new ImagesToClient();
            try
            {
                imagesToClient = HttpService.GetImages(selectedPage, Options.PhotosPerPage, SelectedDoctor.Id, imageType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось завантажити зображення: {ex.Message}", "Помилка");
                return;
            }
            CountPages = imagesToClient.CountPages;
            ImageViewModels = new ObservableCollection<ImageViewModel>(imagesToClient.Images.Select(i => new ImageViewModel(i, photoflowViewModelToSet: this)));
            createVisiblePages();
        }
    }
}
