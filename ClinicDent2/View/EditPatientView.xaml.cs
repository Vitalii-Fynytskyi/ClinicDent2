using ClinicDent2.TabbedBrowser;
using ClinicDent2.ViewModel;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicDent2.View
{
    /// <summary>
    /// Interaction logic for EditPatientView.xaml
    /// </summary>
    public partial class EditPatientView : UserControl
    {
        PatientViewModel patientViewModel;
        public EditPatientView(PatientViewModel patientViewModelToSet)
        {
            patientViewModel = patientViewModelToSet;
            DataContext = patientViewModelToSet;
            InitializeComponent();
            fillListBoxStatuses();
        }
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                patientViewModel.PutPatient();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось оновити дані пацієнта: {ex.Message}", "Помилка");
                return;
            }
            Options.MainWindow.mainMenu.browserControl.RemoveTabIfInBrowser(this);
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(patientViewModel.PatientId, TabButtonType.PatientStages) == false)
            {
                BrowserTabButton stagesPatient = new BrowserTabButton();
                stagesPatient.TabText = patientViewModel.Name;
                stagesPatient.PatientViewModel = patientViewModel;
                stagesPatient.ButtonType = TabButtonType.PatientStages;
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
                stagesPatient.Control = stagesView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(stagesPatient);
            }
        }
        private void listBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newStatuses = "";
            foreach (string item in listBoxStatus.SelectedItems)
            {
                newStatuses += item + "|";
            }
            patientViewModel.Statuses = newStatuses;
        }
        private void fillListBoxStatuses()
        {
            string[] statuses = patientViewModel.Statuses.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string status in statuses)
            {
                listBoxStatus.SelectedItems.Add(status);
            }
        }
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                HttpService.DeletePatient(patientViewModel.PatientId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось видалити пацієнта: {ex.Message}", "Помилка");
                return;
            }
            //close stage tab if exists
            BrowserTabButton tabButton = Options.MainWindow.mainMenu.browserControl.GetTabButton(patientViewModel.PatientId, TabButtonType.PatientStages);
            if (tabButton != null)
            {
                Options.MainWindow.mainMenu.browserControl.RemoveTab(tabButton);
            }
            Options.MainWindow.mainMenu.browserControl.RemoveTabIfInBrowser(this);
            
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Options.MainWindow.mainMenu.browserControl.RemoveTabIfInBrowser(this);
        }

        private void buttonSelectImageFromDisk_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png *.bmp | All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                patientViewModel.ImageBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
        }
        private void patientPhoto_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Options.MainWindow.mainMenu.createFullSizeImageControl(patientViewModel.ImageBytes);
        }
    }
}
