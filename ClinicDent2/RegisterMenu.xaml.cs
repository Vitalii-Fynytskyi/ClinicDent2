using ClinicDent2.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicDent2
{
    /// <summary>
    /// Логика взаимодействия для RegisterMenu.xaml
    /// </summary>
    public partial class RegisterMenu : UserControl
    {
        public RegisterMenu()
        {
            InitializeComponent();
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Options.MainWindow.goToLoginMenu();
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterModel registerModel = new RegisterModel();
            registerModel.Email = TextBoxEmail.Text;
            registerModel.Name = TextBoxName.Text;
            registerModel.Password = TextBoxPassword.Password;
            registerModel.ConfirmPassword = TextBoxConfirmPassword.Password;
            Doctor doctor = null;
            try
            {
                doctor = HttpService.Register(registerModel);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Не вдалось зареєструватись: {ex.Message}","Помилка");
                return;
            }
            LoginModel loginModel = new LoginModel()
            {
                Email = registerModel.Email,
                Password = registerModel.Password
            };
            Options.WriteCookies(Environment.CurrentDirectory + "\\data\\cookies.dat", loginModel);
            Options.MainWindow.goToMainMenu(doctor);
        }
    }
}
