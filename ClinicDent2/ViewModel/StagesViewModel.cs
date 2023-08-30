using ClinicDent2.Commands;
using ClinicDent2.Model;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public class StagesViewModel : BaseViewModel
    {
        public RelayCommand PhotoClickedCommand { get; set; }
        public RelayCommand EditPatientCommand { get; set; }
        public RelayCommand CreateNewStageCommand { get; set; }
        private void PhotoClicked(object arg)
        {
            Options.MainWindow.mainMenu.createFullSizeImageControl(patient.ImageBytes);
        }
        private void EditPatient(object arg)
        {
            if (Options.MainWindow.mainMenu.browserControl.ScreenRequested(patient.PatientId, TabButtonType.PatientDataEdit) == false)
            {
                BrowserTabButton editPatientDataButton = new BrowserTabButton();
                editPatientDataButton.TabText = patient.Name + " (Дані)";
                editPatientDataButton.PatientViewModel = patient;
                editPatientDataButton.ButtonType = TabButtonType.PatientDataEdit;
                EditPatientView editPatientView = new EditPatientView(patient);
                editPatientDataButton.Control = editPatientView;
                Options.MainWindow.mainMenu.browserControl.AddNewTab(editPatientDataButton);
                App.Current.MainWindow.Activate();
            }
        }

        internal void ServerUpdateStages()
        {
            try
            {
                HttpService.PutPatientCurePlan(patient.PatientId, patient.CurePlan);
            }
            catch
            {
                MessageBox.Show($"Не вдалось оновити план лікування пацієнта", "Помилка");
            }
            if(stages == null) { return; }
            foreach (StageViewModel stageViewModel in stages)
            {
                if (stageViewModel.IsOwner && stageViewModel.Error == string.Empty)
                {
                    try
                    {
                        HttpService.PutStage(stageViewModel.Stage);
                    }
                    catch
                    {
                        MessageBox.Show($"Не вдалось оновити дані роботи: {stageViewModel.Title}/{stageViewModel.StageDatetime}/{patient.Name}", "Помилка");
                    }
                }
            }
        }

        /// <summary>
        /// Create stage attached to schedule if schedule is available, otherwise create stage with no associated schedule 
        /// </summary>
        /// <param name="stageName">Name of the stage (string type)</param>
        private void CreateNewStage(object stageName)
        {
            Stage stage = new Stage();
            stage.PatientId = patient.PatientId;
            stage.DoctorId = Options.CurrentDoctor.Id;
            stage.DoctorName = "<null>";
            stage.Title = stageName as string;
            stage.StageDatetime = DateTime.Now.ToString(Options.DateTimePattern);
            try
            {
                stage = HttpService.PostStage(stage);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Не вдалось створити новий етап:{ex.Message}", "Помилка");
                return;
            }
            Stages.Insert(0, new StageViewModel(stage, this));
        }

        PatientViewModel patient;
        public PatientViewModel Patient
        {
            get
            {
                return patient;
            }
            set
            {
                if(value != patient)
                {
                    patient = value;
                    NotifyPropertyChanged();
                }
                
            }
        }
        ObservableCollection<StageViewModel> stages;
        public ObservableCollection<StageViewModel> Stages
        {
            get
            {
                return stages;
            }
            set
            {
                if(value != stages)
                {
                    stages = value;
                    NotifyPropertyChanged();

                }
            }
        }
        public void LoadAllPatientStages(PatientViewModel patientToSet)
        {
            patient = patientToSet;
            Stages = new ObservableCollection<StageViewModel>(HttpService.GetPatientStages(patient.PatientId).Select(s => new StageViewModel(s, this)));
        }
        public DateTime? MarkedDate { get; set; } = null;
        public void LoadAllPatientStagesWithRelatedMarked(DateTime markedDateToSet, int patientId)
        {
            MarkedDate= markedDateToSet;
            Patient loadedPatient = null;
            try
            {
                loadedPatient = HttpService.GetPatient(patientId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалось завантажити пацієнта: {ex.Message}");
                return;
            }
            Patient = new PatientViewModel(loadedPatient);
            try
            {
                Stages = new ObservableCollection<StageViewModel>(HttpService.GetPatientStages(patient.PatientId).Select(s => new StageViewModel(s, this)));
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {ex.Message}");
                return;
            }
        }
        public StagesViewModel()
        {
            PhotoClickedCommand = new RelayCommand(PhotoClicked);
            EditPatientCommand = new RelayCommand(EditPatient);
            CreateNewStageCommand = new RelayCommand(CreateNewStage);
        }
    }
}
