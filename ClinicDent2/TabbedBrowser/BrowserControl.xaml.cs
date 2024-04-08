using ClinicDent2.Interfaces;
using Newtonsoft.Json.Linq;
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
        private BrowserTabButton selectedTab;
        public BrowserTabButton GetSelectedTab()
        {
            return selectedTab;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="browserTab"></param>
        /// <returns>false if currently selected tab cannot be deactivated</returns>
        public bool SetSelectedTab(BrowserTabButton browserTab)
        {
            if (browserTab != selectedTab)
            {
                if (selectedTab != null) //try deactivate tab
                {
                    if ((selectedTab.Control as IBrowserTabControl)?.TabDeactivated() == false)
                    {
                        return false; //can't deactivate tab
                    }
                    selectedTab.BackgroundBorder.Background = Brushes.Transparent;
                    selectedTab.TabLabel.Foreground = Brushes.Black;
                }
                
                
                selectedTab = browserTab;

                if (selectedTab != null)
                {
                    selectedTab.BackgroundBorder.Background = Brushes.Black;
                    selectedTab.TabLabel.Foreground = Brushes.White;
                    currentTabOpened.Content = selectedTab.Control;
                    (selectedTab.Control as IBrowserTabControl)?.TabActivated();
                }
                return true;
            }
            return true;
        }
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
                        SetSelectedTab(tabButton);
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
                        SetSelectedTab(tabButton);
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
                        SetSelectedTab(tabButton);
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
                SetSelectedTab(newTab);
            }
        }
        public bool RemoveTabIfInBrowser(FrameworkElement element)
        {
            if (element.Parent is BrowserOpenedTab scrollViewer)
            {
                return RemoveTab(GetSelectedTab());
            }
            return true;
        }
        public bool RemoveTab(BrowserTabButton tab)
        {
            if(GetSelectedTab() == tab)
            {
                if (SetSelectedTab(null) == false)
                    return false;
                currentTabOpened.Content= null;

            }
            (tab.Control as IBrowserTabControl)?.TabClosed();
            if (tab.Parent == panelTabs)
            {
                panelTabs.Children.Remove(tab);
            }
            return true;
            
        }
        private void BrowserTabButton_CloseButtonClick(object sender, EventArgs e)
        {
            BrowserTabButton browserTabButton = sender as BrowserTabButton;
            if(RemoveTab(browserTabButton) == false)
            {
                return;
            }
            
        }

        private void BrowserTabButton_TabSelected(object sender, EventArgs e)
        {
            SetSelectedTab(sender as BrowserTabButton);
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
        public void NotifyOtherTabs(int notificationCode, object param)
        {
            foreach(UIElement element in panelTabs.Children)
            {
                if(element != GetSelectedTab() && element is BrowserTabButton tabButton && tabButton.Control is IBrowserTabControl browserTabButton)
                {
                    browserTabButton.Notify(notificationCode, param);
                }
            }
        }
        public void NotifyAllTabs(int notificationCode, object param)
        {
            foreach (UIElement element in panelTabs.Children)
            {
                if (element is BrowserTabButton tabButton && tabButton.Control is IBrowserTabControl browserTabButton)
                {
                    browserTabButton.Notify(notificationCode, param);
                }
            }
        }
    }
}
