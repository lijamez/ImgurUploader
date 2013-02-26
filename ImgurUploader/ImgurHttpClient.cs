using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public bool LoggedIn
        {
            get;
            private set;
        }

        public string AccessToken
        {
            get;
            private set;
        }
        public string TokenType
        {
            get;
            private set;
        }
        public DateTime TokenAcquireTime
        {
            get;
            private set;
        }
        public DateTime ExpireTime
        {
            get;
            private set;
        }
        public string RefreshToken
        {
            get;
            private set;
        }
        public string AccountUsername
        {
            get;
            set;
        }


        private bool _requiresNewAuthorization = true;

        public string ClientID = "Client ID Goes HERE!";
        public string ClientSecret = "Secret goes THERE!";

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
                    if (LoggedIn && TokensValid())
                    {
                        _clientInstance.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
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
            AccessToken = accessToken;
            TokenType = tokenType; 
            ExpireTime = expireTime;
            RefreshToken = refreshToken;
            AccountUsername = accountUsername;

            LoggedIn = true;
            _requiresNewAuthorization = true;
            System.Diagnostics.Debug.WriteLine(String.Format("Now logged in as {0}", AccountUsername));
            OnLogInStatusChanged(EventArgs.Empty);
        }

        public void LogOut()
        {
            AccessToken = null;
            TokenType = null;
            RefreshToken = null;

            _requiresNewAuthorization = true;
            LoggedIn = false;

            System.Diagnostics.Debug.WriteLine(String.Format("Logged out of {0}.", AccountUsername));
            OnLogInStatusChanged(EventArgs.Empty);
        }

        public bool TokensValid()
        {
            if (String.IsNullOrEmpty(AccessToken) || String.IsNullOrEmpty(TokenType) || TokenAcquireTime == null) return false;

            if (DateTime.UtcNow.CompareTo(ExpireTime) >= 0) //If the current time is after expiration time.
            {
                return false;
            }

            return true;
            
        }
    }
}
