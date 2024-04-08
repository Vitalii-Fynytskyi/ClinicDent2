﻿using ClinicDent2.Commands;
using ClinicDent2.Exceptions;
using ClinicDent2.Model;
using ClinicDent2.RequestAnswers;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace ClinicDent2.ViewModel
{
    public class OperationResult
    {
        public bool AllowDeactivateWindow { get; set; } = true;
        public List<Exception> Exceptions { get; set; } = new List<Exception>();
    }
    public class StagesViewModel : BaseViewModel
    {
        public RelayCommand PhotoClickedCommand { get; set; }
        public RelayCommand EditPatientCommand { get; set; }
        public RelayCommand UpdateCurePlanCommand { get; set; }

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
        private void UpdateCurePlan(object arg)
        {
            if (Mark_IsCurePlanUpdated == true)
            {
                try
                {
                    Patient.Patient.LastModifiedDateTime = HttpService.PutPatientCurePlan(patient.PatientId, patient.CurePlan, patient.Patient.LastModifiedDateTime);
                    Mark_IsCurePlanUpdated = false;
                }
                catch (ConflictException e)
                {
                    MessageBoxResult result = MessageBox.Show("Інший користувач редагував дані пацієнта. Натисніть 'Так' шоб завантажити нові дані.", "Конфлікт", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        Patient.Patient = HttpService.GetPatient(patient.PatientId);
                        NotifyPropertyChanged(nameof(CurePlan));
                        Mark_IsCurePlanUpdated = false;
                        
                    }
                    if (arg is OperationResult operationResult)
                    {
                        operationResult.AllowDeactivateWindow = false;
                    }
                }
                catch (NotFoundException)
                {
                    MessageBox.Show($"Пацієнт не існує або був видалений", "Помилка");
                }
                catch (Exception e)
                {
                    if (arg is OperationResult operationResult)
                    {
                        operationResult.Exceptions.Add(e);
                    }
                    else
                    {
                        MessageBox.Show($"Не вдалось оновити план лікування пацієнта: {e.Message}", "Помилка");
                    }
                }

            }
        }
        private bool CanUpdateCurePlan(object arg)
        {
            return Mark_IsCurePlanUpdated;
        }
        #region Added funcions
        internal OperationResult ServerUpdateStages()
        {
            OperationResult operationResult = new OperationResult();
            UpdateCurePlan(operationResult);
            if (stages == null)
            {
                return operationResult;
            }

            var selectedStageViewModels = stages
                .Where(vm => vm.IsOwner && vm.ViewModelStatus == ViewModelStatus.Updated && string.IsNullOrEmpty(vm.Error))
                .ToList();

            var stagesToUpdate = selectedStageViewModels
                .Select(vm => new StageDTO(vm.Stage))
                .ToList();

            if (stagesToUpdate.Count == 0)
            {
                return operationResult;
            }

            try
            {
                var putStagesRequestAnswer = HttpService.PutStages(stagesToUpdate);
                UpdateViewModels(selectedStageViewModels, putStagesRequestAnswer);
            }
            catch (ConflictException ex)
            {
                HandleConflictException(selectedStageViewModels, ex, operationResult);
            }
            catch(Exception ex)
            {
                operationResult.Exceptions.Add(ex);
            }
            return operationResult;
        }
        public void ReloadStages()
        {
            if (MarkedDate != null)
                LoadAllPatientStagesWithRelatedMarked(MarkedDate.Value, Patient.PatientId);
            else
                LoadAllPatientStages(Patient);
        }
        private void UpdateViewModels(List<StageViewModel> viewModels, PutStagesRequestAnswer response)
        {
            foreach (var vm in viewModels)
            {
                UpdateViewModel(vm, response.NewLastModifiedDateTime);
            }
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.PatientStagesUpdated, Patient.PatientId);

        }
        private void UpdateViewModel(StageViewModel viewModel, string lastModifiedDateTime)
        {
            viewModel.ViewModelStatus = ViewModelStatus.NotChanged;
            viewModel.Stage.OldPrice = viewModel.Stage.Price;
            viewModel.Stage.OldPayed = viewModel.Stage.Payed;
            viewModel.Stage.OldExpenses = viewModel.Stage.Expenses;
            viewModel.Stage.LastModifiedDateTime = lastModifiedDateTime;
        }
        private void HandleConflictException(List<StageViewModel> viewModels, ConflictException ex, OperationResult operationResult)
        {
            var conflictedIds = ex.Param as PutStagesRequestAnswer;
            var conflictedStageViewModels = viewModels
                .Where(s => conflictedIds.ConflictedStagesIds.Contains(s.Stage.Id))
                .ToList();

            viewModels.RemoveAll(s => conflictedStageViewModels.Contains(s));
            UpdateViewModels(viewModels, ex.Param as PutStagesRequestAnswer);

            MessageBoxResult result = MessageBox.Show(
                $"Інший користувач редагував етапи пацієнта ({String.Join("|", conflictedStageViewModels.Select(s => s.Stage.Title))}). Натисніть 'Так' шоб завантажити нові дані.",
                "Конфлікт", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Cancel);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ReloadStages();
                }
                catch(Exception e)
                {
                    operationResult.Exceptions.Add(e);
                    return;
                }
                MessageBox.Show("Оновлено етапи робіт.");
            }
            operationResult.AllowDeactivateWindow = false;

        }
        #endregion
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
        public string CurePlan
        {
            get
            {
                return Patient.CurePlan;
            }
            set
            {
                if(value != Patient.CurePlan)
                {
                    Patient.CurePlan = value;
                    Mark_IsCurePlanUpdated = true;
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
            Patient loadedPatient = HttpService.GetPatient(patientId);
            Patient = new PatientViewModel(loadedPatient);
            Stages = new ObservableCollection<StageViewModel>(HttpService.GetPatientStages(patient.PatientId).Select(s => new StageViewModel(s, this)));
        }
        public StagesViewModel()
        {
            PhotoClickedCommand = new RelayCommand(PhotoClicked);
            EditPatientCommand = new RelayCommand(EditPatient);
            CreateNewStageCommand = new RelayCommand(CreateNewStage);
            UpdateCurePlanCommand = new RelayCommand(UpdateCurePlan, CanUpdateCurePlan);
        }
        public bool Mark_IsCurePlanUpdated = false;
    }
}
