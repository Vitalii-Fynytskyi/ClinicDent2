using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ClinicDent2.ViewModel
{
    public class TeethUnderObservationViewModel:BaseViewModel
    {
        private ObservableCollection<ToothUnderObservationViewModel> toothViewModels;
        public ObservableCollection<ToothUnderObservationViewModel> ToothViewModels
        {
            get { return toothViewModels; }
            set
            {
                if (toothViewModels != value)
                {
                    toothViewModels = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TeethUnderObservationViewModel()
        {
            ToothViewModels = new ObservableCollection<ToothUnderObservationViewModel>(HttpService.GetAllToothUnderObservation().Select(t => new ToothUnderObservationViewModel(t,null,this)));
        }
    }
}
