﻿using ClinicDent2.Model;
using ClinicDent2.RequestAnswers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для ScheduleTimeGridView.xaml
    /// </summary>
    public partial class ScheduleTimeGridView : UserControl, INotifyPropertyChanged
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

        public string CabinetComment
        {
            get { return cabinetComment; }
            set
            {
                if(value != cabinetComment)
                {
                    cabinetComment = value;
                    Owner.Owner.TcpClient.UpdateCabinetComment(Owner.SelectedDate.ToString(Options.DateTimePattern), Cabinet.Id, cabinetComment);
                    OnPropertyChanged();
                }
            }
        }
        public string cabinetComment;
        public string DayMoneySummary
        {
            get
            {
                if(Owner.SelectedDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    return $"День: {PayedSum}/{PriceSum} грн.";
                }
                else
                {
                    int weekPayedSum = PayedSum;
                    int weekPriceSum = PriceSum;
                    if (Owner.SelectedDate.Day < 7) //request to server required
                    {
                        WeekMoneySummaryRequestAnswer weekMoneySummaryRequestAnswer = null;
                        try
                        {
                            weekMoneySummaryRequestAnswer = HttpService.GetWeekMoneySummary(Cabinet.Id, Owner.SelectedDate);
                            weekPayedSum = weekMoneySummaryRequestAnswer.PaidSum;
                            weekPriceSum = weekMoneySummaryRequestAnswer.PriceSum;

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return $"День: {PayedSum}/{PriceSum} грн.(Помилка підсумку тижня)";
                        }
                    }
                    else //calculate on client
                    {
                        //if day = 7: i=5;i>-1
                        for(int i = Owner.SelectedDate.Day-2;i != Owner.SelectedDate.Day - 8; i--) //start at saturday
                        {
                            weekPayedSum += Owner.Owner.ScheduleForDayViews[i].TimeGrids.First(g => g.Cabinet == Cabinet).PayedSum;
                            weekPriceSum += Owner.Owner.ScheduleForDayViews[i].TimeGrids.First(g => g.Cabinet == Cabinet).PriceSum;
                        }
                    }
                    return $"День: {PayedSum}/{PriceSum} грн.{Environment.NewLine}Тиждень: {weekPayedSum}/{weekPriceSum} грн.";
                }
            }
        }
        public int PayedSum
        {
            get
            {
                return payedSum;
            }
            set
            {
                if(payedSum!= value)
                {
                    payedSum = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DayMoneySummary));

                }
            }
        }
        public int payedSum;
        public int PriceSum
        {
            get
            {
                return priceSum;
            }
            set
            {
                if (priceSum != value)
                {
                    priceSum = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DayMoneySummary));

                }
            }
        }
        public int priceSum;


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

            ScheduleRecordsForDayInCabinet schedules = HttpService.GetSchedule(date, Cabinet.Id.ToString());
            foreach(Schedule s in schedules.Schedules)
            {
                TimeGridElementViews.Add(new ScheduleTimeGridElementView(s, this));
            }
            OnPropertyChanged(nameof(DayMoneySummary));
            cabinetComment = schedules.CabinetComment;
            OnPropertyChanged(nameof(CabinetComment));
            PlaceRecords();
        }

        private void ClearTimeGrid()
        {
            cabinetComment=null;
            OnPropertyChanged(nameof(CabinetComment));
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
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
