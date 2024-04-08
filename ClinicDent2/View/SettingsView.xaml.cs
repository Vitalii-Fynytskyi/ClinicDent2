using ClinicDent2.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicDent2.View
{
    public partial class SettingsView : UserControl, IBrowserTabControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public void Notify(int notificationCode, object param)
        {
        }

        public void TabActivated()
        {
        }

        public void TabClosed()
        {
            
        }

        public bool TabDeactivated()
        {
            IniService.WritePrivateString("Settings", "CanDeleteImage", Options.CanDeleteImage.ToString());
            IniService.WritePrivateString("Settings", "PatientsPerPage", Options.PatientsPerPage.ToString());
            IniService.WritePrivateString("Settings", "PhotosPerPage", Options.PhotosPerPage.ToString());
            IniService.WritePrivateString("Settings", "DefaultSelectedTable", Options.DefaultSelectedCabinet.Id.ToString());
            return true;
        }

        private void ButtonEditClinicMaterials_Click(object sender, RoutedEventArgs e)
        {
            CreateNewStageAssetView createNewStageAssetView = new CreateNewStageAssetView();
            WindowContainer createNewStageAssetWindow = new WindowContainer();
            createNewStageAssetWindow.Title = "Створення параметру робіт";
            createNewStageAssetWindow.Content = createNewStageAssetView;
            createNewStageAssetWindow.ShowDialog();
        }
    }
}
