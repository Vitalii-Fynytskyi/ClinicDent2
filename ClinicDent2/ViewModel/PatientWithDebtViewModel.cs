using ClinicDent2.Model;

namespace ClinicDent2.ViewModel
{
    public class PatientWithDebtViewModel:PatientViewModel
    {
        private int debtSum;
        public int DebtSum
        {
            get { return debtSum; }
            set
            {
                if(value != debtSum)
                {
                    debtSum = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public PatientWithDebtViewModel(PatientWithDebt p):base(p)
        {
            debtSum=p.DebtSum;
        }
    }
}
