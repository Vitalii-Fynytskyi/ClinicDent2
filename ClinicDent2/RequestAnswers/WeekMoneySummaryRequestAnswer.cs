using System.Collections.Generic;

namespace ClinicDent2.RequestAnswers
{
    public class WeekMoneySummaryRequestAnswer
    {
        public List<int> StagesPaidSum { get; set; }
        public List<int> StagesPriceSum { get; set; }
        public List<int> StagesExpensesSum { get; set; }
        public List<int> DoctorIds { get; set; }

    }
}
