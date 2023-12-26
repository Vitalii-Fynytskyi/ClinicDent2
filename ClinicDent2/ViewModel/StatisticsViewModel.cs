using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public class StatisticsViewModel:BaseViewModel
    {
        public StatisticsViewModel()
        {
            int cabinetId = Options.DefaultSelectedCabinet.Id;
            futureWorkingHours = TimeSpan.FromMinutes(HttpService.GetFutureWorkingMinutes(cabinetId));
            futureUniquePatients = HttpService.GetFutureUniquePatients(cabinetId);

        }
        public TimeSpan FutureWorkingHours
        {
            get
            {
                return futureWorkingHours;
            }
            set
            {
                if(futureWorkingHours != value)
                {
                    futureWorkingHours = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(FutureWorkingHoursStr));
                }
            }
        }
        private TimeSpan futureWorkingHours;
        public string FutureWorkingHoursStr
        {
            get
            {
                return $"Робочого часу в '{Options.DefaultSelectedCabinet.CabinetName}' записано на " +futureWorkingHours.TotalHours + " годин";
            }
        }
        public int FutureUniquePatients
        {
            get
            {
                return futureUniquePatients;
            }
            set
            {
                if(futureUniquePatients != value)
                {
                    futureUniquePatients = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(FutureUniquePatientsStr));

                }
            }
        }
        private int futureUniquePatients;
        public string FutureUniquePatientsStr
        {
            get
            {
                return $"Унікальних пацієнтів в розкладі на '{Options.DefaultSelectedCabinet.CabinetName}': {FutureUniquePatients}";
            }
        }
    }
}
