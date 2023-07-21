using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для ScheduleForDayView.xaml
    /// </summary>
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
            foreach (ScheduleTimeGridView timeGridView in TimeGrids)
            {
                timeGridView.LoadSchedule(selectedDate.Date);
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
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class CabinetColors
    {
        public Color CabinetColor { get; set; }
        public Color ElementColor { get; set; }
        public Color ElementColorHovered { get; set; }
    }
}
