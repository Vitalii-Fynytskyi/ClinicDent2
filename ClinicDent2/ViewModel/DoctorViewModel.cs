using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
namespace ClinicDent2.ViewModel
{
    public class DoctorViewModel:BaseViewModel
    {
        private Doctor doctor;
        public DoctorViewModel(Doctor doctorToSet)
        {
            doctor= doctorToSet;
        }
        public int Id
        {
            get
            {
                return doctor.Id;
            }
            set
            {
                if(doctor.Id != value)
                {
                    doctor.Id = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Name
        {
            get
            {
                return doctor.Name;
            }
            set
            {
                if (doctor.Name != value)
                {
                    doctor.Name = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
