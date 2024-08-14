using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;

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
            GetTenantList();
            DataContext = this;
        }
        public async Task GetTenantList()
        {
            Tenants = await HttpService.GetTenantList();

        }
        private async void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginModel loginModel = new LoginModel();
            loginModel.Email = TextBoxEmail.Text;
            loginModel.Password = TextBoxPassword.Password;
            loginModel.Tenant = SelectedTenant;
            Doctor doctor = await HttpService.Authenticate(loginModel);
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
