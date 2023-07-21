using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClinicDent2.Model
{
    public class Image :INotifyPropertyChanged
    {
        public int Id { get; set; }
        public byte[] OriginalBytes
        {
            get
            {
                return originalBytes;
            }
            set
            {
                originalBytes = value;
                if(originalBytes != null)
                    CompressedBytes = ImageCompressor.CompressImage(originalBytes, new System.Drawing.Size(200,150));
            }
        }

        private byte[] originalBytes;

        public int DoctorId { get; set; }

        public byte[] CompressedBytes { get; set; }
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }
        private string fileName;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class ImagesToClient
    {
        public Image[] Images;
        public int CountPages { get; set; }
    }
}
