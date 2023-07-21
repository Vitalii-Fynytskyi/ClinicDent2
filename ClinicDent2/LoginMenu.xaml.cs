using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
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
    public partial class LoginMenu : UserControl
    {
        public LoginMenu()
        {
            InitializeComponent();
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginModel loginModel = new LoginModel();
            loginModel.Email = TextBoxEmail.Text;
            loginModel.Password = TextBoxPassword.Password;
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
    }
}
