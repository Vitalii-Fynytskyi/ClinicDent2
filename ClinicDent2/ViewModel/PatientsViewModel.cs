﻿using ClinicDent2.Commands;
using ClinicDent2.TabbedBrowser;
using ClinicDentClientCommon;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public enum PatientListMode
    {
        MyPatients, AllPatients, SearchPatientsWithImage
    }
    public class PatientsViewModel:BaseViewModel
    {
        
        private Doctor selectedDoctor = null;
        public Doctor SelectedDoctor
        {
            get
            {
                return selectedDoctor;
            }
            set
            {
                if(value != selectedDoctor)
                {
                    selectedDoctor = value;
                    NotifyPropertyChanged();
                    SelectedPage = 1;
                    ReceivePatients();
                }
            }
        }
        private PatientListMode _patientListMode;
        public PatientListMode patientListMode
        {
            get { return _patientListMode; }
            set
            {
                if (_patientListMode != value)
                {
                    _patientListMode = value;
                    NotifyPropertyChanged();
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
                if(visiblePages != value)
                {
                    visiblePages = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private ObservableCollection<PatientViewModel> patientViewModels;
        public ObservableCollection<PatientViewModel> PatientViewModels
        {
            get { return patientViewModels; }
            set
            {
                if(patientViewModels != value)
                {
                    patientViewModels = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private int? imageId;
        public PatientsViewModel(PatientListMode patientListModeToSet, int? imageIdToSet=null)
        {
            patientListMode= patientListModeToSet;
            SearchPressedCommand = new RelayCommand(searchPressed);
            PageChangedCommand = new RelayCommand(pageChanged);
            selectedDoctor = SharedData.AllDoctors.FirstOrDefault(d => d.Id == SharedData.CurrentDoctor.Id);
            switch(patientListModeToSet)
            {
                case PatientListMode.MyPatients:
                    PageTitle= ScreenNames.MY_PATIENTS;
                    break;
                case PatientListMode.AllPatients:
                    PageTitle = ScreenNames.ALL_PATIENTS;
                    break;
                case PatientListMode.SearchPatientsWithImage:
                    PageTitle = ScreenNames.PATIENTS_WITH_REQUESTED_IMAGE;
                    break;

            }
            imageId= imageIdToSet;
            selectedSorting = Patient.AvailableSorts[0];
            selectedStatus = Patient.AvailableStatuses[0];
            selectedPage = 1;
            searchText= string.Empty;
            visiblePages = new ObservableCollection<int>();
            ReceivePatients();
        }
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if(searchText != value)
                {
                    searchText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string pageTitle;
        public string PageTitle
        {
            get { return pageTitle; }
            set
            {
                if (pageTitle != value)
                {
                    pageTitle = value;
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
                if(countPages != value)
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
        private string selectedStatus;
        public string SelectedStatus
        {
            get { return selectedStatus; }
            set
            {
                if (selectedStatus != value)
                {
                    selectedStatus = value;
                    NotifyPropertyChanged();
                    SelectedPage = 1;
                    ReceivePatients();
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
        public async Task ReceivePatients()
        {
            string searchTextForRequest = SearchText == "" ? "<null>" : SearchText;
            PatientsToClient patientsToClient = null;
            try
            {
                switch (patientListMode)
                {
                    case PatientListMode.AllPatients:
                        patientsToClient= await HttpService.GetPatients(selectedStatus, selectedSorting, selectedPage, Options.PatientsPerPage, searchTextForRequest);
                        break;
                    case PatientListMode.MyPatients:
                        patientsToClient = await HttpService.GetPatients(selectedStatus, selectedSorting, selectedPage, Options.PatientsPerPage, searchTextForRequest, SelectedDoctor.Id);
                        break;
                    case PatientListMode.SearchPatientsWithImage:
                        patientsToClient = await HttpService.GetPatientsByImage(imageId.Value, selectedStatus, selectedSorting, selectedPage, Options.PatientsPerPage, searchTextForRequest);
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Не вдалось отримати список пацієнтів: {ex.Message}");
                return;
            }
            CountPages = patientsToClient.CountPages;
            PatientViewModels = new ObservableCollection<PatientViewModel>(patientsToClient.Patients.Select(p=>new PatientViewModel(p)));
            createVisiblePages();

        }
        private void createVisiblePages()
        {
            if(CountPages<=1)
            {
                VisiblePages.Clear();
                return;
            }
            List<int> pages = new List<int>(15);
            int pageToAdd = SelectedPage;
            pages.Add(pageToAdd);
            pageToAdd--;
            while(pageToAdd>0 && pageToAdd >= SelectedPage - 7)
            {
                pages.Insert(0, pageToAdd);
                pageToAdd--;
            }
            pageToAdd = SelectedPage + 1;
            while(CountPages>=pageToAdd && pageToAdd <= SelectedPage + 7)
            {
                pages.Add(pageToAdd);
                pageToAdd++;
            }
            VisiblePages = new ObservableCollection<int>(pages);
        }
    }
}
