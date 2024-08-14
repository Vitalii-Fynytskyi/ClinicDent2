using ClinicDentClientCommon.Model;
using System;
using System.IO;
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

        #region IniFile data
        public static bool CanDeleteImage { get; set; }
        public static int PatientsPerPage { get; set; }
        public static int PhotosPerPage { get; set; }
        public static int DefaultSelectedTable { get; set; }
        public static Cabinet DefaultSelectedCabinet { get; set; }
        #endregion


        public static void WriteCookies(string path, LoginModel loginModel)
        {
            IniService.WritePrivateString("Settings", "DefaultTenant", loginModel.Tenant);
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(loginModel.Email);
                writer.Write(loginModel.Password);
                writer.Write(loginModel.Tenant);

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
                loginModel.Tenant = reader.ReadString();
            }
            return loginModel;
        }
    }
}
