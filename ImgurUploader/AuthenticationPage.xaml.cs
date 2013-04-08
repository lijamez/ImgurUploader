using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
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
    public sealed partial class AuthenticationPage : Page
    {
        public AuthenticationPage()
        {
            this.InitializeComponent();
        }

        async void AuthorizationWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Uri uri = e.Uri;

            System.Diagnostics.Debug.WriteLine(uri.AbsoluteUri);
            System.Diagnostics.Debug.WriteLine(uri.AbsolutePath);
            System.Diagnostics.Debug.WriteLine(uri.LocalPath);

            Dictionary<string, string> parameters = HashQueryParser.Parse(uri.AbsoluteUri);
            if (String.Equals(uri.Host, "imgur.com"))
            {
                string logInResultsMessage;

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

                        logInResultsMessage = String.Format("Successfully logged in as {0}", accountUserName);
                    }
                    else
                    {
                        logInResultsMessage = "Unable to get your token. And that's terrible.";
                    }
                }
                else
                {
                    logInResultsMessage = "There appears to be a problem with Imgur's authentication system. Please try again later.";
                }

                this.Frame.GoBack();

                MessageDialog msg = new MessageDialog(logInResultsMessage);
                await msg.ShowAsync();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string authUrl = CreateAuthenticationUrl(ImgurHttpClient.Instance.ClientID);

            AuthorizationWebView.Navigate(new Uri(authUrl));
            AuthorizationWebView.LoadCompleted += AuthorizationWebView_LoadCompleted;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private string CreateAuthenticationUrl(string clientId)
        {
            return String.Format("https://api.imgur.com/oauth2/authorize?client_id={0}&response_type=token", clientId);
        }

    }
}
