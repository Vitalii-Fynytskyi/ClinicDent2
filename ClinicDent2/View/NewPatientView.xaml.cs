using ClinicDent2.Model;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.ViewModel;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ClinicDent2.View
{
    /// <summary>
    /// Interaction logic for NewPatientView.xaml
    /// </summary>
    public partial class NewPatientView : UserControl
    {
        PatientViewModel newPatientViewModel;
        public NewPatientView()
        {
            Patient p = new Patient()
            {
                RegisterDate = DateTime.Today.ToString(Options.DatePattern),
                Statuses=""
            };
            newPatientViewModel = new PatientViewModel(p);
            DataContext = newPatientViewModel;
            InitializeComponent();
        }
        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                newPatientViewModel.PostPatient();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось створити пацієнта: {ex.Message}", "Помилка");
                return;
            }
            BrowserTabButton stagesPatient = new BrowserTabButton();
            stagesPatient.TabText = newPatientViewModel.Name;
            stagesPatient.PatientViewModel = newPatientViewModel;
            stagesPatient.ButtonType = TabButtonType.PatientStages;
            StagesView stagesView = new StagesView();
            try
            {
                stagesView.LoadAllPatientStages(newPatientViewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {ex.Message}");
                return;
            }
            stagesPatient.Control = stagesView;
            Options.MainWindow.mainMenu.browserControl.RemoveTabIfInBrowser(this);
            Options.MainWindow.mainMenu.browserControl.AddNewTab(stagesPatient);
        }
        private void listBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newStatuses = "";
            foreach (string item in listBoxStatus.SelectedItems)
            {
                newStatuses += item + "|";
            }
            newPatientViewModel.Statuses = newStatuses;
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
                newPatientViewModel.ImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                labelPhotoPath.Content = System.IO.Path.GetFileName(openFileDialog.FileName);
            }
        }
    }
}
