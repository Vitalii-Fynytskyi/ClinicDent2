using ClinicDent2.ViewModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ClinicDent2.Model
{
    public class Stage
    {
        public Stage() { }
        public Stage(StageDTO d)
        {
            Id = d.Id;
            PatientId = d.PatientId;
            DoctorId = d.DoctorId;
            Title = d.Title;
            StageDatetime = d.StageDatetime;
            IsSentViaViber = d.IsSentViaViber;
            if(d.Operation != null)
                Operation = StageViewModel.Operations.FirstOrDefault(a => a.Id == d.Operation);
            if(d.Bond !=null)
                Bond = StageViewModel.Bonds.FirstOrDefault(a => a.Id == d.Bond);
            if(d.Dentin!=null)
                Dentin = StageViewModel.Dentins.FirstOrDefault(a => a.Id == d.Dentin);
            if(d.Enamel!=null)
                Enamel = StageViewModel.Enamels.FirstOrDefault(a => a.Id == d.Enamel);

            if(d.CanalMethod!=null)
                CanalMethod = StageViewModel.CanalMethods.FirstOrDefault(a => a.Id == d.CanalMethod);
            if(d.Sealer !=null)
                Sealer = StageViewModel.Sealers.FirstOrDefault(a => a.Id == d.Sealer);
            if(d.Pin!=null)
                Pin = StageViewModel.Pins.FirstOrDefault(a => a.Id == d.Pin);
            if(d.Cement!=null)
                Cement = StageViewModel.Cements.FirstOrDefault(a => a.Id == d.Cement);
            if(d.Calcium!=null)
                Calcium = StageViewModel.Calciums.FirstOrDefault(a => a.Id == d.Calcium);
            if(d.Technician!=null)
                Technician = StageViewModel.Technicians.FirstOrDefault(a => a.Id == d.Technician);

            Payed = d.Payed;
            Price = d.Price;
            OldPayed = d.OldPayed;
            OldPrice = d.OldPrice;
            CommentText = d.CommentText;
            DoctorName = d.DoctorName;
        }
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        public bool IsSentViaViber { get; set; }
        public string Title { get; set; }
        public string StageDatetime { get; set; }
        public StageAsset Operation { get; set; } //'Реставрація' 'Плом. каналів' 'Цементування коронок'

        //***********************FOR RESTORATION********************
        public StageAsset Bond { get; set; }
        public StageAsset Dentin { get; set; }
        public StageAsset Enamel { get; set; }

        //**********************FOR CANALS**************************
        public StageAsset CanalMethod { get; set; }
        public StageAsset Sealer { get; set; }
        public StageAsset Pin { get; set; }


        //**********************ЦЕМЕНТУВАННЯ КОРОНОК****************
        public StageAsset Cement { get; set; }
        public StageAsset Calcium { get; set; }
        public StageAsset Technician { get; set; }

        public int Payed { get; set; }
        public int Price { get; set; }
        public int OldPayed { get; set; }
        public int OldPrice { get; set; }
        public string CommentText { get; set; }
        public string DoctorName { get; set; }
    }
    public class StageDTO
    {
        public StageDTO() { }
        public StageDTO(Stage d)
        {
            Id = d.Id;
            PatientId = d.PatientId;
            DoctorId = d.DoctorId;
            Title = d.Title;
            StageDatetime = d.StageDatetime;
            IsSentViaViber = d.IsSentViaViber;
            Operation = d.Operation?.Id;
            Bond = d.Bond?.Id;
            Dentin = d.Dentin?.Id;
            Enamel = d.Enamel?.Id;

            CanalMethod = d.CanalMethod?.Id;
            Sealer = d.Sealer?.Id;
            Pin = d.Pin?.Id;
            Cement = d.Cement?.Id;
            Calcium = d.Calcium?.Id;
            Technician = d.Technician?.Id;

            Payed = d.Payed;
            Price = d.Price;
            OldPayed= d.OldPayed;
            OldPrice= d.OldPrice;
            CommentText = d.CommentText;
            DoctorName = d.DoctorName;
        }
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public bool IsSentViaViber { get; set; }


        public string Title { get; set; }
        public string StageDatetime { get; set; }
        public int? Operation { get; set; } //'Реставрація' 'Плом. каналів' 'Цементування коронок'

        //***********************FOR RESTORATION********************
        public int? Bond { get; set; }
        public int? Dentin { get; set; }
        public int? Enamel { get; set; }

        //**********************FOR CANALS**************************
        public int? CanalMethod { get; set; }
        public int? Sealer { get; set; }
        public int? Pin { get; set; }


        //**********************ЦЕМЕНТУВАННЯ КОРОНОК****************
        public int? Cement { get; set; }
        public int? Calcium { get; set; }
        public int? Technician { get; set; }

        public int Payed { get; set; }
        public int Price { get; set; }
        public int OldPayed { get; set; }
        public int OldPrice { get; set; }

        public string CommentText { get; set; }
        public string DoctorName { get; set; }
    }
}
