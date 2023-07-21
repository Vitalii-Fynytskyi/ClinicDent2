﻿using System;
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
        public Schedule() { }
        public Schedule(string id, string startDateTime, string endDateTime, string comment, string patientId, string doctorId, string patientName, string cabinetId, string cabinetName, string state)
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

    }
}
