using ClinicDent2.ViewModel;
using System.Windows.Controls;

namespace ClinicDent2.View
{
    public partial class StatisticsView : UserControl
    {
        StatisticsViewModel statisticsViewModel;
        public StatisticsView()
        {
            statisticsViewModel = new StatisticsViewModel();
            InitializeComponent();
            DataContext = statisticsViewModel;
        }
    }
}
