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
    public partial class StagesView : UserControl, IBrowserTabControl
    {
        public StagesViewModel stagesViewModel;
        public StagesView()
        {
            stagesViewModel = new StagesViewModel();
            InitializeComponent();
            DataContext = stagesViewModel;
        }
        public void LoadAllPatientStages(PatientViewModel patientToSet)
        {
            stagesViewModel.LoadAllPatientStages(patientToSet);
        }
        public void LoadAllPatientStagesWithScheduleMarked(DateTime date, int patientId)
        {
            stagesViewModel.LoadAllPatientStagesWithRelatedMarked(date, patientId);
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
        public void TabActivated()
        {
        }

        public void TabDeactivated()
        {
            stagesViewModel.ServerUpdateStages();
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.PatientStagesUpdated, stagesViewModel.Patient.PatientId);
        }

        public void TabClosed()
        {
        }

        public void Notify(int notificationCode, object param)
        {
            switch (notificationCode)
            {
                case NotificationCodes.PatientStagesUpdated:
                    int patientId = (int)param;
                    if (stagesViewModel.Patient.PatientId == patientId)
                    {
                        if (stagesViewModel.MarkedDate != null)
                            stagesViewModel.LoadAllPatientStagesWithRelatedMarked(stagesViewModel.MarkedDate.Value, stagesViewModel.Patient.PatientId);
                        else
                        {
                            try
                            {
                                stagesViewModel.LoadAllPatientStages(stagesViewModel.Patient);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show($"Не вдалось завантажити етапи робіт пацієнта: {e.Message}");
                                return;
                            }
                        }
                    }
                    break;
            }
        }
    }
}
