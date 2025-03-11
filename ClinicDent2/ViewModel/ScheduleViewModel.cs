using ClinicDentClientCommon;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using System;

namespace ClinicDent2.ViewModel
{
    public class ScheduleViewModel:BaseViewModel
    {
        public Schedule schedule;
        public ScheduleViewModel(Schedule scheduleToSet)
        {
            schedule = scheduleToSet;
            startDateTimeDT = DateTime.ParseExact(scheduleToSet.StartDatetime, SharedData.DateTimePattern, null);
            endDateTimeDT = DateTime.ParseExact(scheduleToSet.EndDatetime, SharedData.DateTimePattern, null);

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
        private DateTime endDateTimeDT;
        public DateTime EndDateTimeDT
        {
            get
            {
                return endDateTimeDT;
            }
            set
            {
                if (value != endDateTimeDT)
                {
                    endDateTimeDT = value;
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
        public string Comment
        {
            get
            {
                return schedule.Comment;
            }
            set
            {
                if(value != schedule.Comment)
                {
                    schedule.Comment = value;
                    NotifyPropertyChanged();
                    Options.MainWindow.mainMenu.TcpClient.UpdateRecordComment(schedule.Id, value);
                    Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordCommentUpdated, (schedule.Id, value, schedule.StartDatetime, schedule.CabinetId));
                }
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
