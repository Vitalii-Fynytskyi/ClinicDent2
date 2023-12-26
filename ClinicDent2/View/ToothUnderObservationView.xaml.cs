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
    /// Interaction logic for ToothUnderObservationView.xaml
    /// </summary>
    public partial class ToothUnderObservationView : UserControl
    {
        public ToothUnderObservationViewModel toothUnderObservationViewModel;
        public ToothUnderObservationView()
        {
            InitializeComponent();
        }
        public ToothUnderObservationView(ToothUnderObservationViewModel toothUnderObservationViewModelToSet)
        {
            toothUnderObservationViewModel = toothUnderObservationViewModelToSet;
            toothUnderObservationViewModel.SessionCompleted += ToothUnderObservationViewModel_SessionCompleted;
            InitializeComponent();
            DataContext = toothUnderObservationViewModelToSet;
        }

        private void ToothUnderObservationViewModel_SessionCompleted(object sender, EventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
