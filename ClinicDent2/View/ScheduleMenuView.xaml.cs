using ClinicDent2.Attached;
using ClinicDent2.Interfaces;
using ClinicDentClientCommon;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using ClinicDentClientCommon.TcpClientToServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ClinicDent2.View
{
    public partial class ScheduleMenuView : UserControl, INotifyPropertyChanged, IBrowserTabControl
    {
        public static double? DesiredVerticalScrollOffset = null;
        public bool isInitialized = false;
        private bool isScrollChangedEventBlocked = false;
        public float[] ScheduleCalendarDayColorsThreshold;
        public List<ScheduleForDayView> ScheduleForDayViews { get; set; }
        public ScheduleMenuView()
        {

            InitializeComponent();
            ScheduleCalendarDayColorsThreshold = Options.ScheduleCalendarDayColorsThreshold.Split(new char[] {'|'},StringSplitOptions.RemoveEmptyEntries).Select(n=>float.Parse(n)).ToArray();
            selectedDate = DefaultSelectedDate;
            DataContext = this;
        }
        private void Notification_SchedlueCabinetCommentUpdated(string datetime, int cabinetId, string newComment)
        {
            if (selectedDate.ToString("yyyy-MM") == datetime.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == datetime.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.UpdateCabinetComment(newComment, cabinetId);
                }
            };
        }
        private void Notification_ScheduleRecordCommentUpdated(int recordId, string newComment, string date, int cabinetId)
        {
            if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.UpdateRecordComment(recordId, cabinetId, newComment);
                }
            }
        }
        private void Notification_ScheduleRecordStateUpdated(int recordId, SchedulePatientState newState, string date, int cabinetId)
        {
            if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.UpdateRecordState(recordId, cabinetId, newState);
                }
            }

        }
        private void Notification_ScheduleRecordDeleted(int recordId, string date, int cabinetId)
        {
            if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.DeleteRecord(recordId, cabinetId);
                }
            }
        }
        private void Notification_ScheduleRecordUpdated(Schedule e)
        {
            if (selectedDate.ToString("yyyy-MM") == e.StartDatetime.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == e.StartDatetime.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.UpdateRecord(e);
                }
            }
        }
        private void Notification_ScheduleStagesPaymentUpdated(string updatedStagesContent)
        {
            string[] splitted = updatedStagesContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= splitted.Length - 6; i += 6) //if length is 5 then start from 0 and next time begin with 5
            {
                int patientId = Convert.ToInt32(splitted[i]);
                DateTime stageDatetime = DateTime.ParseExact(splitted[i + 1], SharedData.DateTimePattern, null);
                int priceDifference = Convert.ToInt32(splitted[i + 2]);
                int payedDifference = Convert.ToInt32(splitted[i + 3]);
                int doctorId = Convert.ToInt32(splitted[i + 4]);
                int expensesDifference = Convert.ToInt32(splitted[i + 5]);


                if (SelectedDate.Year == stageDatetime.Year && SelectedDate.Month == stageDatetime.Month)
                {
                    //find requred day
                    int scheduleForDayIndex = -1;
                    for (int c = 0; c < ScheduleForDayViews.Count; c++)
                    {
                        if (ScheduleForDayViews[c].SelectedDate.Day == stageDatetime.Day)
                        {
                            scheduleForDayIndex = c;
                            break;
                        }
                    }
                    if (scheduleForDayIndex == -1) { return; }
                    bool daySummaryAlreadyUpdated = false;
                    //try to find to which cabinet this patient belongs
                    for (int j = 0; j < ScheduleForDayViews[scheduleForDayIndex].TimeGrids.Count; j++)
                    {

                        for (int k = 0; k < ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews.Count; k++)
                        {
                            if (ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.PatientId == patientId)
                            {
                                ///find to which doctor it belongs
                                int foundDoctorIndex = -1;
                                for (int q = 0; q < ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.DoctorIds.Count; q++)
                                {
                                    if (ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.DoctorIds[q] == doctorId)
                                    {
                                        foundDoctorIndex = q; break;
                                    }
                                }
                                if (foundDoctorIndex == -1)
                                {
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.DoctorIds.Add(doctorId);
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPaidSum.Add(payedDifference);
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPriceSum.Add(priceDifference);
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesExpensesSum.Add(expensesDifference);
                                    foundDoctorIndex = ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.DoctorIds.Count;
                                }
                                else
                                {
                                    //add to existing
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPaidSum[foundDoctorIndex] += payedDifference;
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPriceSum[foundDoctorIndex] += priceDifference;
                                    ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesExpensesSum[foundDoctorIndex] += expensesDifference;

                                }
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("StagesPaidSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("StagesPriceSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("PaidPriceText");
                                if (daySummaryAlreadyUpdated == false)
                                {
                                    ScheduleForDayViews[scheduleForDayIndex].AddToDaySummary(doctorId, priceDifference, payedDifference, expensesDifference);

                                    int daysInWeek = 7;
                                    int daysTillNextSunday = (daysInWeek - (int)stageDatetime.DayOfWeek) % daysInWeek;
                                    DateTime nextSunday = stageDatetime + TimeSpan.FromDays(daysTillNextSunday);
                                    if (nextSunday.Month == stageDatetime.Month)
                                    {
                                        ScheduleForDayViews[scheduleForDayIndex + daysTillNextSunday].AddToWeekSummary(doctorId, priceDifference, payedDifference, expensesDifference);
                                    }
                                    daySummaryAlreadyUpdated = true;
                                }

                            }
                        }
                    }
                }
                else if (IsNextSundayInThisMonth(stageDatetime) == true)
                {
                    DateTime dateTime = stageDatetime + TimeSpan.FromDays(7);
                    if (dateTime.Year == SelectedDate.Year && dateTime.Month == SelectedDate.Month)
                    {
                        //find first sunday in this month
                        ScheduleForDayView scheduleForDayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.DayOfWeek == DayOfWeek.Sunday);
                        scheduleForDayView.AddToWeekSummary(doctorId, priceDifference, payedDifference, expensesDifference);
                    }
                }
            }
        }
        private void Notification_ScheduleRecordAdded(Schedule e)
        {
            if (selectedDate.ToString("yyyy-MM") == e.StartDatetime.Substring(0, 7)) // check if updated record related to current month
            {
                //find day to which record belongs
                ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == e.StartDatetime.Substring(8, 2));
                if (dayView != null)
                {
                    dayView.AddRecord(e);
                }
            }
            e.CabinetName = SharedData.AllCabinets.FirstOrDefault(c => c.Id == e.CabinetId).CabinetName;
        }
        private DateTime selectedDate;
        public static DateTime DefaultSelectedDate;
        public DateTime SelectedDate
        {
            get
            {
                return selectedDate;
            }
            set
            {
                if (value.Year != selectedDate.Year || value.Month != selectedDate.Month)
                {
                    DefaultSelectedDate = value;
                    selectedDate = value;
                    OnPropertyChanged();
                    for (int i = 0; i < ScheduleForDayViews.Count; i++)
                    {
                        ScheduleForDayViews[i].UpdateDate(selectedDate.Year, selectedDate.Month, i + 1);
                    }
                    ScrollToTopOfScrollViewer(ScheduleForDayViews[selectedDate.Day - 1], scrollViewerSchedule);
                    //UpdateCalendarDaysState();
                }
                else
                {
                    DefaultSelectedDate = value;
                    selectedDate = value;
                    int dayOfMonth = selectedDate.Day;
                    ScrollToTopOfScrollViewer(ScheduleForDayViews[dayOfMonth - 1], scrollViewerSchedule);
                    OnPropertyChanged();
                }
                
            }
        }
        static ScheduleMenuView()
        {
            DefaultSelectedDate = DateTime.Now;

        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        private void datePicker_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Mouse.Captured is CalendarItem)
            {
                Mouse.Captured.ReleaseMouseCapture();
            }
        }
        private void scrollViewerSchedule_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 && isScrollChangedEventBlocked == false)
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;

                ScheduleForDayView mostVisibleItem = null;
                double maxVisibleHeight = 0;

                foreach (var item in ScheduleForDayViews)
                {
                    Rect itemRect = item.TransformToVisual(scrollViewer).TransformBounds(new Rect(0.0, 0.0, item.ActualWidth, item.ActualHeight));
                    Rect intersection = Rect.Intersect(new Rect(0.0, 0.0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight), itemRect);

                    if (intersection != Rect.Empty && intersection.Height > maxVisibleHeight)
                    {
                        maxVisibleHeight = intersection.Height;
                        mostVisibleItem = item;
                    }
                }

                if (mostVisibleItem != null)
                {
                    selectedDate = mostVisibleItem.SelectedDate;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
            isScrollChangedEventBlocked = false;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(isInitialized == false)
            {
                ScheduleForDayViews = new List<ScheduleForDayView>(31);
                for (int i = 0; i < 31; i++)
                {
                    ScheduleForDayView newScheduleForDayView = new ScheduleForDayView(selectedDate.Year, selectedDate.Month, i + 1, this);
                    stackPanelDays.Children.Add(newScheduleForDayView);
                    ScheduleForDayViews.Add(newScheduleForDayView);
                }
                isInitialized = true;
            }
            if (DesiredVerticalScrollOffset == null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    int dayOfMonth = selectedDate.Day;
                    ScrollToTopOfScrollViewer(ScheduleForDayViews[dayOfMonth - 1], scrollViewerSchedule);
                }), DispatcherPriority.Loaded);
            }
            else
            {
                scrollViewerSchedule.ScrollToVerticalOffset(DesiredVerticalScrollOffset.Value);
            }
            datePicker.DisplayDate = datePicker.SelectedDate.Value;
        }
        public void TabActivated()
        {

        }
        public Task<bool> TabDeactivated()
        {
            DesiredVerticalScrollOffset=scrollViewerSchedule.VerticalOffset;
            return Task.FromResult(true);
        }
        public void TabClosed()
        {
        }
        private bool isDragging = false;
        private System.Windows.Point clickPosition;
        private void datePicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(datePicker);
            datePicker.CaptureMouse();
        }
        private void datePicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(scrollViewerSchedule);
                Canvas.SetTop(datePicker, currentPosition.Y - clickPosition.Y);
                Canvas.SetLeft(datePicker, currentPosition.X - clickPosition.X);
            }
        }
        private void datePicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            datePicker.ReleaseMouseCapture();
        }
        public void Notify(int notificationCode, object param)
        {
            switch(notificationCode)
            {
                case NotificationCodes.ScheduleStagesPaymentUpdated:
                    string updatedStagesContent = (string)param;
                    Notification_ScheduleStagesPaymentUpdated(updatedStagesContent);
                    break;
                case NotificationCodes.ScheduleCabinetCommentUpdated:
                    (string datetime, int cabinetId, string newComment) obj = ((string datetime, int cabinetId, string newComment))param;
                    Notification_SchedlueCabinetCommentUpdated(obj.datetime, obj.cabinetId, obj.newComment);
                    break;
                case NotificationCodes.ScheduleRecordCommentUpdated:
                    (int recordId, string newComment, string date, int cabinetId) obj2 = ((int recordId, string newComment, string date, int cabinetId))param;
                    Notification_ScheduleRecordCommentUpdated(obj2.recordId, obj2.newComment, obj2.date, obj2.cabinetId);
                    break;
                case NotificationCodes.ScheduleRecordStateUpdated:
                    (int recordId, SchedulePatientState newState, string dateTime, int cabinetId) obj3 = ((int recordId, SchedulePatientState newState, string dateTime, int cabinetId))param;
                    Notification_ScheduleRecordStateUpdated(obj3.recordId, obj3.newState, obj3.dateTime, obj3.cabinetId);
                    break;
                case NotificationCodes.ScheduleRecordDeleted:
                    (int recordId, string date, int cabinetId) obj4 = ((int recordId, string date, int cabinetId))param;
                    Notification_ScheduleRecordDeleted(obj4.recordId, obj4.date, obj4.cabinetId);
                    break;
                case NotificationCodes.ScheduleRecordUpdated:
                    Schedule s = (Schedule)param;
                    Notification_ScheduleRecordUpdated(s);
                    break;
                case NotificationCodes.ScheduleRecordAdded:
                    Schedule s2 = (Schedule)param;
                    Notification_ScheduleRecordAdded(s2);
                    break;
            }
        }
        public bool IsNextSundayInThisMonth(DateTime stageDatetime)
        {
            // Calculate how many days to add to reach next Sunday
            int daysToAdd = ((int)DayOfWeek.Sunday - (int)stageDatetime.DayOfWeek + 7) % 7;
            if (daysToAdd == 0) daysToAdd = 7; // If today is Sunday, then next Sunday would be 7 days later.

            DateTime nextSunday = stageDatetime.AddDays(daysToAdd);

            // Check if the month of the next Sunday is different from the original date
            return nextSunday.Month != stageDatetime.Month;
        }

        public void UpdateCalendarDayState(List<ScheduleTimeGridElementView> timeGridElementViews, DateTime dateTime)
        {
            ///Calculate hours
            TimeSpan stagesSumTime = TimeSpan.Zero;
            foreach (ScheduleTimeGridElementView scheduleTimeGridElementView in timeGridElementViews)
            {
                stagesSumTime += scheduleTimeGridElementView.ScheduleEndTime - scheduleTimeGridElementView.ScheduleStartTime;
            }

            
            ///Mark button at calendar with corresponding background
            CalendarDayButton calendarDayButton = datePicker.FindVisualChildren<System.Windows.Controls.Primitives.CalendarDayButton>().FirstOrDefault(button => (button.Content as string) == dateTime.Day.ToString() && button.IsInactive == false);
            if (calendarDayButton == null) { return; }
            calendarDayButton.Background = GetBrushForValue(stagesSumTime.TotalHours);
        }
        public Brush GetBrushForValue(double hours)
        {
            Color color;

            if (hours < 0.5)
            {
                color = Colors.Transparent;

            }
            else if (hours <= ScheduleCalendarDayColorsThreshold[0])
            {
                color = Colors.LightGreen;
            }
            else if (hours < ScheduleCalendarDayColorsThreshold[1])
            {
                color = Colors.Green;
            }
            else if (hours < ScheduleCalendarDayColorsThreshold[2])
            {
                color = Colors.Yellow;
            }
            else if (hours < ScheduleCalendarDayColorsThreshold[3])
            {
                color = Colors.Orange;
            }
            else
            {
                color = Colors.Red;

            }
            return new SolidColorBrush(color);
        }

        private Color InterpolateColor(Color color1, Color color2, double fraction)
        {
            // Check if the transition is from Transparent to a solid color
            if (color1.A == 0 && color2.A == 255)
            {
                // Only interpolate the alpha channel
                byte a = (byte)(color1.A + (color2.A - color1.A) * fraction);
                return Color.FromArgb(a, color2.R, color2.G, color2.B);
            }
            else
            {
                // Interpolate all channels as before
                byte a = (byte)(color1.A + (color2.A - color1.A) * fraction);
                byte r = (byte)(color1.R + (color2.R - color1.R) * fraction);
                byte g = (byte)(color1.G + (color2.G - color1.G) * fraction);
                byte b = (byte)(color1.B + (color2.B - color1.B) * fraction);

                return Color.FromArgb(a, r, g, b);
            }
        }
        public void ClearCalendarDaysState()
        {
            CalendarDayButton[] calendarDayButtons = datePicker.FindVisualChildren<System.Windows.Controls.Primitives.CalendarDayButton>().ToArray();
            foreach (CalendarDayButton button in calendarDayButtons)
            {
                button.Background = System.Windows.Media.Brushes.Transparent;
            }

        }

        private void datePicker_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            ClearCalendarDaysState();
            if (e.AddedDate.HasValue)
            {
                if(e.AddedDate.Value.Month == SelectedDate.Month && e.AddedDate.Value.Year == SelectedDate.Year)
                {
                    UpdateCalendarDaysState();
                }
            }
            
        }
        public void ScrollToTopOfScrollViewer(UIElement targetElement, ScrollViewer scrollViewer)
        {
            // Convert the target element's top-left position to the scroll viewer's coordinate space.
            GeneralTransform generalTransform = targetElement.TransformToVisual(scrollViewer);
            System.Windows.Point offset = generalTransform.Transform(new System.Windows.Point(0, 0));
            // Set the scroll viewer's vertical offset to the calculated position.
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset.Y);
            isScrollChangedEventBlocked = true;

        }
        private void UpdateCalendarDaysState()
        {
            foreach (ScheduleForDayView scheduleForDayView in ScheduleForDayViews)
            {
                scheduleForDayView.TimeGrids.FirstOrDefault(t => t.Cabinet.Id == Options.DefaultSelectedTable)?.UpdateCalendarDayState();
            }
        }
    }
    public enum CalendarDayButtonStates
    {
        White=0,Green=1,Yellow=2,Orange=3,Red=4
    };
}
