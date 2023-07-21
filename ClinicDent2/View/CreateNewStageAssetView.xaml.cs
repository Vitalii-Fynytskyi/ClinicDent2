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
    /// Interaction logic for CreateNewStageAssetView.xaml
    /// </summary>
    public partial class CreateNewStageAssetView : UserControl
    {
        public StageAssetViewModel stageAssetViewModel;
        public CreateNewStageAssetView()
        {
            stageAssetViewModel = new StageAssetViewModel(new Model.StageAsset());
            InitializeComponent();
            DataContext= stageAssetViewModel;
        }
    }
}
