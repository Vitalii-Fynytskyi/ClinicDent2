using ClinicDent2.Interfaces;
using ClinicDent2.Model;
using ClinicDent2.TabbedBrowser;
using ClinicDent2.View;
using System;
using System.Net.Http;
using System.Windows;

namespace ClinicDent2
{
    public partial class MainWindow : Window
    {
        public MainMenu mainMenu;
        public void Init()
        {
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = 0;
            Options.CanDeleteImage = Convert.ToBoolean(IniService.GetPrivateString("Settings","CanDeleteImage"));
            Options.PatientsPerPage = Convert.ToInt32(IniService.GetPrivateString("Settings","PatientsPerPage"));
            Options.PhotosPerPage = Convert.ToInt32(IniService.GetPrivateString("Settings","PhotosPerPage"));
            Options.MainWindow = this;
        }

        public MainWindow()
        {
            InitializeComponent();
            Init();
            Doctor doctor = TryAuthenticateWithCookies();
            if(doctor != null)
            {

                goToMainMenu(doctor);
                return;
            }
            goToLoginMenu();
            
            
        }
        private Doctor TryAuthenticateWithCookies()
        {
            LoginModel loginModel = Options.ReadCookies(Environment.CurrentDirectory + "\\data\\cookies.dat");
            if(loginModel == null) { return null; }
            Doctor doctor = HttpService.Authenticate(loginModel);
            return doctor;
        }

        public void CloseApp()
        {
            foreach(Window w in App.Current.Windows) //iterate all windows in order to find StagesView, stages need to be updated before quiting application
            {
                if(w.Content is IBrowserTabControl i)
                {
                    i.CommitChanges();
                    break;
                }
            }
            if(mainMenu.browserControl.currentTabOpened.Content is IBrowserTabControl commitChanges)
            {
                commitChanges.CommitChanges();
            }
            App.Current.Shutdown();
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
