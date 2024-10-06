using ClinicDent2.TabbedBrowser;
using ClinicDentClientCommon.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicDent2.View
{
    public partial class ScheduleTimeGridElementView : UserControl, INotifyPropertyChanged
    {
        public bool isMovingMode = false;
        public SchedulePatientState State
        {
            get
            {
                if (Schedule == null)
                    return 0;
                return Schedule.State;
            }
        }
        public static TimeSpan startTimeSpan;
        public static TimeSpan endTimeSpan;
        private TimeSpan scheduleStartTime;
        private TimeSpan scheduleEndTime;
        public TimeSpan ScheduleStartTime
        {
            get
            {
                return scheduleStartTime;
            }
            set
            {
                scheduleStartTime = value;
                GridRow = (int)(scheduleStartTime - startTimeSpan).TotalMinutes / 30 + 1;
            }
        }

        internal void UpdateTimes()
        {
            string date = ParentGridView.Owner.SelectedDate.ToString("yyyy-MM-dd ");
            Schedule.StartDatetime = date + ScheduleStartTime.ToString(@"hh\:mm");
            Schedule.EndDatetime = date + ScheduleEndTime.ToString(@"hh\:mm");
            Options.MainWindow.mainMenu.TcpClient.UpdateRecord(Schedule);
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordUpdated,Schedule);
            ParentGridView.UpdateCalendarDayState();
        }
        private void WillAppearChanged()
        {
            switch (Schedule.State)
            {
                case SchedulePatientState.Unknown:
                    Options.MainWindow.mainMenu.TcpClient.UpdateRecordState(Schedule.Id, SchedulePatientState.WillAppear);
                    this.Schedule.State = SchedulePatientState.WillAppear;
                    break;
                case SchedulePatientState.WillAppear:
                    Options.MainWindow.mainMenu.TcpClient.UpdateRecordState(Schedule.Id, SchedulePatientState.Refused);
                    this.Schedule.State = SchedulePatientState.Refused;
                    break;
                case SchedulePatientState.Refused:
                    Options.MainWindow.mainMenu.TcpClient.UpdateRecordState(Schedule.Id, SchedulePatientState.Unknown);
                    this.Schedule.State = SchedulePatientState.Unknown;
                    break;
            }
            OnPropertyChanged("State");
        }
        private async Task GoToStages()
        {

            string dayOfMonth = ParentGridView.Owner.SelectedDate.GetUkrainianDayOfMonth();
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(Schedule.PatientId.Value, TabButtonType.PatientScheduleStages,dayOfMonth) == false)
            {
                BrowserTabButton patientStagesButton = new BrowserTabButton();
                StagesView stagesView = new StagesView();
                try
                {
                    await stagesView.LoadAllPatientStagesWithScheduleMarked(ParentGridView.Owner.SelectedDate, Schedule.PatientId.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {ex.Message}");
                    return;
                }
                patientStagesButton.TabText = this.PatientName + $" {dayOfMonth}";
                patientStagesButton.PatientViewModel = stagesView.stagesViewModel.Patient;
                patientStagesButton.ButtonType = TabButtonType.PatientScheduleStages;
                patientStagesButton.Control = stagesView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(patientStagesButton);
            }

        }
        public TimeSpan ScheduleEndTime
        {
            get
            {
                return scheduleEndTime;
            }
            set
            {
                scheduleEndTime = value;
                GridRowSpan = (int)(scheduleEndTime - scheduleStartTime).TotalMinutes / 30;
            }
        }
        int gridRow;
        int gridRowSpan;
        public int GridRow
        {
            get
            {
                return gridRow;
            }
            set
            {
                gridRow = value;
                Grid.SetRow(this, gridRow);
            }
        }
        public int GridRowSpan
        {
            get
            {
                return gridRowSpan;
            }
            set
            {
                gridRowSpan = value;
                Grid.SetRowSpan(this, gridRowSpan);
            }
        }
        public string PatientName
        {
            get
            {
                return Schedule.PatientName;
            }
        }
        public string Comment
        {
            get
            {
                return Schedule.Comment;
            }
            set
            {
                Schedule.Comment = value;
                OnPropertyChanged();
                Options.MainWindow.mainMenu.TcpClient.UpdateRecordComment(Schedule.Id, value);
            }
        }
        public ScheduleIsSentViaMessagetState StagesSentViaMessagerState
        {
            get
            {
                return Schedule.StagesSentViaMessagerState;
            }
            set
            {
                Schedule.StagesSentViaMessagerState = value;
                OnPropertyChanged();
            }
        }
        public int StagesPaidSum
        {
            get
            {
                return Schedule.StagesPaidSum.Sum();
            }
        }
        public int StagesPriceSum
        {
            get
            {
                return Schedule.StagesPriceSum.Sum();
            }
        }
        public string PaidPriceText
        {
            get
            {
                return $"{StagesPaidSum}/{StagesPriceSum} грн.";
            }
        }
        internal void UpdateRecordState(SchedulePatientState newState)
        {
            Schedule.State = newState;
            OnPropertyChanged("State");
        }

        static ScheduleTimeGridElementView()
        {
            startTimeSpan = TimeSpan.FromHours(8);
            endTimeSpan = TimeSpan.FromHours(20);
        }
        public TimeGridEmpinessView MouseDownElement { get; set; }
        public Schedule Schedule { get; set; }
        public ScheduleTimeGridView ParentGridView { get; set; }
        public ScheduleTimeGridElementView(Schedule s, ScheduleTimeGridView parentToSet)
        {
            ParentGridView = parentToSet;
            Schedule = s;
            DataContext = this;
            InitializeComponent();
            grid.Background = new SolidColorBrush(ParentGridView.CabinetColor.ElementColor);
            //get time from datetime (last 5 characters) and convert it to TimeSpan
            ScheduleStartTime = TimeSpan.ParseExact(s.StartDatetime.Substring(s.StartDatetime.Length - 5, 5), @"hh\:mm", CultureInfo.CurrentCulture);
            ScheduleEndTime = TimeSpan.ParseExact(s.EndDatetime.Substring(s.StartDatetime.Length - 5, 5), @"hh\:mm", CultureInfo.CurrentCulture);
            Grid.SetColumn(this, 1);
        }

        internal void UpdateSchedule(Schedule e)
        {
            Schedule.PatientId = e.PatientId;
            Schedule.PatientName = e.PatientName;
            Schedule.State = e.State;
            Schedule.Comment = e.Comment;
            Schedule.DoctorId = e.DoctorId;
            Schedule.StartDatetime = e.StartDatetime;
            Schedule.EndDatetime = e.EndDatetime;
            //TODO Ensure Comment is updated
            //OnPropertyChanged("Comment");

            OnPropertyChanged("PatientName");
            OnPropertyChanged("State");


            ScheduleStartTime = TimeSpan.ParseExact(e.StartDatetime.Substring(e.StartDatetime.Length - 5, 5), @"hh\:mm", CultureInfo.CurrentCulture);
            ScheduleEndTime = TimeSpan.ParseExact(e.EndDatetime.Substring(e.StartDatetime.Length - 5, 5), @"hh\:mm", CultureInfo.CurrentCulture);
        }

        private void Separator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ParentGridView.SeparatorSelectedElement = this;
            e.Handled = true;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(ParentGridView.SeparatorSelectedElement != null)
            {
                Point pt = e.GetPosition(ParentGridView);
                VisualTreeHelper.HitTest(ParentGridView, null, new HitTestResultCallback(ChangeEndTimeToEarlierHitTestResult), new PointHitTestParameters(pt));
            }
            else if(ParentGridView.MovementSelectedElement != null)
            {
                ParentGridView.MovementSelectedElement.MoveRecordListener(e.GetPosition(ParentGridView));
            }
        }

        internal void UpdateRecordComment(string newComment)
        {
            Schedule.Comment = newComment;
            OnPropertyChanged(nameof(Comment));
        }

        public void MoveRecordListener(Point pt)
        {
            if (MouseDownElement != null)
            {
                VisualTreeHelper.HitTest(ParentGridView, null, new HitTestResultCallback(MoveRecordHitTestResult), new PointHitTestParameters(pt));
            }
        }
        // Return the result of the hit test to the callback.
        public HitTestResultBehavior ChangeEndTimeToEarlierHitTestResult(HitTestResult result)
        {
            DependencyObject element = result.VisualHit;

            while (element != null && !(element is TimeGridEmpinessView))
                element = VisualTreeHelper.GetParent(element);

            if(element == null)
            {
                return HitTestResultBehavior.Continue;
            }
            else if(element is TimeGridEmpinessView target)
            {
                if (ParentGridView.SeparatorSelectedElement != null)
                {
                    ParentGridView.TryChangeEndTimeToEarlier(target);
                }
                isMovingMode = true;
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }
        public HitTestResultBehavior MoveRecordHitTestResult(HitTestResult result)
        {
            DependencyObject element = result.VisualHit;

            while (element != null && !(element is TimeGridEmpinessView))
                element = VisualTreeHelper.GetParent(element);

            if (element == null)
            {
                return HitTestResultBehavior.Continue;
            }
            else if (element is TimeGridEmpinessView target)
            {
                if (target.Time > MouseDownElement.Time && scheduleEndTime<TimeSpan.FromHours(20) + TimeSpan.FromMinutes(30))
                {
                    ScheduleStartTime += target.Time - MouseDownElement.Time;
                    ScheduleEndTime += target.Time - MouseDownElement.Time;
                    MouseDownElement = target;
                }
                else if(target.Time < MouseDownElement.Time && scheduleStartTime >TimeSpan.FromHours(8))
                {
                    ScheduleStartTime -= MouseDownElement.Time - target.Time;
                    ScheduleEndTime -= MouseDownElement.Time - target.Time;
                    MouseDownElement = target;
                }
                isMovingMode = true;
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }
        public HitTestResultBehavior MouseDownHitTestResult(HitTestResult result)
        {
            DependencyObject element = result.VisualHit;

            while (element != null && !(element is TimeGridEmpinessView))
                element = VisualTreeHelper.GetParent(element);

            if (element == null)
            {
                return HitTestResultBehavior.Continue;
            }
            else if (element is TimeGridEmpinessView target)
            {
                MouseDownElement = target;
                ParentGridView.MovementSelectedElement = this;
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMovingMode = false;
            Point pt = e.GetPosition(ParentGridView);
            VisualTreeHelper.HitTest(ParentGridView, null, new HitTestResultCallback(MouseDownHitTestResult), new PointHitTestParameters(pt));
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            MouseDownElement = null;
            ParentGridView.SeparatorSelectedElement = null;
            isMovingMode = false;
            ParentGridView.MovementSelectedElement = null;

        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            grid.Background = new SolidColorBrush(ParentGridView.CabinetColor.ElementColorHovered);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            grid.Background = new SolidColorBrush(ParentGridView.CabinetColor.ElementColor);

        }

        private void menuItemChangePatient_Click(object sender, RoutedEventArgs e)
        {
            WindowContainer windowSelectPatient = new WindowContainer();
            FormSelectPatient formSelectPatient = new FormSelectPatient();
            formSelectPatient.PatientSelected += FormSelectPatient_PatientSelected;
            windowSelectPatient.Content = formSelectPatient;
            windowSelectPatient.ShowDialog();
        }

        private void FormSelectPatient_PatientSelected(object sender, Patient e)
        {
            Schedule.PatientId = e.Id;
            Schedule.PatientName = e.Name;
            OnPropertyChanged("PatientName");
            Options.MainWindow.mainMenu.TcpClient.UpdateRecord(Schedule);
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordUpdated, Schedule);
        }

        private void menuItemDeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            ParentGridView.TimeGridElementViews.Remove(this);
            ParentGridView.grid.Children.Remove(this);
            ParentGridView.UpdateCalendarDayState();
            Options.MainWindow.mainMenu.TcpClient.DeleteRecord(Schedule.Id);
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordDeletedIdOnly, Schedule.Id);
        }

        private void buttonWillAppear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WillAppearChanged();
        }

        private async void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await GoToStages();
        }

        private void UserControl_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(e.OriginalSource.ToString());
            if(e.OriginalSource is TextBlock || e.OriginalSource is Panel)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
