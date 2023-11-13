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
            Options.DefaultSelectedTable = Convert.ToInt32(IniService.GetPrivateString("Settings", "DefaultSelectedTable"));

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
            if(mainMenu.browserControl.currentTabOpened.Content is IBrowserTabControl openedBrowserTabControl)
            {
                openedBrowserTabControl.TabDeactivated();
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
