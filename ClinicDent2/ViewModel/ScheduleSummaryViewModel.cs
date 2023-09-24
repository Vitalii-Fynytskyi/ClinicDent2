using ClinicDent2.Model;

namespace ClinicDent2.ViewModel
{
    public class ScheduleSummaryViewModel:BaseViewModel
    {
        public Doctor Doctor { get; set; }
        public int Price { get; set; }
        public int Payed { get; set; }
        public int Expenses { get; set; }
        public string DisplayText
        {
            get
            {
                return $"{Doctor.Name}: {Payed}/{Price} грн. Прибуток: {Payed - Expenses} грн.";
            }
        }

        public ScheduleSummaryViewModel(Doctor doctor, int price, int payed, int expenses)
        {
            Doctor = doctor;
            Price = price;
            Payed = payed;
            Expenses = expenses;
        }
    }
}
