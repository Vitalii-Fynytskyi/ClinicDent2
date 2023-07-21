using ClinicDent2.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicDent2.TabbedBrowser
{
    /// <summary>
    /// Interaction logic for BrowserTabButton.xaml
    /// </summary>
    public enum TabButtonType
    {
        Regular, PatientDataEdit, PatientStages
    }
    public partial class BrowserTabButton : UserControl
    {
        public UserControl Control
        {
            get
            {
                return control;
            }
            set
            {
                control = value;
            }
        }
        private UserControl control;
        public PatientViewModel PatientViewModel { get; set; } = null;
        public TabButtonType ButtonType { get; set; } = TabButtonType.Regular;
        public BrowserTabButton()
        {
            InitializeComponent();
        }
        public string TabText
        {
            get { return TabLabel.Text; }
            set { TabLabel.Text = value; }
        }
        public Brush SelectedBackground
        {
            get { return BackgroundBorder.Background; }
            set { BackgroundBorder.Background = value; }
        }
        public event EventHandler CloseButtonClick;
        public event EventHandler TabSelected;
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseButtonClick?.Invoke(this, EventArgs.Empty);
        }
        private void CustomTabControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabSelected?.Invoke(this, EventArgs.Empty);
        }
        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Background = Brushes.Gray;
        }
        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Background = Brushes.Transparent;
        }
    }
}