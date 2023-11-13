using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicDent2.Model
{
    public class PatientWithDebt:Patient
    {
        public int DebtSum { get; set; }
    }
    public class DebtPatientsToClient
    {
        public PatientWithDebt[] Patients { get; set; }
        public int CountPages { get; set; }
    }
}
