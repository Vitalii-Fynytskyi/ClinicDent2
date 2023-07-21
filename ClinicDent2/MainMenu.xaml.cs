using ClinicDent2.Model;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using ClinicDent2.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace ClinicDent2
{
    public partial class MainMenu : UserControl
    {
        public MainMenu(Doctor doctorToSet)
        {
            Options.CurrentDoctor = doctorToSet;
            InitializeComponent();
            Options.AllDoctors = HttpService.GetDoctors();
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            StageViewModel.LoadAssets();
        }

        public void createFullSizeImageControl(byte[] imageSourceToDisplay)
        {
            FullSizeImageControl fullSizeImageControl = new FullSizeImageControl(imageSourceToDisplay);
            fullSizeImageControl.Show();
        }
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Options.MainWindow.CloseApp();
        }
        private void buttonCreatePatient_Click(object sender, RoutedEventArgs e)
        {
            if(browserControl.ScreenRequested(ScreenNames.NEW_PATIENT) == false)
            {
                BrowserTabButton createPatient = new BrowserTabButton();
                createPatient.TabText = ScreenNames.NEW_PATIENT;
                createPatient.Control = new NewPatientView();
                browserControl.AddNewTab(createPatient);
            }
        }

        private void buttonExitAccount_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(Environment.CurrentDirectory + "\\data\\cookies.dat");
            Options.MainWindow.goToLoginMenu();
        }

        private void buttonSchedule_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window w in App.Current.Windows)
            {
                if (w.Title == "Розклад")
                {
                    w.WindowState= WindowState.Normal;
                    w.Focus();
                    return;
                }
            }
            WindowContainer windowContainer= new WindowContainer();
            windowContainer.Title = "Розклад";
            windowContainer.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            windowContainer.WindowState = WindowState.Maximized;
            ScheduleMenuView scheduleMenuView = new ScheduleMenuView();
            windowContainer.Content = scheduleMenuView;
            windowContainer.Show();
            scheduleMenuView.scrollViewerSchedule.ScrollToVerticalOffset(scheduleMenuView.ScheduleForDayViews[scheduleMenuView.SelectedDate.Day - 1].ActualHeight * (scheduleMenuView.SelectedDate.Day - 1));
            
        }

        private void buttonPatients_Click(object sender, RoutedEventArgs e)
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

        private void buttonPhotos_Click(object sender, RoutedEventArgs e)
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

        private void buttonUnpayedStages_Click(object sender, RoutedEventArgs e)
        {
            if (browserControl.ScreenRequested(ScreenNames.DEBTORS) == false)
            {
                BrowserTabButton debtors = new BrowserTabButton();
                debtors.TabText = ScreenNames.DEBTORS;
                PatientsView patientsView = new PatientsView();
                patientsView.PatientsViewModel = new PatientsViewModel(PatientListMode.Debtors);
                debtors.Control = patientsView;
                browserControl.AddNewTab(debtors);
            }
        }

        private void buttonAllPatients_Click(object sender, RoutedEventArgs e)
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

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
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

        private void buttonStatistics_Click(object sender, RoutedEventArgs e)
        {
            BrowserTabButton settings = new BrowserTabButton();
            settings.TabText = "Статистика";
            StatisticsView statisticsView = new StatisticsView();
            settings.Control = statisticsView;
            browserControl.AddNewTab(settings);
        }
    }
}
