using ClinicDent2.Commands;
using ClinicDent2.Model;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicDent2.ViewModel
{
    public class PatientViewModel : BaseViewModel
    {
        private Patient patient;
        public Patient Patient
        { 
            get
            {
                return patient; 
            }
            set
            {
                if(patient != value)
                {
                    patient = value;
                    NotifyPropertyChanged(nameof(Patient), nameof(PatientId), nameof(Name), nameof(Gender), nameof(Phone), nameof(Address), nameof(Birthdate), nameof(Illness), nameof(Notes), nameof(RegisterDate), nameof(Statuses), nameof(CurePlan), nameof(ImageBytes));
                }
            }
        }
        public int PatientId
        {
            get
            {
                return patient.Id;
            }
            set
            {
                if(patient.Id != value)
                {
                    patient.Id = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Name
        {
            get
            {
                return patient.Name;
            }
            set
            {
                if(patient.Name != value)
                {
                    patient.Name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Gender
        {
            get
            {
                return patient.Gender;
            }
            set
            {
                if(patient.Gender != value)
                {
                    patient.Gender = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Phone
        {
            get
            {
                return patient.Phone;
            }
            set
            {
                if(patient.Phone != value)
                {
                    patient.Phone = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Address
        {
            get
            {
                return patient.Address;
            }
            set
            {
                if(patient.Address != value)
                {
                    patient.Address = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Birthdate
        {
            get
            {
                return patient.Birthdate;
            }
            set
            {
                if(patient.Birthdate != value)
                {
                    patient.Birthdate = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Illness
        {
            get
            {
                return patient.Illness;
            }
            set
            {
                if(patient.Illness != value)
                {
                    patient.Illness = value;
                    NotifyPropertyChanged();
                }
                
            }
        }
        public string Notes
        {
            get
            {
                return patient.Notes;
            }
            set
            {
                if(patient.Notes != value)
                {
                    patient.Notes = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string RegisterDate
        {
            get
            {
                return patient.RegisterDate;
            }
            set
            {
                if(patient.RegisterDate != value)
                {
                    patient.RegisterDate = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Statuses
        {
            get
            {
                return patient.Statuses;
            }
            set
            {
                if(patient.Statuses != value)
                {
                    patient.Statuses = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string CurePlan
        {
            get
            {
                return patient.CurePlan;
            }
            set
            {
                if(patient.CurePlan != value)
                {
                    patient.CurePlan = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public byte[] ImageBytes
        {
            get
            {
                return patient.ImageBytes;
            }
            set
            {
                if(patient.ImageBytes != value)
                {
                    patient.ImageBytes = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RelayCommand CopyImageCommand { get; set; }
        public RelayCommand RemoveImageCommand { get; set; }
        public PatientViewModel(Patient patientToSet)
        {
            CopyImageCommand = new RelayCommand(copyImage);
            RemoveImageCommand = new RelayCommand(removeImage);
            patient = patientToSet;
        }
        private void copyImage(object param)
        {
            MemoryStream ms = new MemoryStream(ImageBytes);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            Clipboard.SetImage(bitmapImage);
        }
        private void removeImage(object param)
        {
            ImageBytes = null;
        }
        public void PostPatient()
        {
            patient = HttpService.PostPatient(patient);
            NotifyPropertyChanged(nameof(PatientId));
        }
        public void PutPatient()
        {
            patient.LastModifiedDateTime = HttpService.PutPatient(patient);
        }
        public void RemovePatient()
        {
            HttpService.DeletePatient(patient.Id);
        }
    }
}
