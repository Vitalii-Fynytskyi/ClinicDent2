using ClinicDent2.Interfaces;
using ClinicDent2.TabbedBrowser;
using ClinicDentClientCommon;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ClinicDent2
{
    public partial class MainWindow : Window
    {
        public MainMenu mainMenu;
        private void CheckForUpdates()
        {
            string clientVersion = "3";
            string apiVersion = HttpService.GetApiVersion().Result;
            if(apiVersion != clientVersion)
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string executablePath = Path.Combine(directory, "ClientUpdater.exe");
                Process.Start(executablePath);
                Environment.Exit(0);
            }
        }

        public MainWindow()
        {
            ConfigData.ServerAddress = IniService.GetPrivateString("Settings", "ServerAddress");
            ConfigData.LanServerAddress = IniService.GetPrivateString("Settings", "LanServerAddress");
            CheckForUpdates();
            InitializeComponent();

            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = 0;
            Options.CanDeleteImage = Convert.ToBoolean(IniService.GetPrivateString("Settings", "CanDeleteImage"));
            Options.PatientsPerPage = Convert.ToInt32(IniService.GetPrivateString("Settings", "PatientsPerPage"));
            Options.PhotosPerPage = Convert.ToInt32(IniService.GetPrivateString("Settings", "PhotosPerPage"));
            Options.DefaultSelectedTable = Convert.ToInt32(IniService.GetPrivateString("Settings", "DefaultSelectedTable"));
            Options.MainWindow = this;

            Authenticate();
        }

        public void Authenticate()
        {
            Doctor doctor = TryAuthenticateWithCookies().Result;
            if (doctor != null)
            {

                goToMainMenu(doctor);
                return;
            }
            goToLoginMenu();
        }
        private async Task<Doctor> TryAuthenticateWithCookies()
        {
            LoginModel loginModel = Options.ReadCookies(Environment.CurrentDirectory + "\\data\\cookies.dat");
            if(loginModel == null) { return null; }
            Doctor doctor = await HttpService.Authenticate(loginModel).ConfigureAwait(false);
            return doctor;
        }

        public async Task CloseApp()
        {
            if(mainMenu.browserControl.currentTabOpened.Content is IBrowserTabControl openedBrowserTabControl)
            {
                if(await openedBrowserTabControl.TabDeactivated() == false)
                {
                    return;
                }
            }
            foreach (UIElement element in mainMenu.browserControl.panelTabs.Children)
            {
                if(element is BrowserTabButton tabButton) //ensure element is tab button
                {
                    if (tabButton.Control is IBrowserTabControl browserTabControl) //ensure interface is supported
                    {
                        browserTabControl.TabClosed();
                    }
                }
            }
            mainMenu.TcpClient.DisconnectFromServer();
            Environment.Exit(0);
        }
        public void goToMainMenu(Doctor doctor)
        {

            mainMenu = new MainMenu(doctor);
            ViewBox.Child = mainMenu;
        }
        public void goToLoginMenu()
        {
            LoginMenu loginMenu = new LoginMenu();
            ViewBox.Child = loginMenu;

        }
        public void goToRegisterMenu()
        {
            RegisterMenu registerMenu = new RegisterMenu();
            ViewBox.Child = registerMenu;
        }
    }
}
