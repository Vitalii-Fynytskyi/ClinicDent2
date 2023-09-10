using ClinicDent2.Model;
using System;

namespace ClinicDent2.ViewModel
{
    public class ScheduleViewModel:BaseViewModel
    {
        public Schedule schedule;
        public ScheduleViewModel(Schedule scheduleToSet)
        {
            schedule = scheduleToSet;
            startDateTimeDT = DateTime.ParseExact(scheduleToSet.StartDatetime, Options.DateTimePattern, null);
        }
        public string StartDateTime
        {
            get
            {
                return schedule.StartDatetime;
            }
            set
            {
                if(value != schedule.StartDatetime)
                {
                    schedule.StartDatetime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private DateTime startDateTimeDT;
        public DateTime StartDateTimeDT
        {
            get
            {
                return startDateTimeDT;
            }
            set
            {
                if (value != startDateTimeDT)
                {
                    startDateTimeDT = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Id
        {
            get
            {
                return schedule.Id;
            }
        }
        public int CabinetId
        {
            get
            {
                return schedule.CabinetId;
            }
        }
        public string CabinetName
        {
            get
            {
                return schedule.CabinetName;
            }
        }
    }
}
