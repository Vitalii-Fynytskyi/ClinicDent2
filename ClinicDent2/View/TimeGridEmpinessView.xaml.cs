using ClinicDentClientCommon.Model;
using System;
using System.Collections.Generic;
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

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для TimeGridEmpinessView.xaml
    /// </summary>
    public partial class TimeGridEmpinessView : UserControl
    {
        public event EventHandler<Patient> PatientSelected;
        public static readonly DependencyProperty TimeProperty;
        public TimeSpan Time
        {
            set
            {
                SetValue(TimeProperty, value);
            }
            get
            {
                return (TimeSpan)GetValue(TimeProperty);
            }
        }

        static TimeGridEmpinessView()
        {
            TimeProperty = DependencyProperty.Register("Time", typeof(TimeSpan), typeof(TimeGridEmpinessView));
        }
        public TimeGridEmpinessView()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowContainer windowSelectPatient = new WindowContainer();
            FormSelectPatient formSelectPatient = new FormSelectPatient();
            formSelectPatient.PatientSelected += FormSelectPatient_PatientSelected;
            windowSelectPatient.Content = formSelectPatient;
            windowSelectPatient.ShowDialog();
        }

        private void FormSelectPatient_PatientSelected(object sender, Patient e)
        {
            PatientSelected?.Invoke(this, e);
        }
    }
}
