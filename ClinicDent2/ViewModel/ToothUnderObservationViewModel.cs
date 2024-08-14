using ClinicDent2.Commands;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using System;

namespace ClinicDent2.ViewModel
{
    public class ToothUnderObservationViewModel:BaseViewModel
    {
        public event EventHandler SessionCompleted;
        public ToothUnderObservation toothUnderObservation;
        public StageViewModel stageViewModel;
        public TeethUnderObservationViewModel teethUnderObservationViewModel;

        public ToothUnderObservationViewModel(ToothUnderObservation toothToSet, StageViewModel stageViewModelToSet = null,TeethUnderObservationViewModel teethUnderObservationViewModelToSet=null)
        {
            toothUnderObservation = toothToSet;
            stageViewModel = stageViewModelToSet;
            teethUnderObservationViewModel = teethUnderObservationViewModelToSet;
            DeleteObservationCommand = new RelayCommand(DeleteObservation, CanDeleteObservation);
            SaveObservationCommand=new RelayCommand(SaveObservation, CanSaveObservation);
        }
        public RelayCommand DeleteObservationCommand { get; set; }
        public RelayCommand SaveObservationCommand { get; set; }
        public void DeleteObservation(object param)
        {
            HttpService.DeleteToothUnderObservation(toothUnderObservation.Id);
            if(stageViewModel != null)
            {
                stageViewModel.ToothUnderObservationId = null;
            }
            if(teethUnderObservationViewModel != null)
            {
                teethUnderObservationViewModel.ToothViewModels.Remove(this);
            }
            SessionCompleted?.Invoke(this, EventArgs.Empty);
        }
        public bool CanDeleteObservation(object param)
        {
            return !(ViewModelStatus.HasFlag(ViewModelStatus.Inserted) == true || ViewModelStatus.HasFlag(ViewModelStatus.Deleted) == true);
        }
        public async void SaveObservation(object param)
        {
            if(toothUnderObservation.Id == 0) //decide to create or update record on server
            {
                int toothObservationId = await HttpService.PostToothUnderObservation(toothUnderObservation);
                toothUnderObservation.Id = toothObservationId;
                if (stageViewModel != null)
                {
                    stageViewModel.ToothUnderObservationId = toothObservationId;
                }
            }
            else
            {
                await HttpService.PutToothUnderObservation(toothUnderObservation);
            }
            SessionCompleted?.Invoke(this, EventArgs.Empty);

        }
        public bool CanSaveObservation(object param)
        {
            return ViewModelStatus.HasFlag(ViewModelStatus.Inserted) || ViewModelStatus.HasFlag(ViewModelStatus.Updated);
        }
        public string ToothName
        {
            get
            {
                return toothUnderObservation.ToothName;
            }
            set
            {
                if(value != toothUnderObservation.ToothName)
                {
                    toothUnderObservation.ToothName = value;
                    ViewModelStatus |= ViewModelStatus.Updated;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ObservationDescription
        {
            get
            {
                return toothUnderObservation.ObservationDescription;
            }
            set
            {
                if (value != toothUnderObservation.ObservationDescription)
                {
                    toothUnderObservation.ObservationDescription = value;
                    ViewModelStatus |= ViewModelStatus.Updated;
                    NotifyPropertyChanged();
                }
            }
        }
        public string PatientName
        {
            get
            {
                return toothUnderObservation.PatientName;
            }
        }
    }
}
