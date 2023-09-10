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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicDent2.ViewModel
{
    public class StageViewModel : BaseViewModel, IDataErrorInfo
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
        public RelayCommand MarkSentViaMessagerCommand { get; set; }
        public RelayCommandAsync SendStageViaViberCommand { get; set; }
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
        private async Task SendStageViaTelegram(object arg)
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
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Надіслати етап через Telegram?", "Надсилання", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes) { return; }
            try
            {
                await TelegramMessageSender.SendStageAsync(owner.Patient.Name, phone, this);
                MarkSentViaMessagerCommand?.Execute("1");
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
                if(stage.Title != value)
                {
                    stage.Title = value;
                    ViewModelStatus = ViewModelStatus.Updated;
                    NotifyPropertyChanged();
                }
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
                if(stage.StageDatetime != value)
                {
                    stage.StageDatetime = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
            }
        }
        public bool IsSentViaViber
        {
            get
            {
                return stage.IsSentViaViber;
            }
            set
            {
                if(stage.IsSentViaViber != value)
                {
                    stage.IsSentViaViber = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Operation != value)
                {
                    stage.Operation = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
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
                if(stage.Cement != value)
                {
                    stage.Cement = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Calcium != value)
                {
                    stage.Calcium = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Bond != value)
                {
                    stage.Bond = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Pin != value)
                {
                    stage.Pin = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Dentin != value)
                {
                    stage.Dentin = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Enamel != value)
                {
                    stage.Enamel = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.CanalMethod != value)
                {
                    stage.CanalMethod = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
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
                if(stage.Sealer != value)
                {
                    stage.Sealer = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Technician != value)
                {
                    stage.Technician = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.DoctorName != value)
                {
                    stage.DoctorName = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.CommentText != value)
                {
                    stage.CommentText = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    NotifyPropertyChanged();
                }
                
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
                if(stage.Payed != value)
                {
                    stage.Payed = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    UpdateImagePaymentStatus();
                    NotifyPropertyChanged();
                }
            }
        }

        private void UpdateImagePaymentStatus()
        {
            if(paymentStatusImagePath == "..\\assets/images/OK.png" && Price != Payed)
            {
                NotifyPropertyChanged("PaymentStatusImagePath");
            }
            else if(paymentStatusImagePath == "..\\assets/images/WRONG.png" && Price == Payed)
            {
                NotifyPropertyChanged("PaymentStatusImagePath");
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
                if(stage.Price != value)
                {
                    stage.Price = value;
                    ViewModelStatus = ViewModelStatus.Updated;

                    UpdateImagePaymentStatus();
                    NotifyPropertyChanged();
                }
                
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
                if(images != value)
                {
                    images = value;
                    NotifyPropertyChanged();
                }
                
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
            if(ownerToSet.MarkedDate != null)
            {
                if(ownerToSet.MarkedDate == DateTime.ParseExact(stageToSet.StageDatetime,Options.DateTimePattern,null).Date)
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
            SendStageViaViberCommand = new RelayCommandAsync(SendStageViaTelegram);
            AddImageFromDiskCommand = new RelayCommand(AddImageFromDisk);
            AttachImageCommand = new RelayCommand(AttachImage);
            AddImageFromClipboardCommand = new RelayCommand(AddImageFromClipboard);
            MarkSentViaMessagerCommand = new RelayCommand(MarkSentViaMessager);
        }

        private void MarkSentViaMessager(object obj)
        {
            int mark = Convert.ToInt32(obj as string);
            HttpService.StageMarkSentViaMessager(stage.Id, mark);
            IsSentViaViber = Convert.ToBoolean(mark);
        }

        private StagesViewModel owner;
    }
}
