using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicDent2.Model
{
    public enum SchedulePatientState
    {
        Unknown = 0, WillAppear = 1, Refused = 2
    }
    public class Schedule
    {
        public Schedule()
        {
            StagesPriceSum = new List<int>();
            StagesPaidSum = new List<int>();
            StagesExpensesSum = new List<int>();
            DoctorIds = new List<int>();
        }
        public Schedule(string id, string startDateTime, string endDateTime, string comment, string patientId, string doctorId, string patientName, string cabinetId, string cabinetName, string state,string priceSum, string payedSum,string messagerStateNumber, string doctorsId, string expensesSum)
        {
            Id = Int32.Parse(id);
            StartDatetime = startDateTime;
            EndDatetime = endDateTime;
            Comment = comment;
            if (patientId == "<null>")
            {
                PatientId = null;
            }
            else
            {
                PatientId = Int32.Parse(patientId);
            }
            DoctorId = Int32.Parse(doctorId);
            PatientName = patientName;
            CabinetId = Int32.Parse(cabinetId);
            CabinetName = cabinetName;
            State = (SchedulePatientState)Int32.Parse(state);
            string[] priceSumSplit = priceSum.Split('|', StringSplitOptions.RemoveEmptyEntries);
            string[] paidSumSplit = payedSum.Split('|', StringSplitOptions.RemoveEmptyEntries);
            string[] doctorIdsSplit = doctorsId.Split('|', StringSplitOptions.RemoveEmptyEntries);
            string[] expensesSumSplit = expensesSum.Split('|', StringSplitOptions.RemoveEmptyEntries);

            StagesPriceSum = new List<int>(priceSumSplit.Length);
            StagesPaidSum = new List<int>(priceSumSplit.Length);
            DoctorIds = new List<int>(priceSumSplit.Length);
            StagesExpensesSum = new List<int>(priceSumSplit.Length);

            for (int i = 0; i < doctorIdsSplit.Length; i++)
            {
                StagesPriceSum.Add(Int32.Parse(priceSumSplit[i]));
                StagesPaidSum.Add(Int32.Parse(paidSumSplit[i]));
                DoctorIds.Add(Int32.Parse(doctorIdsSplit[i]));
                StagesExpensesSum.Add(Int32.Parse(expensesSumSplit[i]));
            }
            StagesSentViaMessagerState = (ScheduleIsSentViaMessagetState)Convert.ToInt32(messagerStateNumber);
        }
        public Schedule(string id, string startDateTime, string endDateTime, string comment, string patientId, string doctorId, string patientName, string cabinetId, string cabinetName, string state):this()
        {
            Id = Int32.Parse(id);
            StartDatetime = startDateTime;
            EndDatetime = endDateTime;
            Comment = comment;
            if (patientId == "<null>")
            {
                PatientId = null;
            }
            else
            {
                PatientId = Int32.Parse(patientId);
            }
            DoctorId = Int32.Parse(doctorId);
            PatientName = patientName;
            CabinetId = Int32.Parse(cabinetId);
            CabinetName = cabinetName;
            State = (SchedulePatientState)Int32.Parse(state);
        }
        public Schedule(Schedule s)
        {
            Id = s.Id;
            StartDatetime = s.StartDatetime;
            EndDatetime = s.EndDatetime;
            Comment = s.Comment;
            PatientId= s.PatientId;
            DoctorId= s.DoctorId;
            CabinetId= s.CabinetId;
            CabinetName= s.CabinetName;
            State= s.State;
            StagesSentViaMessagerState=s.StagesSentViaMessagerState;
            StagesPaidSum= s.StagesPaidSum;
            StagesPriceSum= s.StagesPriceSum;
            StagesExpensesSum = s.StagesExpensesSum;
            DoctorIds = s.DoctorIds;
        }
        public int Id { get; set; }
        public string StartDatetime { get; set; }
        public string EndDatetime { get; set; }

        public string Comment { get; set; }

        public int? PatientId { get; set; }
        public string PatientName { get; set; }

        public int DoctorId { get; set; }
        public int CabinetId { get; set; }

        public string CabinetName { get; set; }
        public SchedulePatientState State { get; set; } //0 - unknown, 1 - will appear, 2 - refused
        public ScheduleIsSentViaMessagetState StagesSentViaMessagerState { get; set; } = ScheduleIsSentViaMessagetState.NoStages;
        public List<int> StagesPaidSum { get; set; }
        public List<int> StagesPriceSum { get; set; }
        public List<int> DoctorIds { get; set; }
        public List<int> StagesExpensesSum { get; set; }
    }
    public enum ScheduleIsSentViaMessagetState
    {
        NoStages = 0,
        CanSend = 1,
        AllSent = 2
    }
}
