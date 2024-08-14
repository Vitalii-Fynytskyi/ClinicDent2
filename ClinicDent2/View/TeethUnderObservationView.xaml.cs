using ClinicDentClientCommon.Model;
using ClinicDent2.ViewModel;
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
    /// Interaction logic for TeethUnderObservationView.xaml
    /// </summary>
    public partial class TeethUnderObservationView : UserControl
    {
        TeethUnderObservationViewModel viewModel;
        public TeethUnderObservationView()
        {
            viewModel = new TeethUnderObservationViewModel();
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ButtonToothObservation_Click(object sender, RoutedEventArgs e)
        {
            ToothUnderObservationViewModel toothUnderObservationViewModel = (sender as FrameworkElement).DataContext as ToothUnderObservationViewModel;
            ToothUnderObservationView toothUnderObservationView = new ToothUnderObservationView(toothUnderObservationViewModel);
            WindowContainer window = new WindowContainer();
            window.Content = toothUnderObservationView;
            window.Title = "Огляд спостереження";
            window.Show();
        }
    }
}
