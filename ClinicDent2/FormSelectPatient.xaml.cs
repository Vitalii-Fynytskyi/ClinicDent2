using ClinicDent2.Model;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ClinicDent2
{
    /// <summary>
    /// Логика взаимодействия для FormSelectPatient.xaml
    /// </summary>
    public partial class FormSelectPatient : UserControl
    {
        Label labelThreeDots1;
        Label labelThreeDots2;
        Model.Patient[] shownPatients;
        Patient selectedPatient;
        PatientsToClient receivedPatients;
        public event EventHandler<Patient> PatientSelected;


        static string selectedStatus = "Всі статуси";
        static string searchText = "";
        static string selectedSortDescription = "Ім'я: від А до Я";
        static string selectedPage = "1";
        static double verticalScrollOffset = 0;
        public FormSelectPatient()
        {
            InitializeComponent();

            labelThreeDots1 = new Label();
            labelThreeDots1.Content = "...";
            labelThreeDots2 = new Label();
            labelThreeDots2.Content = "...";
            setAppropriateFields();
            comboBoxSorting.SelectionChanged += comboBoxSorting_SelectionChanged;
            comboBoxStatus.SelectionChanged += comboBoxStatus_SelectionChanged;
            textBoxSearchPatient.KeyDown += textBoxSearchPatient_KeyDown;
            if (Convert.ToInt32(selectedPage) <= 0)
                selectedPage = "1";
            ReceivePatients();

        }
        public void ReceivePatients()
        {
            if (searchText == "")
                searchText = "<null>";
            try
            {
                receivedPatients = HttpService.GetPatients(selectedStatus, selectedSortDescription, Convert.ToInt32(selectedPage), Options.PatientsPerPage, searchText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось завантажити пацієнтів: {ex.Message}", "Помилка");
                return;
            }
            shownPatients = receivedPatients.Patients;
            constructPagesPanel(selectedPage, receivedPatients.CountPages);
            addPatients();
            scrollViewer.ScrollToVerticalOffset(verticalScrollOffset);
        }

        public void constructPagesPanel(string activePage, int countPages)
        {
            if (Convert.ToInt32(activePage) > countPages)
            {
                activePage = countPages.ToString();
                selectedPage = activePage;
            }
            if (countPages < 8)
            {
                for (int i = 0; i < countPages; ++i)
                {
                    ToggleButton buttonToAdd = new ToggleButton();
                    buttonToAdd.MinWidth = 20;
                    buttonToAdd.Margin = new Thickness(5, 0, 0, 0);
                    buttonToAdd.Content = (i + 1).ToString();
                    if (((string)buttonToAdd.Content) == activePage)
                    {
                        buttonToAdd.IsChecked = true;
                    }
                    buttonToAdd.Click += PageButton_Click;
                    panelPages.Children.Add(buttonToAdd);
                }

            }
            else
            {
                ///add current active page
                ToggleButton currentButton = new ToggleButton();
                currentButton.MinWidth = 20;
                currentButton.Margin = new Thickness(5, 0, 0, 0);
                currentButton.Content = activePage;
                currentButton.Click += PageButton_Click;
                currentButton.IsChecked = true;
                panelPages.Children.Add(currentButton);
                ///insert buttons leftside
                int leftSideAmount = 0;
                int nActivePage = Convert.ToInt32(activePage) - 1;
                while (leftSideAmount != 7 && nActivePage != 0)
                {
                    ToggleButton buttonToAdd = new ToggleButton();
                    buttonToAdd.MinWidth = 20;
                    buttonToAdd.Margin = new Thickness(5, 0, 0, 0);
                    buttonToAdd.Content = nActivePage.ToString();
                    --nActivePage;
                    leftSideAmount++;
                    buttonToAdd.Click += PageButton_Click;
                    panelPages.Children.Insert(0, buttonToAdd);
                }
                if (nActivePage > 0)
                {
                    panelPages.Children.Insert(0, labelThreeDots1);
                }
                ///insert buttons rightside
                int rightSideAmount = 0;
                nActivePage = Convert.ToInt32(activePage) + 1;
                while (rightSideAmount != 7 && nActivePage != countPages + 1)
                {
                    ToggleButton buttonToAdd = new ToggleButton();
                    buttonToAdd.MinWidth = 20;
                    buttonToAdd.Margin = new Thickness(5, 0, 0, 0);
                    buttonToAdd.Content = nActivePage.ToString();
                    nActivePage++;
                    rightSideAmount++;
                    buttonToAdd.Click += PageButton_Click;
                    panelPages.Children.Add(buttonToAdd);
                }
                if (nActivePage != countPages + 1)
                {
                    panelPages.Children.Add(labelThreeDots2);
                }
            }
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton target = sender as ToggleButton;
            selectedPage = target.Content as string;
            verticalScrollOffset = 0;
            scrollViewer.ScrollToVerticalOffset(0);
            removePatients();
            removePages();
            ReceivePatients();
        }

        private void addPatients()
        {
            listPatients.ItemsSource = shownPatients;
        }

        private void removePatients()
        {
            listPatients.ItemsSource = null;
        }

        private void removePages()
        {
            panelPages.Children.Clear();
        }

        void setAppropriateFields()
        {
            if (searchText == "<null>")
                searchText = "";
            textBoxSearchPatient.Text = searchText;
            comboBoxStatus.SelectedItem = selectedStatus;
            comboBoxSorting.SelectedItem = selectedSortDescription;
        }

        private void buttonEditPatient_Click(object sender, RoutedEventArgs e)
        {
            Button target = sender as Button;
            Patient patient = target.DataContext as Patient;
            //TODO switch to edit patient screen
            //Options.MainWindow.mainMenu.switchToEditPatient(patient);
            e.Handled = true;
        }

        private void textBoxSearchPatient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                searchText = textBoxSearchPatient.Text;
                selectedPage = "1";
                removePages();
                removePatients();
                ReceivePatients();
            }
        }

        private void comboBoxSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPage = "1";
            selectedSortDescription = comboBoxSorting.SelectedItem as string;
            removePages();
            removePatients();
            ReceivePatients();

        }

        private void comboBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedStatus = comboBoxStatus.SelectedItem as string;
            selectedPage = "1";
            removePages();
            removePatients();
            ReceivePatients();

        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            verticalScrollOffset = scrollViewer.VerticalOffset;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            searchText = textBoxSearchPatient.Text;
            selectedPage = "1";
            removePages();
            removePatients();
            ReceivePatients();
        }

        private void RadioButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RadioButton target = sender as RadioButton;
            Patient patient = target.DataContext as Patient;
            selectedPatient = patient;
            if (selectedPatient != null)
            {
                PatientSelected?.Invoke(this, selectedPatient);
            }
            Window.GetWindow(this).Close();
        }
    }
}
