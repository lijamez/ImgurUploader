using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountFlyout : Page
    {
        //private double _authPopupHeight = 710;
        ImgurHttpClient _imgurHttpClient = ImgurHttpClient.Instance;
        Frame _parentFrame;

        public AccountFlyout(Frame parentFrame)
        {
            this.InitializeComponent();

            _parentFrame = parentFrame;
            LoggedIn.DataContext = _imgurHttpClient.LogInState;
            _imgurHttpClient.LogInStatusChanged += new ImgurHttpClient.LogInStatusChangedHandler(LogInStatusChanged);
            RefreshUI();


            InputPane input = InputPane.GetForCurrentView();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void LogInStatusChanged(object sender, EventArgs e)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_imgurHttpClient.LogInState.LoggedIn)
            {
                LoggedIn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                LoggedOut.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                LoggedIn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                LoggedOut.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

        }

        /// <summary>
        /// This is the click handler for the back button on the Flyout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackClicked(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

            // If the app is not snapped, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                SettingsPane.Show();
            }
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(AuthenticationPage));
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
            
        }



        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            ImgurHttpClient.Instance.LogOut();
        }

    }
}
