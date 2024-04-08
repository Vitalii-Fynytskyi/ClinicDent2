using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClinicDent2.Model;

namespace ClinicDent2
{
    /// <summary>
    /// Логика взаимодействия для LoginMenu.xaml
    /// </summary>
    public partial class LoginMenu : UserControl, INotifyPropertyChanged
    {
        public string[] Tenants
        {
            get
            {
                return tenants;
            }
            set //updates tenant list in UI, try to select default tenant from settings
            {
                if( tenants != value )
                {
                    tenants = value;
                    OnPropertyChanged();
                    selectedTenant = null;
                    if(tenants != null && tenants.Length > 0)
                    {
                        string defaultTenant = IniService.GetPrivateString("Settings", "DefaultTenant");
                        if(tenants.Contains(defaultTenant))
                        {
                            selectedTenant = defaultTenant;
                        }
                        else
                        {
                            selectedTenant = tenants[0];
                        }
                    }
                    OnPropertyChanged("SelectedTenant");
                }
            }
        }
        private string[] tenants;
        public string SelectedTenant
        {
            get
            {
                return selectedTenant;
            }
            set
            {
                if( selectedTenant != value )
                {
                    selectedTenant = value;
                    OnPropertyChanged();
                }
            }
        }
        private string selectedTenant;
        
        public LoginMenu()
        {
            InitializeComponent();
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            Tenants = HttpService.GetTenantList();
            DataContext = this;
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginModel loginModel = new LoginModel();
            loginModel.Email = TextBoxEmail.Text;
            loginModel.Password = TextBoxPassword.Password;
            loginModel.Tenant = SelectedTenant;
            Doctor doctor = HttpService.Authenticate(loginModel);
            if(doctor == null)
            {
                labelInfo.Content = "Неправильно введено пошту або пароль";
                return;
            }
            Options.WriteCookies(Environment.CurrentDirectory + "\\data\\cookies.dat", loginModel);
            Options.MainWindow.goToMainMenu(doctor);

        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            Options.MainWindow.goToRegisterMenu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
