using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для ScheduleTimeGridView.xaml
    /// </summary>
    public partial class ScheduleTimeGridView : UserControl
    {
        public List<ScheduleTimeGridElementView> TimeGridElementViews { get; set; }
        public List<TimeGridEmpinessView> emptySlots { get; set; }
        public ScheduleTimeGridElementView SeparatorSelectedElement
        {
            get => separatorSelectedElement;
            set
            {
                if (value == null && separatorSelectedElement != null)
                {
                    separatorSelectedElement.UpdateTimes();
                }
                separatorSelectedElement = value;
            }
        }
        private ScheduleTimeGridElementView separatorSelectedElement = null;
        public ScheduleTimeGridElementView MovementSelectedElement
        {
            get => movementSelectedElement;
            set
            {
                if (value == null && movementSelectedElement!=null)
                {
                    movementSelectedElement.isMovingMode = false;
                    movementSelectedElement.UpdateTimes();
                }
                movementSelectedElement = value;
            }
        }
        private ScheduleTimeGridElementView movementSelectedElement = null;

        public ScheduleForDayView Owner { get; set; }

        public Cabinet Cabinet { get; set; }
        public CabinetColors CabinetColor { get; set; }
        public ScheduleTimeGridView(Cabinet cabinetToSet, ScheduleForDayView ownerToSet, CabinetColors cabinetColorToSet)
        {
            Owner = ownerToSet;
            Cabinet = cabinetToSet;
            TimeGridElementViews = new List<ScheduleTimeGridElementView>();
            DataContext = this;
            emptySlots = new List<TimeGridEmpinessView>(25);
            CabinetColor = cabinetColorToSet;
            InitializeComponent();
            grid.Background = new SolidColorBrush(CabinetColor.CabinetColor);

            foreach (object child in grid.Children)
            {
                if(child is TimeGridEmpinessView empinessView)
                {
                    emptySlots.Add(empinessView);
                    empinessView.PatientSelected += EmpinessView_PatientSelected;
                }
            }
        }

        private void EmpinessView_PatientSelected(object sender, Patient e)
        {
            TimeGridEmpinessView target = sender as TimeGridEmpinessView;
            Schedule newRecord = new Schedule();
            newRecord.PatientId = e.Id;
            newRecord.PatientName = e.Name;
            newRecord.DoctorId = Options.CurrentDoctor.Id;
            newRecord.State = SchedulePatientState.Unknown;
            newRecord.StartDatetime = (Owner.SelectedDate.Date + target.Time).ToString(Options.DateTimePattern);
            newRecord.EndDatetime = (Owner.SelectedDate.Date + target.Time + TimeSpan.FromMinutes(30)).ToString(Options.DateTimePattern);
            newRecord.CabinetId = Cabinet.Id;

            Owner.Owner.TcpClient.AddRecord(newRecord);
        }

        private void Emptiness_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }
        public void TryChangeEndTimeToEarlier(TimeGridEmpinessView target)
        {
            if(target.Time < SeparatorSelectedElement.ScheduleEndTime - TimeSpan.FromMinutes(30) && target.Time> SeparatorSelectedElement.ScheduleStartTime - TimeSpan.FromMinutes(30))
            {
                SeparatorSelectedElement.ScheduleEndTime = target.Time + TimeSpan.FromMinutes(30);
            }
        }

        internal void UpdateRecordState(int recordId, SchedulePatientState newState)
        {

            ScheduleTimeGridElementView target = TimeGridElementViews.FirstOrDefault(e => e.Schedule.Id == recordId);
            if (target != null)
            {
                target.UpdateRecordState(newState);
            }
        }

        public void TryChangeEndTimeLater(TimeGridEmpinessView target)
        {
            if (target.Time >= SeparatorSelectedElement.ScheduleEndTime)
            {
                SeparatorSelectedElement.ScheduleEndTime = target.Time + TimeSpan.FromMinutes(30);
            }
        }

        internal void DeleteRecord(int recordId)
        {
            ScheduleTimeGridElementView target = TimeGridElementViews.FirstOrDefault(e => e.Schedule.Id == recordId);
            if (target != null)
            {
                TimeGridElementViews.Remove(target);
                grid.Children.Remove(target);
            }
        }

        private void Emptiness_MouseMove(object sender, MouseEventArgs e)
        {
            if (SeparatorSelectedElement != null)
            {
                TimeGridEmpinessView target = sender as TimeGridEmpinessView;
                TryChangeEndTimeLater(target);
            }
            else if(MovementSelectedElement != null)
            {
                MovementSelectedElement.MoveRecordListener(e.GetPosition(this));
            }
        }

        private void Emptiness_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        internal void LoadSchedule(DateTime date)
        {
            ClearTimeGrid();

            List<Schedule> schedules = HttpService.GetSchedule(date, Cabinet.Id.ToString());
            foreach(Schedule s in schedules)
            {
                TimeGridElementViews.Add(new ScheduleTimeGridElementView(s, this));
            }
            PlaceRecords();
        }

        private void ClearTimeGrid()
        {
            foreach(ScheduleTimeGridElementView timeGridElementView in TimeGridElementViews)
            {
                if(timeGridElementView.Parent != null)
                {
                    grid.Children.Remove(timeGridElementView);
                }
            }
            TimeGridElementViews.Clear();

        }

        private void PlaceRecords()
        {
            foreach(ScheduleTimeGridElementView scheduleTimeGridElementView in TimeGridElementViews)
            {
                grid.Children.Add(scheduleTimeGridElementView);
            }
        }

        internal void UpdateRecordComment(int recordId, string newComment)
        {
            ScheduleTimeGridElementView target = TimeGridElementViews.FirstOrDefault(e => e.Schedule.Id == recordId);
            if (target != null)
            {
                target.UpdateRecordComment(newComment);
            }
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SeparatorSelectedElement = null;
        }

        private void grid_MouseLeave(object sender, MouseEventArgs e)
        {
            SeparatorSelectedElement = null;
            MovementSelectedElement = null;
        }
    }
}
