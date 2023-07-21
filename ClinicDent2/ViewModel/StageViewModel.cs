using ClinicDent2.Commands;
using ClinicDent2.Model;
using ClinicDent2.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicDent2.ViewModel
{
    public class StageViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        #region IDataErrorInfo_Implementation
        protected string error = string.Empty;
        public string Error => error;
        public string this[string propertyName]
        {
            get
            {
                error= string.Empty;
                if(propertyName == nameof(StageDatetime))
                {
                    bool isValid = DateTime.TryParseExact(StageDatetime, Options.DateTimePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
                    if(isValid == false)
                    {
                        error = $"Дата має бути в форматі {Options.DateTimePattern}. Етап не буде збережено";
                    }
                }
                return error;
            }
        }
        #endregion
        public static List<StageAsset> Operations { get; set; }
        public static List<StageAsset> Bonds { get; set; }
        public static List<StageAsset> CanalMethods { get; set; }
        public static List<StageAsset> Cements { get; set; }
        public static List<StageAsset> Calciums { get; set; }
        public static List<StageAsset> Dentins { get; set; }
        public static List<StageAsset> Enamels { get; set; }
        public static List<StageAsset> Pins { get; set; }
        public static List<StageAsset> Sealers { get; set; }
        public static List<StageAsset> Technicians { get; set; }
        public RelayCommand DeleteStageCommand { get; set; }
        public RelayCommand SendStageViaViberCommand { get; set; }
        public RelayCommand AddImageFromClipboardCommand { get; set; }
        public RelayCommand AddImageFromDiskCommand { get; set; }
        public RelayCommand AttachImageCommand { get; set; }


        private void AddImageFromClipboard(object arg)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource bitmapSource = Clipboard.GetImage();
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                BitmapFrame outputFrame = BitmapFrame.Create(bitmapSource);
                encoder.Frames.Add(outputFrame);
                MemoryStream memoryStream = new MemoryStream();
                encoder.Save(memoryStream);
                Model.Image imageToStage = new Model.Image();
                imageToStage.OriginalBytes = memoryStream.ToArray();
                imageToStage.FileName = "Image from clipboard";
                imageToStage.DoctorId = Options.CurrentDoctor.Id;
                try
                {
                    imageToStage = HttpService.PostImage(imageToStage);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Не вдалось завантажити зображення:{ex.Message}", "Помилка");
                    return;
                }
                try
                {
                    HttpService.AddImageToStage(imageToStage.Id, stage.Id);
                }
                catch (Exception ex)
                { 
                    MessageBox.Show($"Зображення в фотопотоці, але не вдалось прикріпити зображення до роботи: {ex.Message}", "Помилка");
                    return;
                }
                Images.Add(new ImageViewModel(imageToStage, this));
            }
            else
            {
                MessageBox.Show("Нема зображення в буфері обміну", "Помилка");
            }
        }
        private void DeleteStage(object arg)
        {
            try
            {
                HttpService.DeleteStage(stage.Id);
            }
            catch
            {
                MessageBox.Show("Помилка", "Не вдалось видалити етап");
                return;
            }
            owner.Stages.Remove(this);
        }
        private void SendStageViaViber(object arg)
        {
            string phone = arg as string;
            if (String.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show($"Номер пустий");
                return;
            }
            if (phone.Length == 10)
            {
                phone = "38" + phone;
            }
            else if(phone.StartsWith("38") == false || phone.Length != 12)
            {
                MessageBox.Show($"Номер '{arg}' не є в правильному форматі");
                return;
            }
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Надіслати етап через Viber?", "Надсилання", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes) { return; }
            ViberController viberController = new ViberController();
            try
            {
                viberController.SendToUser(phone, this);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void AddImageFromDisk(object arg)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.tif, *.tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|All files(*.*)|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string imagePath in openFileDialog.FileNames)
                {
                    Model.Image imageToStage = new Model.Image();
                    imageToStage.OriginalBytes = File.ReadAllBytes(imagePath);
                    imageToStage.FileName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                    imageToStage.DoctorId = Options.CurrentDoctor.Id;
                    if (Options.CanDeleteImage)
                        File.Delete(imagePath);
                    imageToStage = HttpService.PostImage(imageToStage);
                    if (imageToStage == null)
                    {
                        MessageBox.Show("Помилка", "Не вдалось завантажити зображення");
                        return;
                    }
                    try
                    {
                        HttpService.AddImageToStage(imageToStage.Id, stage.Id);
                    }
                    catch
                    {
                        MessageBox.Show("Помилка", "Не вдалось прикріпити зображення до роботи");
                        return;
                    }
                    Images.Add(new ImageViewModel(imageToStage, this));
                }
            }
        }
        private void AttachImage(object arg)
        {
            WindowContainer windowContainer= new WindowContainer();
            windowContainer.Title = "Додати зображення до етапу";
            windowContainer.Content = new SelectImagesView(this);
            windowContainer.ShowDialog();
        }
        public void AttachImages(IEnumerable<ImageViewModel> images)
        {
            foreach(ImageViewModel i in images)
            {
                try
                {
                    HttpService.AddImageToStage(i.Id, stage.Id);
                    Images.Add(i);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалось додати зображення до етапу: {ex.Message}", "Помилка");
                    return;
                }
            }
        }

        public string Title
        {
            get
            {
                return stage.Title;
            }
            set
            {
                stage.Title = value;
                OnPropertyChanged();
            }
        }
        public string StageDatetime
        {
            get
            {
                return stage.StageDatetime;
            }
            set
            {
                stage.StageDatetime = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Operation
        {
            get
            {
                return stage.Operation;
            }
            set
            {
                stage.Operation = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Cement
        {
            get
            {
                return stage.Cement;       
            }
            set
            {
                stage.Cement = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Calcium
        {
            get
            {
                return stage.Calcium;
            }
            set
            {
                stage.Calcium = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Bond
        {
            get
            {
                return stage.Bond;
            }
            set
            {
                stage.Bond = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Pin
        {
            get
            {
                return stage.Pin;
            }
            set
            {
                stage.Pin = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Dentin
        {
            get
            {
                return stage.Dentin;
            }
            set
            {
                stage.Dentin = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Enamel
        {
            get
            {
                return stage.Enamel;
            }
            set
            {
                stage.Enamel = value;
                OnPropertyChanged();
            }
        }
        public StageAsset CanalMethod
        {
            get
            {
                return stage.CanalMethod;
            }
            set
            {
                stage.CanalMethod = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Sealer
        {
            get
            {
                return stage.Sealer;
            }
            set
            {
                stage.Sealer = value;
                OnPropertyChanged();
            }
        }
        public StageAsset Technician
        {
            get
            {
                return stage.Technician;
            }
            set
            {
                stage.Technician = value;
                OnPropertyChanged();
            }
        }
        public string DoctorName
        {
            get
            {
                return stage.DoctorName;
            }
            set
            {
                stage.DoctorName = value;
                OnPropertyChanged();
            }
        }
        public string CommentText
        {
            get
            {
                return stage.CommentText;
            }
            set
            {
                stage.CommentText = value;
                OnPropertyChanged();
            }
        }
        public int Payed
        {
            get
            {
                return stage.Payed;
            }
            set
            {
                stage.Payed = value;
                UpdateImagePaymentStatus();
                OnPropertyChanged();
            }
        }

        private void UpdateImagePaymentStatus()
        {
            if(paymentStatusImagePath == "..\\assets/images/OK.png" && Price != Payed)
            {
                OnPropertyChanged("PaymentStatusImagePath");
            }
            else if(paymentStatusImagePath == "..\\assets/images/WRONG.png" && Price == Payed)
            {
                OnPropertyChanged("PaymentStatusImagePath");
            }
        }

        public int Price
        {
            get
            {
                return stage.Price;
            }
            set
            {
                stage.Price = value;
                UpdateImagePaymentStatus();
                OnPropertyChanged();
            }
        }
        public bool IsOwner
        {
            get
            {
                return DoctorId == Options.CurrentDoctor.Id;
            }
        }
        private string paymentStatusImagePath = "..\\assets/images/OK.png";
        public string PaymentStatusImagePath
        {
            get
            {
                if (Price != Payed)
                {
                    paymentStatusImagePath = "..\\assets/images/WRONG.png";
                    return paymentStatusImagePath;
                }
                else
                {
                    paymentStatusImagePath = "..\\assets/images/OK.png";
                    return paymentStatusImagePath;
                }
            }
        }
        private ObservableCollection<ImageViewModel> images;
        public ObservableCollection<ImageViewModel> Images
        {
            get
            {
                return images;
            }
            set
            {
                images = value;
                OnPropertyChanged();
            }
        }
        public int DoctorId
        {
            get
            {
                return stage.DoctorId;
            }
        }
        public string BoundBackground { get; set; }
        public Stage Stage
        {
            get
            {
                return stage;
            }
        }
        private Stage stage;
        public static void LoadAssets()
        {
            List<StageAsset> allAssets = HttpService.GetStageAssets();
            Operations = new List<StageAsset>();
            Bonds = new List<StageAsset>();
            Enamels = new List<StageAsset>();
            Dentins = new List<StageAsset>();
            CanalMethods = new List<StageAsset>();
            Sealers = new List<StageAsset>();
            Cements = new List<StageAsset>();
            Technicians = new List<StageAsset>();
            Pins = new List<StageAsset>();
            Calciums = new List<StageAsset>();
            foreach(StageAsset asset in allAssets)
            {
                switch (asset.Type)
                {
                    case AssetType.Bond:
                        Bonds.Add(asset);
                        break;
                    case AssetType.Dentin:
                        Dentins.Add(asset);
                        break;
                    case AssetType.Enamel:
                        Enamels.Add(asset);
                        break;
                    case AssetType.CanalMethod:
                        CanalMethods.Add(asset);
                        break;
                    case AssetType.Sealer:
                        Sealers.Add(asset);
                        break;
                    case AssetType.Cement:
                        Cements.Add(asset);
                        break;
                    case AssetType.Technician:
                        Technicians.Add(asset);
                        break;
                    case AssetType.Pin:
                        Pins.Add(asset);
                        break;
                    case AssetType.Operation:
                        Operations.Add(asset);
                        break;
                    case AssetType.Calcium:
                        Calciums.Add(asset);
                        break;
                }
            }
        }

        public StageViewModel(Stage stageToSet, StagesViewModel ownerToSet)
        {
            owner = ownerToSet;
            stage = stageToSet;
            if(ownerToSet.ScheduleRecordViewModel != null)
            {
                if(ownerToSet.ScheduleRecordViewModel.Id == stageToSet.ScheduleId)
                {
                    BoundBackground = "PaleGreen";
                }
                else
                {
                    BoundBackground = "Transparent";

                }
            }
            else
            {
                BoundBackground = "Transparent";

            }
            Images = new ObservableCollection<ImageViewModel>(HttpService.GetImagesForStage(stageToSet.Id).Select(i => new ImageViewModel(i, this)));

            DeleteStageCommand = new RelayCommand(DeleteStage);
            SendStageViaViberCommand = new RelayCommand(SendStageViaViber);
            AddImageFromDiskCommand = new RelayCommand(AddImageFromDisk);
            AttachImageCommand = new RelayCommand(AttachImage);
            AddImageFromClipboardCommand = new RelayCommand(AddImageFromClipboard);
        }
        private StagesViewModel owner;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
