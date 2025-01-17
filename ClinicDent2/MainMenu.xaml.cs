using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using ClinicDent2.ViewModel;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon;
using ClinicDentClientCommon.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicDentClientCommon.TcpClientToServer;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ClinicDent2
{
    public partial class MainMenu : UserControl
    {
        public TcpClient TcpClient;
        public MainMenu(Doctor doctorToSet)
        {
            Init();
            SharedData.CurrentDoctor = doctorToSet;
            InitializeComponent();
            if (Options.DefaultSelectedCabinet == null)
            {
                MessageBox.Show("Неправильно задано DefaultSelectedTable в .ini файлі.", "Помилка");
            }
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            StageViewModel.LoadAssets().Wait();

            TcpClient = new TcpClient(HttpService.httpClient.BaseAddress, Convert.ToInt32(IniService.GetPrivateString("Settings", "TcpPort")));
            TcpClient.ScheduleRecordAdded += TcpClient_ScheduleRecordAdded;
            TcpClient.ScheduleRecordUpdated += TcpClient_ScheduleRecordUpdated;
            TcpClient.ScheduleRecordDeleted += TcpClient_ScheduleRecordDeleted;
            TcpClient.ScheduleRecordStateUpdated += TcpClient_ScheduleRecordStateUpdated;
            TcpClient.ScheduleRecordCommentUpdated += TcpClient_ScheduleRecordCommentUpdated;
            TcpClient.ScheduleCabinetCommentUpdated += TcpClient_ScheduleCabinetCommentUpdated;
            TcpClient.StagePayInfoUpdated += TcpClient_StagePayInfoUpdated;
            TcpClient.ConnectToServer();
        }
        private void Init()
        {
            SharedData.AllDoctors = HttpService.GetDoctors().Result;
            SharedData.AllCabinets = HttpService.GetCabinets().Result;
            Options.DefaultSelectedCabinet = SharedData.AllCabinets.FirstOrDefault(c => c.Id == Options.DefaultSelectedTable);
        }
        #region TcpServerEvents
        private void TcpClient_StagePayInfoUpdated(string updatedStagesContent)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleStagesPaymentUpdated, updatedStagesContent);
            }));
        }

        private void TcpClient_ScheduleCabinetCommentUpdated(string datetime, int cabinetId, string newComment)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleCabinetCommentUpdated, (datetime, cabinetId, newComment));
            }));
        }

        private void TcpClient_ScheduleRecordCommentUpdated(int recordId, string newComment, string dateTime, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordCommentUpdated, (recordId, newComment, dateTime, cabinetId));
            }));
        }

        private void TcpClient_ScheduleRecordStateUpdated(int recordId, SchedulePatientState newState, string dateTime, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordStateUpdated, (recordId, newState, dateTime, cabinetId));
            }));
        }

        private void TcpClient_ScheduleRecordDeleted(int recordId, string dateTime, int cabinetId)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordDeleted, (recordId, dateTime, cabinetId));
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordDeletedIdOnly, recordId);
            }));
        }

        private void TcpClient_ScheduleRecordUpdated(object sender, Schedule e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordUpdated, e);
            }));
        }

        private void TcpClient_ScheduleRecordAdded(object sender, Schedule e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserControl.NotifyAllTabs(NotificationCodes.ScheduleRecordAdded, e);
            }));
        }
        #endregion

        public void createFullSizeImageControl(byte[] imageSourceToDisplay)
        {
            FullSizeImageControl fullSizeImageControl = new FullSizeImageControl(imageSourceToDisplay);
            fullSizeImageControl.Show();
        }
        #region NavBarEvents
        private void buttonExit_Click(object sender, MouseButtonEventArgs e)
        {
            Options.MainWindow.CloseApp();
        }
        private void buttonCreatePatient_Click(object sender, MouseButtonEventArgs e)
        {
            if(browserControl.ScreenRequested(ScreenNames.NEW_PATIENT) == false)
            {
                BrowserTabButton createPatient = new BrowserTabButton();
                createPatient.TabText = ScreenNames.NEW_PATIENT;
                createPatient.Control = new NewPatientView();
                browserControl.AddNewTab(createPatient);
            }
        }

        private void buttonExitAccount_Click(object sender, MouseButtonEventArgs e)
        {
            TcpClient.DisconnectFromServer();
            File.Delete(Environment.CurrentDirectory + "\\data\\cookies.dat");
            Options.MainWindow.goToLoginMenu();
        }
        private void buttonSchedule_Click(object sender, MouseButtonEventArgs e)
        {

            if (browserControl.ScreenRequested(ScreenNames.SCHEDULE) == false)
            {
                BrowserTabButton scheduleTabButton = new BrowserTabButton();
                scheduleTabButton.TabText = ScreenNames.SCHEDULE;
                ScheduleMenuView scheduleMenuView = new ScheduleMenuView();
                scheduleTabButton.Control = scheduleMenuView;
                browserControl.AddNewTab(scheduleTabButton);
            }
        }

        private void buttonPatients_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.MY_PATIENTS) == false)
            {
                BrowserTabButton myPatients = new BrowserTabButton();
                myPatients.TabText = ScreenNames.MY_PATIENTS;
                PatientsView patientsView= new PatientsView();
                patientsView.PatientsViewModel = new PatientsViewModel(PatientListMode.MyPatients);
                myPatients.Control = patientsView;
                browserControl.AddNewTab(myPatients);
            }
        }

        private void buttonPhotos_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.PHOTOFLOW) == false)
            {
                BrowserTabButton photoflowButton = new BrowserTabButton();
                photoflowButton.TabText = ScreenNames.PHOTOFLOW;
                PhotoflowView photoflowView = new PhotoflowView();
                photoflowButton.Control = photoflowView;
                browserControl.AddNewTab(photoflowButton);
            }
        }

        private void buttonUnpayedStages_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.DEBTORS) == false)
            {
                BrowserTabButton debtors = new BrowserTabButton();
                debtors.TabText = ScreenNames.DEBTORS;
                DebtorsView patientsView = new DebtorsView();
                patientsView.PatientsViewModel = new DebtorsViewModel();
                debtors.Control = patientsView;
                browserControl.AddNewTab(debtors);
            }
        }

        private void buttonAllPatients_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.ALL_PATIENTS) == false)
            {
                BrowserTabButton allPatients = new BrowserTabButton();
                allPatients.TabText = ScreenNames.ALL_PATIENTS;
                PatientsView patientsView = new PatientsView();
                patientsView.PatientsViewModel = new PatientsViewModel(PatientListMode.AllPatients);
                allPatients.Control = patientsView;
                browserControl.AddNewTab(allPatients);
            }
        }

        private void buttonSettings_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.SETTINGS) == false)
            {
                BrowserTabButton settings = new BrowserTabButton();
                settings.TabText = ScreenNames.SETTINGS;
                SettingsView settingsView = new SettingsView();
                settings.Control = settingsView;
                browserControl.AddNewTab(settings);
            }
        }

        private void buttonStatistics_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.STATISTICS) == false)
            {
                BrowserTabButton statistics = new BrowserTabButton();
                statistics.TabText = ScreenNames.STATISTICS;
                StatisticsView statisticsView = new StatisticsView();
                statistics.Control = statisticsView;
                browserControl.AddNewTab(statistics);
            }
        }

        private void buttonUnderObservation_Click(object sender, MouseButtonEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.UNDER_OBSERVATION) == false)
            {
                BrowserTabButton underObservation = new BrowserTabButton();
                underObservation.TabText = ScreenNames.UNDER_OBSERVATION;
                TeethUnderObservationView teethUnderObservationView  = new TeethUnderObservationView();
                underObservation.Control = teethUnderObservationView;
                browserControl.AddNewTab(underObservation);
            }
        }
        #endregion
        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility

            if (Tg_Btn.IsChecked == true)
            {
                tt_addNewPatient.Visibility = Visibility.Collapsed;
                tt_myPatients.Visibility = Visibility.Collapsed;
                tt_schedule.Visibility = Visibility.Collapsed;
                tt_photoflow.Visibility = Visibility.Collapsed;
                tt_debtors.Visibility = Visibility.Collapsed;
                tt_myPatients.Visibility = Visibility.Collapsed;
                tt_settings.Visibility = Visibility.Collapsed;
                tt_statistics.Visibility = Visibility.Collapsed;
                tt_underObservation.Visibility = Visibility.Collapsed;
                tt_exit.Visibility = Visibility.Collapsed;
                tt_exitAccount.Visibility = Visibility.Collapsed;

            }
            else
            {
                tt_addNewPatient.Visibility = Visibility.Visible;
                tt_myPatients.Visibility = Visibility.Visible;
                tt_schedule.Visibility = Visibility.Visible;
                tt_photoflow.Visibility = Visibility.Visible;
                tt_debtors.Visibility = Visibility.Visible;
                tt_myPatients.Visibility = Visibility.Visible;
                tt_settings.Visibility = Visibility.Visible;
                tt_statistics.Visibility = Visibility.Visible;
                tt_underObservation.Visibility = Visibility.Visible;
                tt_exit.Visibility = Visibility.Visible;
                tt_exitAccount.Visibility = Visibility.Visible;
            }
        }
    }
}
