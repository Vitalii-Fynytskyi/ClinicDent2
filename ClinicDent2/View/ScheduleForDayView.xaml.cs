using ClinicDent2.Model;
using ClinicDent2.RequestAnswers;
using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2.View
{
    public partial class ScheduleForDayView : UserControl, INotifyPropertyChanged
    {
        private static CabinetColors[] CabinetColors;
        private bool isControlEnabled = true;
        public bool IsControlEnabled
        {
            get
            {
                return isControlEnabled;
            }
            set
            {
                isControlEnabled = value;
            }
        }
        public ScheduleMenuView Owner { get; set; }
        public List<ScheduleTimeGridView> TimeGrids { get; set; }
        public static List<Cabinet> Cabinets { get; set; }
        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get
            {
                return selectedDate;
            }
            set
            {
                selectedDate = value;
                OnPropertyChanged();
                OnPropertyChanged("DayOfWeek");
                OnPropertyChanged("AboutDay");
                OnPropertyChanged("SelectedDateString");
                if(isControlEnabled == true)
                    loadSchedule(selectedDate);
            }
        }
        static ScheduleForDayView()
        {
            Cabinets = HttpService.GetCabinets();
            CabinetColors = new CabinetColors[3];

            CabinetColors[0] = new CabinetColors();
            CabinetColors[0].CabinetColor = Colors.SkyBlue;
            CabinetColors[0].ElementColor = Colors.DodgerBlue;
            CabinetColors[0].ElementColorHovered = Colors.RoyalBlue;



            CabinetColors[1] = new CabinetColors();
            CabinetColors[1].CabinetColor = Colors.MediumPurple;
            CabinetColors[1].ElementColor = Colors.DarkOrchid;
            CabinetColors[1].ElementColorHovered = Colors.Purple;


            CabinetColors[2] = new CabinetColors();
            CabinetColors[2].CabinetColor = Colors.LightGreen;
            CabinetColors[2].ElementColor = Colors.MediumSeaGreen;
            CabinetColors[2].ElementColorHovered = Colors.DarkGreen;
        }
        public ScheduleForDayView(int year, int month, int day, ScheduleMenuView ownerToSet)
        {
            Owner = ownerToSet;
            InitializeComponent();
            DataContext = this;
            //TODO check error when there is no 31 day in month
            DateTime newDateTime;
            try
            {
                newDateTime = new DateTime(year, month, day);
            }
            catch
            {
                newDateTime = DateTime.Now;
                IsControlEnabled = false;
                Visibility = Visibility.Collapsed;
            }


            TimeGrids = new List<ScheduleTimeGridView>();
            
            for(int i = 0; i < Cabinets.Count; i++)
            {
                ScheduleTimeGridView scheduleTimeGridView = new ScheduleTimeGridView(Cabinets[i], this, CabinetColors[i]);
                scheduleTimeGridView.Margin = new Thickness(15, 0, 0, 0);
                TimeGrids.Add(scheduleTimeGridView);
                timeGridsPanel.Children.Add(scheduleTimeGridView);
            }
            SelectedDate = newDateTime;
        }
        public ScheduleForDayView()
        {
            InitializeComponent();
        }
        private void loadSchedule(DateTime selectedDate)
        {
            daySummaryList.Clear();
            weekSummaryList.Clear();
            DaySummaryPatientsId.Clear();
            foreach (ScheduleTimeGridView timeGridView in TimeGrids)
            {
                timeGridView.LoadSchedule(selectedDate.Date);
            }
            OnPropertyChanged(nameof(DaySummaryList));
            if (SelectedDate.DayOfWeek == System.DayOfWeek.Sunday)
            {
                GenerateWeekSummary();
            }
            OnPropertyChanged(nameof(WeekSummaryList));
        }
        private void GenerateWeekSummary()
        {
            if (SelectedDate.Day < 7) //request to server required
            {
                WeekMoneySummaryRequestAnswer weekMoneySummaryRequestAnswer = null;
                try
                {
                    weekMoneySummaryRequestAnswer = HttpService.GetWeekMoneySummary(SelectedDate);
                    AddToWeekSummary(weekMoneySummaryRequestAnswer.DoctorIds, weekMoneySummaryRequestAnswer.StagesPriceSum, weekMoneySummaryRequestAnswer.StagesPaidSum, weekMoneySummaryRequestAnswer.StagesExpensesSum);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Підсумок тижня: {SelectedDate.Date} Помилка: " + ex.Message);
                }
                return;
            }
            else //calculate on client
            {
                //if day = 7: i = 5; i > -1
                for (int i = SelectedDate.Day - 2; i != SelectedDate.Day - 8; i--) //start at saturday
                {
                    for(int j = 0; j < Owner.ScheduleForDayViews[i].DaySummaryList.Count; j++)
                    {
                        AddToWeekSummary(Owner.ScheduleForDayViews[i].DaySummaryList[j]);
                    }
                }
                for (int i = 0; i < DaySummaryList.Count; i++)
                {
                    AddToWeekSummary(DaySummaryList[i]);
                }
            }

        }
        public string DayOfWeek
        {
            get
            {

                return selectedDate.ToString("dddd", new CultureInfo("uk-UA")).ToUpper();
            }
        }
        public string AboutDay
        {
            get
            {
                if (SelectedDate.Date == DateTime.Now.Date)
                {
                    return "(Сьогодні)";
                }
                else if (SelectedDate.Date == DateTime.Now.Date.AddDays(1))
                {
                    return "(Завтра)";
                }
                else if (SelectedDate.Date == DateTime.Now.Date.AddDays(-1))
                {
                    return "(Вчора)";
                }
                else
                {
                    return "";
                }
            }
        }
        public string SelectedDateString
        {
            get
            {
                return selectedDate.ToString("d MMMM yyyy", new CultureInfo("uk-UA")) + " Р.";
            }
        }
        internal void UpdateRecordState(int recordId, int cabinetId, SchedulePatientState newState)
        {
            ScheduleTimeGridView timeGrid = TimeGrids.FirstOrDefault(g => g.Cabinet.Id == cabinetId);
            if (timeGrid != null)
            {
                timeGrid.UpdateRecordState(recordId, newState);
            }
        }
        internal void UpdateRecord(Schedule e)
        {
            ScheduleTimeGridView timeGrid = TimeGrids.FirstOrDefault(g => g.Cabinet.Id == e.CabinetId);
            if (timeGrid == null)
            {
                MessageBox.Show($"Не вдалось оновити запис в розкладі. Кабінет з номером {e.CabinetId} не знайдено");
                return;
            }
            ScheduleTimeGridElementView scheduleTimeGridElementView = timeGrid.TimeGridElementViews.FirstOrDefault(v => v.Schedule.Id == e.Id);
            scheduleTimeGridElementView.UpdateSchedule(e);
        }
        public void DeleteRecord(int recordId, int cabinetId)
        {
            ScheduleTimeGridView timeGrid = TimeGrids.FirstOrDefault(g => g.Cabinet.Id == cabinetId);
            if (timeGrid != null)
            {
                timeGrid.DeleteRecord(recordId);
            }
        }
        internal void UpdateRecordComment(int recordId, int cabinetId, string newComment)
        {
            ScheduleTimeGridView timeGrid = TimeGrids.FirstOrDefault(g => g.Cabinet.Id == cabinetId);
            if (timeGrid != null)
            {
                timeGrid.UpdateRecordComment(recordId, newComment);
            }
        }
        internal void UpdateDate(int year, int month, int day)
        {
            IsControlEnabled = true;
            Visibility = Visibility.Visible;
            DateTime newDateTime;
            try
            {
                newDateTime = new DateTime(year, month, day);
            }
            catch
            {
                newDateTime = DateTime.Now;
                IsControlEnabled = false;
                Visibility = Visibility.Collapsed;
            }
            
            SelectedDate = newDateTime;
        }
        internal void AddRecord(Schedule e)
        {
            ScheduleTimeGridView timeGrid = TimeGrids.FirstOrDefault(g => g.Cabinet.Id == e.CabinetId);
            if (timeGrid != null)
            {
                ScheduleTimeGridElementView scheduleTimeGridElementView = new ScheduleTimeGridElementView(e, timeGrid);
                timeGrid.TimeGridElementViews.Add(scheduleTimeGridElementView);
                timeGrid.grid.Children.Add(scheduleTimeGridElementView);
                AddToDaySummary(e.DoctorIds, e.StagesPriceSum, e.StagesPaidSum, e.PatientId.Value, e.StagesExpensesSum);
                DateTime stageDatetime = DateTime.ParseExact(e.StartDatetime, Options.DateTimePattern, null);
                int daysInWeek = 7;
                int daysTillNextSunday = (daysInWeek - (int)stageDatetime.DayOfWeek) % daysInWeek;
                DateTime nextSunday = stageDatetime + TimeSpan.FromDays(daysTillNextSunday);
                if (nextSunday.Month == stageDatetime.Month)
                {
                    ScheduleForDayView requestedSunday = Owner.ScheduleForDayViews.FirstOrDefault(s => s.SelectedDate.Date == nextSunday.Date);
                    if (requestedSunday != null && requestedSunday.SelectedDate.Day>=7)
                    {
                        requestedSunday.AddToWeekSummary(e.DoctorIds, e.StagesPriceSum, e.StagesPaidSum, e.StagesExpensesSum);
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        internal void UpdateCabinetComment(string newComment,int cabinetId)
        {
            ScheduleTimeGridView gridView = TimeGrids.FirstOrDefault(t => t.Cabinet.Id == cabinetId);
            if(gridView != null)
            {
                gridView.cabinetComment=newComment;
                gridView.OnPropertyChanged("CabinetComment");
            }
        }

        public void AddToDaySummary(List<int> doctorIds, List<int> priceSums, List<int> paidSums, int patientId, List<int> expensesSum)
        {
            if (DaySummaryPatientsId.Contains(patientId)) { return; }

            for(int i =0;i<doctorIds.Count; i++)
            {
                ScheduleSummaryViewModel existing = DaySummaryList.FirstOrDefault(vm => vm.Doctor.Id == doctorIds[i]);
                if (existing != null)
                {
                    existing.Price += priceSums[i];
                    existing.Payed += paidSums[i];
                    existing.Expenses += expensesSum[i];
                }
                else
                {
                    Doctor doctor = Options.AllDoctors.FirstOrDefault(d => d.Id == doctorIds[i]);
                    if (doctor == null)
                        throw new Exception("Doctor not found");
                    ScheduleSummaryViewModel vmToAdd = new ScheduleSummaryViewModel(doctor, priceSums[i], paidSums[i], expensesSum[i]);
                    daySummaryList.Add(vmToAdd);
                }
            }
            DaySummaryPatientsId.Add(patientId);
        }
        public void AddToDaySummary(int doctorId, int priceDifference, int paidDifference, int expensesDifference)
        {
            ScheduleSummaryViewModel existing = DaySummaryList.FirstOrDefault(vm => vm.Doctor.Id == doctorId);

            if (existing != null)
            {
                existing.Price += priceDifference;
                existing.Payed += paidDifference;
                existing.Expenses += expensesDifference;

                existing.NotifyPropertyChanged("DisplayText");
            }
            else
            {
                Doctor doctor = Options.AllDoctors.FirstOrDefault(d => d.Id == doctorId);
                if (doctor == null)
                    throw new Exception("DoctorId not found");
                ScheduleSummaryViewModel vmToAdd = new ScheduleSummaryViewModel(doctor, priceDifference, paidDifference,expensesDifference);
                daySummaryList.Add(vmToAdd);
                OnPropertyChanged("DaySummaryList");
            }

        }
        public void AddToWeekSummary(List<int> doctorIds, List<int> priceSums, List<int> paidSums, List<int> expensesSums)
        {
            for (int i = 0; i < doctorIds.Count; i++)
            {
                ScheduleSummaryViewModel existing = WeekSummaryList.FirstOrDefault(vm => vm.Doctor.Id == doctorIds[i]);
                if (existing != null)
                {
                    existing.Price += priceSums[i];
                    existing.Payed += paidSums[i];
                    existing.Expenses += expensesSums[i];
                }
                else
                {
                    Doctor doctor = Options.AllDoctors.FirstOrDefault(d => d.Id == doctorIds[i]);
                    if (doctor == null)
                        throw new Exception("DoctorId not found");
                    ScheduleSummaryViewModel vmToAdd = new ScheduleSummaryViewModel(doctor, priceSums[i], paidSums[i], expensesSums[i]);
                    weekSummaryList.Add(vmToAdd);
                }
            }
        }
        public void AddToWeekSummary(int doctorId, int priceDifference, int paidDifference, int expensesDifference)
        {
            ScheduleSummaryViewModel existing = WeekSummaryList.FirstOrDefault(vm => vm.Doctor.Id == doctorId);
            if (existing != null)
            {
                existing.Price += priceDifference;
                existing.Payed += paidDifference;
                existing.Expenses += expensesDifference;

                existing.NotifyPropertyChanged("DisplayText");

            }
            else
            {
                Doctor doctor = Options.AllDoctors.FirstOrDefault(d => d.Id == doctorId);
                if (doctor == null)
                    throw new Exception("DoctorId not found");
                ScheduleSummaryViewModel vmToAdd = new ScheduleSummaryViewModel(doctor, priceDifference, paidDifference, expensesDifference);
                weekSummaryList.Add(vmToAdd);
                OnPropertyChanged("WeekSummaryList");
            }
        }
        /// <summary>
        /// Adds summary view model from day to week
        /// </summary>
        /// <param name="scheduleSummaryViewModel"></param>
        public void AddToWeekSummary(ScheduleSummaryViewModel scheduleSummaryViewModel)
        {
            ScheduleSummaryViewModel existing = weekSummaryList.FirstOrDefault(vm => vm.Doctor.Id == scheduleSummaryViewModel.Doctor.Id);
            if (existing != null)
            {
                existing.Price += scheduleSummaryViewModel.Price;
                existing.Payed += scheduleSummaryViewModel.Payed;
                existing.Expenses += scheduleSummaryViewModel.Expenses;
            }
            else
            {
                Doctor doctor = Options.AllDoctors.FirstOrDefault(d => d.Id == scheduleSummaryViewModel.Doctor.Id);
                if (doctor == null)
                    throw new Exception("DoctorId not found");
                ScheduleSummaryViewModel vmToAdd = new ScheduleSummaryViewModel(doctor, scheduleSummaryViewModel.Price, scheduleSummaryViewModel.Payed, scheduleSummaryViewModel.Expenses);
                weekSummaryList.Add(vmToAdd);
            }
        }

        public ObservableCollection<ScheduleSummaryViewModel> DaySummaryList
        {
            get
            {
                return daySummaryList;
            }
            set
            {
                if(value!= daySummaryList)
                {
                    daySummaryList = value;
                    OnPropertyChanged();
                }
            }
        }
        private ObservableCollection<ScheduleSummaryViewModel> daySummaryList = new ObservableCollection<ScheduleSummaryViewModel>();
        
        public ObservableCollection<ScheduleSummaryViewModel> WeekSummaryList
        {
            get
            {
                return weekSummaryList;
            }
            set
            {
                if (value != weekSummaryList)
                {
                    weekSummaryList = value;
                    OnPropertyChanged();
                }
            }
        }
        private ObservableCollection<ScheduleSummaryViewModel> weekSummaryList = new ObservableCollection<ScheduleSummaryViewModel>();

        public List<int> DaySummaryPatientsId { get; set; } = new List<int>();
    }
    public class CabinetColors
    {
        public Color CabinetColor { get; set; }
        public Color ElementColor { get; set; }
        public Color ElementColorHovered { get; set; }
    }
}
