using ClinicDent2.Interfaces;
using ClinicDent2.Model;
using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для StagesView.xaml
    /// </summary>
    public partial class StagesView : UserControl, IBrowserTabControl
    {
        public StagesViewModel stagesViewModel;
        static List<StagesViewModel> openedStagesViewModel = new List<StagesViewModel>();
        public StagesView()
        {
            stagesViewModel = new StagesViewModel();
            InitializeComponent();
            DataContext = stagesViewModel;
            openedStagesViewModel.Add(stagesViewModel);
        }
        public void LoadAllPatientStages(PatientViewModel patientToSet)
        {
            stagesViewModel.LoadAllPatientStages(patientToSet);
        }
        public void LoadAllPatientStagesWithScheduleMarked(DateTime date, int patientId)
        {
            stagesViewModel.LoadAllPatientStagesWithRelatedMarked(date, patientId);
        }
        public void ServerUpdateStages()
        {
            stagesViewModel.ServerUpdateStages();
            foreach (StagesViewModel s in openedStagesViewModel)
            {
                //update stages on another window
                if (s.Patient.PatientId == stagesViewModel.Patient.PatientId)
                {
                    if (s.MarkedDate != null)
                        s.LoadAllPatientStagesWithRelatedMarked(s.MarkedDate.Value, s.Patient.PatientId);
                    else
                    {
                        try
                        {
                            s.LoadAllPatientStages(s.Patient);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {e.Message}");
                            return;
                        }
                    }
                }
            }
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CommitChanges();
        }
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = sender as System.Windows.Controls.Primitives.Thumb;
            var grid = thumb.Parent as Grid;
            var textBox = grid.Children.OfType<TextBox>().FirstOrDefault();

            if (textBox != null)
            {
                double newHeight = textBox.ActualHeight + e.VerticalChange;

                if (newHeight > textBox.MinHeight)
                {
                    textBox.Height = newHeight;
                }
            }
        }
        

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Check if the Enter key was pressed
                if (e.Key == Key.Enter)
                {
                    // Set the height back to auto
                    textBox.Height = double.NaN;
                }
            }
        }
        public void CommitChanges()
        {
            openedStagesViewModel.Remove(this.stagesViewModel);
            ServerUpdateStages();
        }
        public void TabActivated()
        {
        }

        public void TabDeactivated()
        {
        }

        public void TabClosed()
        {
        }
    }
}
