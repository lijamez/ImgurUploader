using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImgurUploader
{

    class ImgurHttpClient
    {
        private static ImgurHttpClient _instance;
        public static ImgurHttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ImgurHttpClient();
                }

                return _instance;
            }
        }

        private ImgurHttpClient()
        {

        }

        private LogInState _logInState;
        public LogInState LogInState
        {
            get
            {
                if (_logInState == null)
                {
                    _logInState = new LogInState();
                }

                return _logInState;
            }
        }


        private bool _requiresNewAuthorization = true;

        private string _clientID;
        public string ClientID
        {
            get { return _clientID; }
        }

        private string _clientSecret;
        public string ClientSecret
        {
            get { return _clientSecret; }
        }

        private HttpClient _clientInstance;
        public HttpClient Client
        {
            get
            {
                if (_clientInstance == null)
                {
                    _clientInstance = new HttpClient();
                }

                if (_requiresNewAuthorization)
                {
                    if (LogInState.CredentialsDefined && TokensValid())
                    {
                        _clientInstance.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LogInState.AccessToken);
                        System.Diagnostics.Debug.WriteLine("Got an authorized client.");
                    }
                    else
                    {
                        _clientInstance.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", ClientID);
                        System.Diagnostics.Debug.WriteLine("Got an anonymous client.");
                    }
                }

                return _clientInstance;
            }
        }

        
        public async Task ReadAPIKeys()
        {
            try
            {
                StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"ApiKeys.txt");
                var stream = await file.OpenReadAsync();
                using (StreamReader rdr = new StreamReader(stream.AsStream()))
                {
                    await Task.Run(() =>
                    {
                        _clientID = rdr.ReadLine();
                        _clientSecret = rdr.ReadLine();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public delegate void LogInStatusChangedHandler(object sender, EventArgs e);
        public event LogInStatusChangedHandler LogInStatusChanged;
        protected void OnLogInStatusChanged(EventArgs e)
        {
            if (LogInStatusChanged != null)
            {
                LogInStatusChanged(this, e);
            }
        }


        public void LogIn(string accessToken, string tokenType, DateTime expireTime, string refreshToken, string accountUsername)
        {
            LogInState.AccessToken = accessToken;
            LogInState.TokenType = tokenType;
            LogInState.ExpireTime = expireTime;
            LogInState.RefreshToken = refreshToken;
            LogInState.AccountUsername = accountUsername;

            LogInState.CredentialsDefined = true;
            _requiresNewAuthorization = true;

            System.Diagnostics.Debug.WriteLine(String.Format("Now logged in as {0}", LogInState.AccountUsername));
            OnLogInStatusChanged(EventArgs.Empty);
        }

        public void LogOut()
        {
            LogInState.AccessToken = null;
            LogInState.TokenType = null;
            LogInState.RefreshToken = null;

            _requiresNewAuthorization = true;
            LogInState.CredentialsDefined = false;

            System.Diagnostics.Debug.WriteLine(String.Format("Logged out of {0}.", LogInState.AccountUsername));
            OnLogInStatusChanged(EventArgs.Empty);
        }

        public bool TokensValid()
        {
            if (String.IsNullOrEmpty(LogInState.AccessToken) || String.IsNullOrEmpty(LogInState.TokenType) || LogInState.TokenAcquireTime == null) return false;

            System.Diagnostics.Debug.WriteLine(String.Format("The expire time of this token is: {0}", LogInState.ExpireTime));
            if (DateTime.UtcNow.CompareTo(LogInState.ExpireTime) >= 0) //If the current time is after expiration time.
            {
                return false;
            }

            return true;
            
        }



    }
}
