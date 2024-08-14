using ClinicDentClientCommon.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
            Init();
        }
        private async Task Init()
        {
            ToothViewModels = new ObservableCollection<ToothUnderObservationViewModel>((await HttpService.GetAllToothUnderObservation()).Select(t => new ToothUnderObservationViewModel(t, null, this)));
        }
    }
}
