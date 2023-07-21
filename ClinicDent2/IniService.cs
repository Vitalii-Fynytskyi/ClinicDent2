using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ClinicDent2
{
    /// <summary>
    /// Class for working witn .ini files
    /// </summary>
    public static class IniService
    {
        public static string GetPrivateString(string aSection, string aKey)
        {
            StringBuilder buffer = new StringBuilder(SIZE);

            GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

            return buffer.ToString();
        }

        public static void WritePrivateString(string aSection, string aKey, string aValue)
        {
            WritePrivateString(aSection, aKey, aValue, path);
        }

        public static string Path { get { return path; } set { path = value; } }

        private const int SIZE = 1024;
        private static string path = Environment.CurrentDirectory + "\\data\\ClinicDent.ini";

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateString(string section, string key, string str, string path);
    }
}
