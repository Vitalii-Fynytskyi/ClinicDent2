using ClinicDent2.Commands;
using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public class DebtorsViewModel:BaseViewModel
    {
        public static string[] AvailableSorts { get; set; } = new string[] { "За замовчуванням", "Ім'я: від А до Я", "Ім'я: від Я до А", "За замовчуванням навпаки", "Дата реєстрації: спочатку недавні", "Дата реєстрації: спочатку старіші", "Вік: спочатку молодші", "Вік: спочатку старші", "Сума боргу: спочатку більші", "Сума боргу: спочатку менші" };
        private Doctor selectedDoctor = null;
        public Doctor SelectedDoctor
        {
            get
            {
                return selectedDoctor;
            }
            set
            {
                if (value != selectedDoctor)
                {
                    selectedDoctor = value;
                    NotifyPropertyChanged();
                    SelectedPage = 1;
                    ReceivePatients();
                }
            }
        }
        private ObservableCollection<int> visiblePages;
        public ObservableCollection<int> VisiblePages
        {
            get
            {
                return visiblePages;
            }
            set
            {
                if (visiblePages != value)
                {
                    visiblePages = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private ObservableCollection<PatientWithDebtViewModel> patientViewModels;
        public ObservableCollection<PatientWithDebtViewModel> PatientViewModels
        {
            get { return patientViewModels; }
            set
            {
                if (patientViewModels != value)
                {
                    patientViewModels = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DebtorsViewModel()
        {
            SearchPressedCommand = new RelayCommand(searchPressed);
            PageChangedCommand = new RelayCommand(pageChanged);
            selectedDoctor = Options.AllDoctors.FirstOrDefault(d => d.Id == Options.CurrentDoctor.Id);
            selectedSorting = AvailableSorts[0];
            selectedPage = 1;
            searchText = string.Empty;
            visiblePages = new ObservableCollection<int>();
            ReceivePatients();
        }
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string selectedSorting;
        public string SelectedSorting
        {
            get { return selectedSorting; }
            set
            {
                if (selectedSorting != value)
                {
                    selectedSorting = value;
                    NotifyPropertyChanged();
                    SelectedPage = 1;
                    ReceivePatients();
                }
            }
        }
        private int countPages;
        public int CountPages
        {
            get { return countPages; }
            set
            {
                if (countPages != value)
                {
                    countPages = value;
                    if (SelectedPage > CountPages)
                    {
                        SelectedPage = CountPages;
                    }
                    NotifyPropertyChanged();
                }
            }
        }
        private int selectedPage;
        public int SelectedPage
        {
            get { return selectedPage; }
            set
            {
                if (selectedPage != value)
                {
                    selectedPage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RelayCommand SearchPressedCommand { get; set; }
        private void searchPressed(object param)
        {
            SelectedPage = 1;
            ReceivePatients();
        }
        public RelayCommand PageChangedCommand { get; set; }
        private void pageChanged(object param) //param is page in string format
        {
            SelectedPage = Convert.ToInt32(param.ToString());
            ReceivePatients();
        }
        public void ReceivePatients()
        {
            string searchTextForRequest = SearchText == "" ? "<null>" : SearchText;
            DebtPatientsToClient patientsToClient = null;
            try
            {
                patientsToClient = HttpService.GetDebtors(selectedSorting, selectedPage, Options.PatientsPerPage, searchTextForRequest, SelectedDoctor.Id);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось отримати список боржників: {ex.Message}");
                return;
            }
            CountPages = patientsToClient.CountPages;
            PatientViewModels = new ObservableCollection<PatientWithDebtViewModel>(patientsToClient.Patients.Select(p => new PatientWithDebtViewModel(p)));
            createVisiblePages();

        }
        private void createVisiblePages()
        {
            if (CountPages <= 1)
            {
                VisiblePages.Clear();
                return;
            }
            List<int> pages = new List<int>(15);
            int pageToAdd = SelectedPage;
            pages.Add(pageToAdd);
            pageToAdd--;
            while (pageToAdd > 0 && pageToAdd >= SelectedPage - 7)
            {
                pages.Insert(0, pageToAdd);
                pageToAdd--;
            }
            pageToAdd = SelectedPage + 1;
            while (CountPages >= pageToAdd && pageToAdd <= SelectedPage + 7)
            {
                pages.Add(pageToAdd);
                pageToAdd++;
            }
            VisiblePages = new ObservableCollection<int>(pages);
        }
    }
}
