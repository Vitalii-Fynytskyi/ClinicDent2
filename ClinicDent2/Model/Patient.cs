using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ClinicDent2.Model
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Birthdate { get; set; }
        public string Illness { get; set; }
        public string Notes { get; set; }
        public string RegisterDate { get; set; }
        public string Statuses { get; set; }
        public string CurePlan { get; set; }
        public byte[] ImageBytes { get; set; }

    }
    public class PatientsToClient
    {
        public Patient[] Patients { get; set; }
        public int CountPages { get; set; }
    }
}
