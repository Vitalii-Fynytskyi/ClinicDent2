using ClinicDent2.Interfaces;
using ClinicDent2.Model;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicDent2.View
{
    public partial class StagesView : UserControl, IBrowserTabControl, INotifyPropertyChanged
    {
        public StagesViewModel stagesViewModel;
        public ObservableCollection<ScheduleViewModel> FutureAppointmentsViewModels
        {
            get
            {
                return futureAppointmentsViewModels;
            }
            set
            {
                if(futureAppointmentsViewModels != value)
                {
                    futureAppointmentsViewModels = value;
                    OnPropertyChanged();
                }
            }
        }
        private ObservableCollection<ScheduleViewModel> futureAppointmentsViewModels;
        public StagesView()
        {
            stagesViewModel = new StagesViewModel();
            InitializeComponent();
            DataContext = stagesViewModel;
            futureAppointmentsSection.DataContext = this;
        }
        public void LoadAllPatientStages(PatientViewModel patientToSet)
        {
            stagesViewModel.LoadAllPatientStages(patientToSet);
            if(FutureAppointmentsViewModels == null)
            {
                FutureAppointmentsViewModels = new ObservableCollection<ScheduleViewModel>(HttpService.GetPatientFutureAppointments(patientToSet.PatientId).Select(s => new ScheduleViewModel(s)).ToList());
            }
        }
        public void LoadAllPatientStagesWithScheduleMarked(DateTime date, int patientId)
        {
            stagesViewModel.LoadAllPatientStagesWithRelatedMarked(date, patientId);
            if (FutureAppointmentsViewModels == null)
            {
                FutureAppointmentsViewModels = new ObservableCollection<ScheduleViewModel>(HttpService.GetPatientFutureAppointments(patientId).Select(s => new ScheduleViewModel(s)).ToList());
            }
        }
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = sender as System.Windows.Controls.Primitives.Thumb;
            var grid = thumb.Parent as Grid;
            var textBox = grid.Children.OfType<TextBox>().FirstOrDefault();

            if (textBox != null)
            {
                double newHeight = textBox.ActualHeight + e.VerticalChange;

                if (newHeight > textBox.MinHeight)
                {
                    textBox.Height = newHeight;
                }
            }
        }
        

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Check if the Enter key was pressed
                if (e.Key == Key.Enter)
                {
                    // Set the height back to auto
                    textBox.Height = double.NaN;
                }
            }
        }
        public void TabActivated()
        {
        }

        public void TabDeactivated()
        {
            stagesViewModel.ServerUpdateStages();
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.PatientStagesUpdated, stagesViewModel.Patient.PatientId);
        }

        public void TabClosed()
        {
        }

        public void Notify(int notificationCode, object param)
        {
            switch (notificationCode)
            {
                case NotificationCodes.PatientStagesUpdated:
                    int patientId = (int)param;
                    Notification_PatientStagesUpdated(patientId);
                    break;
                case NotificationCodes.ScheduleRecordDeleted:
                    int recordId = (int)param;
                    Notification_ScheduleRecordDeleted(recordId);
                    break;
                case NotificationCodes.ScheduleRecordAdded:
                    Schedule newRecord = (Schedule)param;
                    Notification_ScheduleRecordAdded(newRecord);
                    break;
                case NotificationCodes.ScheduleRecordUpdated:
                    Schedule schedule = (Schedule)param;
                    Notification_ScheduleRecordUpdated(schedule);
                    break;
                
            }
        }
        private void Notification_PatientStagesUpdated(int patientId)
        {
            if (stagesViewModel.Patient.PatientId == patientId)
            {
                if (stagesViewModel.MarkedDate != null)
                    stagesViewModel.LoadAllPatientStagesWithRelatedMarked(stagesViewModel.MarkedDate.Value, stagesViewModel.Patient.PatientId);
                else
                {
                    try
                    {
                        stagesViewModel.LoadAllPatientStages(stagesViewModel.Patient);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {e.Message}");
                        return;
                    }
                }
            }
        }
        private void Notification_ScheduleRecordDeleted(int recordId)
        {
            if (FutureAppointmentsViewModels != null)
            {
                ScheduleViewModel item = FutureAppointmentsViewModels.FirstOrDefault(s => s.Id == recordId);
                FutureAppointmentsViewModels.Remove(item);
            }
        }
        private void Notification_ScheduleRecordAdded(Schedule newRecord)
        {
            if(newRecord.PatientId != stagesViewModel.Patient.PatientId) { return; } //check if record for this patient
            
            ScheduleViewModel scheduleViewModel = new ScheduleViewModel(new Schedule(newRecord));
            if (FutureAppointmentsViewModels == null)
            {
                FutureAppointmentsViewModels = new ObservableCollection<ScheduleViewModel>();
            }
            int index = FutureAppointmentsViewModels.ToList().FindIndex(s => s.StartDateTimeDT > scheduleViewModel.StartDateTimeDT);
            if (index >= 0)
            {
                FutureAppointmentsViewModels.Insert(index, scheduleViewModel);
            }
            else
            {
                FutureAppointmentsViewModels.Add(scheduleViewModel);
            }
        }
        private void Notification_ScheduleRecordUpdated(Schedule updatedRecord)
        {
            if (FutureAppointmentsViewModels != null) //if records exist
            {
                ScheduleViewModel scheduleViewModel = FutureAppointmentsViewModels.FirstOrDefault(s => s.Id == updatedRecord.Id); //find record with updated id
                if (scheduleViewModel != null) //if updated record exists
                {
                    if(scheduleViewModel.schedule.PatientId != updatedRecord.PatientId) //if patient changed
                    {
                        FutureAppointmentsViewModels.Remove(scheduleViewModel);
                    }
                    else //try update time if patient is the same
                    {
                        if(updatedRecord.StartDatetime != scheduleViewModel.StartDateTime)
                        {
                            scheduleViewModel.StartDateTimeDT = DateTime.ParseExact(updatedRecord.StartDatetime, Options.DateTimePattern, null);
                            scheduleViewModel.StartDateTime = scheduleViewModel.StartDateTime;
                            FutureAppointmentsViewModels = new ObservableCollection<ScheduleViewModel>(FutureAppointmentsViewModels.OrderBy(s => s.StartDateTimeDT));
                        }
                    }
                    return; //leave function if record was unchanged or deleted
                }
            }
            if (updatedRecord.PatientId == stagesViewModel.Patient.PatientId)
            {
                Notification_ScheduleRecordAdded(updatedRecord);
            }
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ScheduleViewModel item = ((ListViewItem)sender).DataContext as ScheduleViewModel;
            DateTime scheduleStartDate = DateTime.ParseExact(item.StartDateTime, Options.DateTimePattern, null);
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(ScreenNames.SCHEDULE) == false)
            {
                BrowserTabButton scheduleTabButton = new BrowserTabButton();
                scheduleTabButton.TabText = ScreenNames.SCHEDULE;
                ScheduleMenuView newScheduleMenuView = new ScheduleMenuView();
                scheduleTabButton.Control = newScheduleMenuView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(scheduleTabButton);
            }
            ScheduleMenuView scheduleMenuView = Options.MainWindow.mainMenu.browserControl.SelectedTab.Control as ScheduleMenuView;
            ScheduleMenuView.DesiredVerticalScrollOffset = null;
            scheduleMenuView.SelectedDate= scheduleStartDate;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
