using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicDent2.Requests
{
    public class WeekMoneySummaryRequest
    {
        public int CabinetId { get; set; }
        public DateTime AnySunday { get; set; }
    }
}
