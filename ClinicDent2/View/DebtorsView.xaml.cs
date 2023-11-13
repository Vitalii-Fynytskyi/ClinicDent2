using ClinicDent2.TabbedBrowser;
using ClinicDent2.ViewModel;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2.View
{
    /// <summary>
    /// Interaction logic for DebtorsView.xaml
    /// </summary>
    public partial class DebtorsView : UserControl
    {
        private DebtorsViewModel patientsViewModel;
        public DebtorsViewModel PatientsViewModel
        {
            get { return patientsViewModel; }
            set
            {
                patientsViewModel = value;
                DataContext = patientsViewModel;
            }
        }
        public DebtorsView()
        {
            InitializeComponent();
        }
        private void ButtonPatient_Click(object sender, RoutedEventArgs e)
        {
            PatientViewModel patientViewModel = (sender as Button).DataContext as PatientViewModel;
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(patientViewModel.PatientId, TabButtonType.PatientStages) == false)
            {
                BrowserTabButton patientStagesButton = new BrowserTabButton();
                patientStagesButton.TabText = patientViewModel.Name;
                patientStagesButton.PatientViewModel = patientViewModel;
                patientStagesButton.ButtonType = TabButtonType.PatientStages;
                StagesView stagesView = new StagesView();
                try
                {
                    stagesView.LoadAllPatientStages(patientViewModel);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {ex.Message}");
                    return;
                }
                patientStagesButton.Control = stagesView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(patientStagesButton);
            }

        }

        private void ButtonEditPatient_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            PatientViewModel patientViewModel = (sender as Button).DataContext as PatientViewModel;
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(patientViewModel.PatientId, TabButtonType.PatientDataEdit) == false)
            {
                BrowserTabButton editPatientDataButton = new BrowserTabButton();
                editPatientDataButton.TabText = patientViewModel.Name + " (Дані)";
                editPatientDataButton.PatientViewModel = patientViewModel;
                editPatientDataButton.ButtonType = TabButtonType.PatientDataEdit;
                EditPatientView editPatientView = new EditPatientView(patientViewModel);
                editPatientDataButton.Control = editPatientView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(editPatientDataButton);
            }

        }

        private void PageButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int buttonPage = (int)button.Content;
            int selectedPage = patientsViewModel.SelectedPage;
            if (buttonPage == selectedPage)
            {
                button.Background = Brushes.Blue;
                button.Foreground = Brushes.White;
            }
            else
            {
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Black;
            }
        }
    }
}
