using System;
using System.Windows;

namespace ClinicDent2
{
    /// <summary>
    /// Interaction logic for WindowEnterMessage.xaml
    /// </summary>
    public partial class WindowEnterMessage : Window
    {
        public event Action<WindowEnterMessage,string> ConfirmPressed;
        public event Action<WindowEnterMessage> CancelPressed;

        public WindowEnterMessage()
        {
            InitializeComponent();
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmPressed?.Invoke(this, textBoxMessage.Text);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelPressed?.Invoke(this);
        }
    }
}
