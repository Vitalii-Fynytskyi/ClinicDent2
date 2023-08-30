using ClinicDent2.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicDent2.TabbedBrowser
{
    /// <summary>
    /// Interaction logic for BrowserControl.xaml
    /// </summary>
    public partial class BrowserControl : UserControl
    {
        public BrowserTabButton SelectedTab
        {
            get
            {
                return selectedTab;
            }
            set
            {
                if (value != selectedTab)
                {
                    if (selectedTab != null)
                    {
                        selectedTab.BackgroundBorder.Background = Brushes.Transparent;
                        selectedTab.TabLabel.Foreground = Brushes.Black;
                        (selectedTab.Control as IBrowserTabControl)?.TabDeactivated();
                    }
                    selectedTab = value;

                    if(selectedTab != null)
                    {
                        selectedTab.BackgroundBorder.Background = Brushes.Black;
                        selectedTab.TabLabel.Foreground = Brushes.White;
                        currentTabOpened.Content = selectedTab.Control;
                        (selectedTab.Control as IBrowserTabControl)?.TabActivated();
                    }
                    
                }
            }
        }
        private BrowserTabButton selectedTab;
        public BrowserControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Navigate to screen if exists and returns true, otherwise returns false and do nothing
        /// </summary>
        /// <param name="screenName">Screen name from ScreenNames.cs</param>
        /// <returns></returns>
        public bool ScreenRequested(string screenName)
        {
            foreach (object i in panelTabs.Children)
            {
                if(i is BrowserTabButton tabButton)
                {
                    if(tabButton.TabText == screenName)
                    {
                        SelectedTab= tabButton;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ScreenRequested(int patientId, TabButtonType tabButtonType)
        {
            foreach (object i in panelTabs.Children)
            {
                if (i is BrowserTabButton tabButton)
                {
                    if (tabButton.PatientViewModel?.PatientId == patientId && tabButton.ButtonType == tabButtonType)
                    {
                        SelectedTab = tabButton;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ScreenRequested(int patientId, TabButtonType tabButtonType, string dayOfMonth)
        {
            foreach (object i in panelTabs.Children)
            {
                if (i is BrowserTabButton tabButton)
                {
                    if (tabButton.PatientViewModel?.PatientId == patientId && tabButton.ButtonType == tabButtonType && dayOfMonth==ExtractDate(tabButton.TabText))
                    {
                        SelectedTab = tabButton;
                        return true;
                    }
                }
            }
            return false;
        }
        public BrowserTabButton GetTabButton(int patientId, TabButtonType tabButtonType)
        {
            foreach (object i in panelTabs.Children)
            {
                if (i is BrowserTabButton tabButton)
                {
                    if (tabButton.PatientViewModel?.PatientId == patientId && tabButton.ButtonType == tabButtonType)
                    {
                        return tabButton;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Add new button to tab panel and redirects to new added screen if requested
        /// </summary>
        /// <param name="newTab">Fully completed BrowserTabButton with TabText and Control set</param>
        /// <param name="autoRedirect">If true user will be automatically redirected to this screen</param>
        public void AddNewTab(BrowserTabButton newTab, bool autoRedirect = true)
        {
            newTab.TabSelected += BrowserTabButton_TabSelected;
            newTab.CloseButtonClick += BrowserTabButton_CloseButtonClick;
            panelTabs.Children.Add(newTab);
            if(autoRedirect)
            {
                SelectedTab = newTab;
            }
        }
        public void RemoveTabIfInBrowser(FrameworkElement element)
        {
            if (element.Parent is BrowserOpenedTab scrollViewer)
            {
                RemoveTab(SelectedTab);
            }
        }
        public void RemoveTab(BrowserTabButton tab)
        {
            if(SelectedTab == tab)
            {
                currentTabOpened.Content= null;
                SelectedTab = null;
            }
            if(tab.Parent == panelTabs)
            {
                panelTabs.Children.Remove(tab);
            }
            
        }
        private void BrowserTabButton_CloseButtonClick(object sender, EventArgs e)
        {
            BrowserTabButton browserTabButton = sender as BrowserTabButton;
            RemoveTab(browserTabButton);
            (browserTabButton.Control as IBrowserTabControl)?.TabClosed();
        }

        private void BrowserTabButton_TabSelected(object sender, EventArgs e)
        {
            SelectedTab = sender as BrowserTabButton;
        }
        public string ExtractDate(string input)
        {
            var words = input.Split(' ');
            if (words.Length >= 2)
            {
                return words[words.Length - 2] + " " + words[words.Length - 1];
            }
            return null;
        }
    }
}
