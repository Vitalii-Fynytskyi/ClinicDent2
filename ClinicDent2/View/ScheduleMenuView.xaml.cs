using ClinicDent2.Model;
using ClinicDent2.TcpClientToServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для ScheduleMenuView.xaml
    /// </summary>
    public partial class ScheduleMenuView : UserControl, INotifyPropertyChanged
    {
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

            InitializeComponent();

            selectedDate = DefaultSelectedDate;
            ScheduleForDayViews = new List<ScheduleForDayView>(31);
            for(int i =0;i< 31; i++)
            {
                ScheduleForDayView newScheduleForDayView = new ScheduleForDayView(selectedDate.Year, selectedDate.Month, i+1, this);
                stackPanelDays.Children.Add(newScheduleForDayView);
                ScheduleForDayViews.Add(newScheduleForDayView);
            }
            
            DataContext = this;
            TcpClient.ConnectToServer();
            scrollViewerSchedule.ScrollToVerticalOffset(ScheduleForDayViews[selectedDate.Day - 1].ActualHeight * (selectedDate.Day - 1));

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
            }));
            
        }

        private void TcpConnection_ScheduleRecordUpdated(object sender, Schedule e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if(selectedDate.ToString("yyyy-MM") == e.StartDatetime.Substring(0, 7)) // check if updated record related to current month
                {
                    //find day to which record belongs
                    ScheduleForDayView dayView = ScheduleForDayViews.FirstOrDefault(v => v.SelectedDate.ToString("dd") == e.StartDatetime.Substring(8, 2));
                    if (dayView != null)
                    {
                        dayView.UpdateRecord(e);
                    }
                }
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
                if(value.Year != selectedDate.Year || value.Month != selectedDate.Month)
                {
                    for(int i = 0; i < ScheduleForDayViews.Count; i++)
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            TcpClient.DisconnectFromServer();
        }

        private void datePicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void datePicker_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Mouse.Capture(null);
        }
    }
}
