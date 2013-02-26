using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AuthenticationPane : Page
    {
        private string _webViewUrl;

        public AuthenticationPane(string url)
        {
            this.InitializeComponent();
            _webViewUrl = url;

            AuthorizationWebView.Navigate(new Uri(url));
            AuthorizationWebView.LoadCompleted += AuthorizationWebView_LoadCompleted;
        }

        void AuthorizationWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Uri uri = e.Uri;

            System.Diagnostics.Debug.WriteLine(uri.AbsoluteUri);


            Dictionary<string, string> parameters = HashQueryParser.Parse(uri.AbsoluteUri);            
            if (parameters.Count > 0)
            {
                string token = parameters["access_token"];
                string tokenType = parameters["token_type"];
                string expiryTime = parameters["expires_in"];
                string refreshToken = parameters["refresh_token"];
                string accountUserName = parameters["account_username"];

                if (token != null)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Imgur has returned access_token '{0}' for user {1}", token, accountUserName));
                    ImgurHttpClient.Instance.LogIn(token, tokenType, DateTime.UtcNow.AddSeconds(Convert.ToDouble(expiryTime)), refreshToken, accountUserName);

                    ClosePopup();
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }

        private void ClosePopup()
        {
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }
    }
}
