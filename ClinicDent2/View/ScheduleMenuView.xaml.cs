﻿using ClinicDent2.Interfaces;
using ClinicDent2.Model;
using ClinicDent2.TcpClientToServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ClinicDent2.View
{
    public partial class ScheduleMenuView : UserControl, INotifyPropertyChanged, IBrowserTabControl
    {
        public static double? DesiredVerticalScrollOffset = null;
        public TcpClient TcpClient;
        public List<ScheduleForDayView> ScheduleForDayViews { get; set; }
        public ScheduleMenuView()
        {
            TcpClient = new TcpClient();
            TcpClient.ScheduleRecordAdded += TcpConnection_ScheduleRecordAdded;
            TcpClient.ScheduleRecordUpdated += TcpConnection_ScheduleRecordUpdated;
            TcpClient.ScheduleRecordDeleted += TcpConnection_ScheduleRecordDeleted;
            TcpClient.ScheduleRecordStateUpdated += TcpConnection_ScheduleStateUpdated;
            TcpClient.ScheduleRecordCommentUpdated += TcpConnection_ScheduleCommentUpdated;
            TcpClient.SchedlueCabinetCommentUpdated += TcpClient_SchedlueCabinetCommentUpdated;

            InitializeComponent();

            selectedDate = DefaultSelectedDate;
            ScheduleForDayViews = new List<ScheduleForDayView>(31);
            for (int i = 0; i < 31; i++)
            {
                ScheduleForDayView newScheduleForDayView = new ScheduleForDayView(selectedDate.Year, selectedDate.Month, i + 1, this);
                stackPanelDays.Children.Add(newScheduleForDayView);
                ScheduleForDayViews.Add(newScheduleForDayView);
            }
            DataContext = this;
            TcpClient.ConnectToServer();
        }

        private void TcpClient_SchedlueCabinetCommentUpdated(string datetime, int cabinetId, string newComment)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (selectedDate.ToString("yyyy-MM") == datetime.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == datetime.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.UpdateCabinetComment(newComment, cabinetId);
                    }
                }
            }));
        }

        private void TcpConnection_ScheduleCommentUpdated(int recordId, string newComment, string date, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.UpdateRecordComment(recordId, cabinetId, newComment);
                    }
                }
            }));
        }

        private void TcpConnection_ScheduleStateUpdated(int recordId, SchedulePatientState newState, string date, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.UpdateRecordState(recordId, cabinetId, newState);
                    }
                }
            }));

        }

        private void TcpConnection_ScheduleRecordDeleted(int recordId, string date, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (selectedDate.ToString("yyyy-MM") == date.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == date.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.DeleteRecord(recordId, cabinetId);
                    }
                }
                Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordDeleted, recordId);
            }));

        }

        private void TcpConnection_ScheduleRecordUpdated(object sender, Schedule e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (selectedDate.ToString("yyyy-MM") == e.StartDatetime.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == e.StartDatetime.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.UpdateRecord(e);
                    }
                }
                Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordUpdated, e);
            }));
        }

        private void TcpConnection_ScheduleRecordAdded(object sender, Schedule e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
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
                e.CabinetName = ScheduleForDayView.Cabinets.FirstOrDefault(c => c.Id == e.CabinetId).CabinetName;
                Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordAdded, new Schedule(e));
            }));
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
                    for (int i = 0; i < ScheduleForDayViews.Count; i++)
                    {
                        ScheduleForDayViews[i].UpdateDate(value.Year, value.Month, i + 1);
                    }
                }
                DefaultSelectedDate = value;
                selectedDate = value;
                int dayOfMonth = selectedDate.Day;
                scrollViewerSchedule.ScrollToVerticalOffset(ScheduleForDayViews[dayOfMonth - 1].ActualHeight * (dayOfMonth - 1));
                OnPropertyChanged();
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
            if (e.VerticalChange != 0)
            {
                double elementHeight = ScheduleForDayViews[0].ActualHeight;
                int currentElement = (int)e.VerticalOffset / (int)elementHeight;
                selectedDate = ScheduleForDayViews[currentElement].SelectedDate;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(DesiredVerticalScrollOffset == null)
            {
                int dayOfMonth = selectedDate.Day;
                scrollViewerSchedule.ScrollToVerticalOffset(ScheduleForDayViews[dayOfMonth - 1].ActualHeight * (dayOfMonth - 1));
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

        public void TabDeactivated()
        {
            DesiredVerticalScrollOffset=scrollViewerSchedule.VerticalOffset;
        }

        public void TabClosed()
        {
            TcpClient.DisconnectFromServer();
        }
        private bool isDragging = false;
        private Point clickPosition;
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
                Point currentPosition = e.GetPosition(scrollViewerSchedule);
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
            }
        }
        private void Notification_ScheduleStagesPaymentUpdated(string updatedStagesContent)
        {
            string[] splitted = updatedStagesContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= splitted.Length - 5; i += 5) //if length is 5 then start from 0 and next time begin with 5
            {
                int patientId = Convert.ToInt32(splitted[i]);
                DateTime stageDatetime = DateTime.ParseExact(splitted[i + 1], Options.DateTimePattern, null);
                int priceDifference = Convert.ToInt32(splitted[i + 2]);
                int payedDifference = Convert.ToInt32(splitted[i + 3]);
                int doctorId = Convert.ToInt32(splitted[i + 4]);

                if(SelectedDate.Year == stageDatetime.Year && SelectedDate.Month == stageDatetime.Month)
                {
                    //find requred day
                    int scheduleForDayIndex = -1;
                    for(int c =0;c< ScheduleForDayViews.Count; c++)
                    {
                        if (ScheduleForDayViews[c].SelectedDate.Day == stageDatetime.Day)
                        {
                            scheduleForDayIndex = c;
                            break;
                        }
                    }
                    if(scheduleForDayIndex == -1) { return; }
                    //try to find to which cabinet this patient belongs
                    for(int j=0;j< ScheduleForDayViews[scheduleForDayIndex].TimeGrids.Count; j++)
                    {
                        for(int k =0; k< ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews.Count; k++)
                        {
                            if (ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.PatientId == patientId)
                            {
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPaidSum += payedDifference;
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].Schedule.StagesPriceSum += priceDifference;
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("StagesPaidSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("StagesPriceSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].TimeGridElementViews[k].OnPropertyChanged("PaidPriceText");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].payedSum += payedDifference;
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].priceSum += priceDifference;
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].OnPropertyChanged("PayedSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].OnPropertyChanged("PriceSum");
                                ScheduleForDayViews[scheduleForDayIndex].TimeGrids[j].OnPropertyChanged("DayMoneySummary");
                                int daysInWeek = 7;
                                int daysTillNextSunday = daysInWeek - (int)stageDatetime.DayOfWeek;
                                if (daysTillNextSunday != 7)
                                {
                                    DateTime nextSunday = stageDatetime + TimeSpan.FromDays(daysTillNextSunday);
                                    if(nextSunday.Month == stageDatetime.Month)
                                    {
                                        //ScheduleForDayViews[scheduleForDayIndex + daysTillNextSunday].TimeGrids[j].AddToWeekSummary(priceDifference, payedDifference);
                                        ScheduleForDayViews[scheduleForDayIndex + daysTillNextSunday].TimeGrids[j].OnPropertyChanged("DayMoneySummary");
                                    }
                                }
                            }
                        }
                    }
                }
                else if(IsNextSundayInThisMonth(stageDatetime) == true)
                {
                    DateTime dateTime = stageDatetime + TimeSpan.FromDays(7);
                    if (dateTime.Year == SelectedDate.Year && dateTime.Month == SelectedDate.Month)
                    {
                        //find first sunday in this month
                        ScheduleForDayView scheduleForDayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.DayOfWeek == DayOfWeek.Sunday);
                        for (int j = 0; j < scheduleForDayView.TimeGrids.Count; j++)
                        {
                            scheduleForDayView.TimeGrids[j].OnPropertyChanged("DayMoneySummary");
                        }
                    }
                }
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
    }
}
