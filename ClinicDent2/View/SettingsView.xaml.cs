using ClinicDent2.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicDent2.View
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl, ICommitChanges
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public void CommitChanges()
        {
            IniService.WritePrivateString("Settings", "CanDeleteImage", Options.CanDeleteImage.ToString());
            IniService.WritePrivateString("Settings", "PatientsPerPage", Options.PatientsPerPage.ToString());
            IniService.WritePrivateString("Settings", "PhotosPerPage", Options.PhotosPerPage.ToString());
        }

        private void ButtonEditClinicMaterials_Click(object sender, RoutedEventArgs e)
        {
            CreateNewStageAssetView createNewStageAssetView = new CreateNewStageAssetView();
            WindowContainer createNewStageAssetWindow = new WindowContainer();
            createNewStageAssetWindow.Title = "Створення параметру робіт";
            createNewStageAssetWindow.Content = createNewStageAssetView;
            createNewStageAssetWindow.ShowDialog();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CommitChanges();
        }
    }
}
