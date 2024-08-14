using ClinicDent2.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

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
