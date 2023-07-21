using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2
{
    public static class Options
    {
        public static DateTime? GetDateTimeFromString(string datetime)
        {
            DateTime dt;
            if (DateTime.TryParse(datetime, out dt) == false)
            {
                return null;
            }
            return dt;
        }
        public static MainWindow MainWindow { get; set; }
        public static Doctor CurrentDoctor { get; set; }
        public static Doctor[] AllDoctors { get; set; }

        public static string DateTimePattern { get; set; } = "yyyy-MM-dd HH:mm";
        public static string DatePattern { get; set; } = "dd.MM.yyyy";

        public static bool CanDeleteImage { get; set; }
        public static int PatientsPerPage { get; set; }
        public static int PhotosPerPage { get; set; }
        public static void WriteCookies(string path, LoginModel loginModel)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(loginModel.Email);
                writer.Write(loginModel.Password);
            }
        }
        public static LoginModel ReadCookies(string path)
        {
            if (!File.Exists(path)) { return null; }
            LoginModel loginModel = new LoginModel();
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                loginModel.Email = reader.ReadString();
                loginModel.Password = reader.ReadString();
            }
            return loginModel;
        }
    }
}
