using ClinicDent2.ViewModel;
using System.Windows.Controls;

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
            stageAssetViewModel = new StageAssetViewModel(new ClinicDentClientCommon.Model.StageAsset());
            InitializeComponent();
            DataContext= stageAssetViewModel;
        }
    }
}
