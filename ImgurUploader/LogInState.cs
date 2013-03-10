using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImgurUploader
{
    class LogInState : ObservableObject
    {
        private ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public LogInState()
        {
            object o = null;

            _loggedIn = (o = _localSettings.Values["LoggedIn"]) == null ? false : (bool) o;
            _accessToken = _localSettings.Values["AccessToken"] as string;
            _tokenType = _localSettings.Values["TokenType"] as string;
            _tokenAcquireTime = (o = _localSettings.Values["TokenAcquireTime"]) == null ? DateTime.MinValue : new DateTime((long) o);
            _expireTime = (o = _localSettings.Values["ExpireTime"]) == null ? DateTime.MinValue : new DateTime((long) o);
            _refreshToken = _localSettings.Values["RefreshToken"] as string;
            _accountUsername = _localSettings.Values["AccountUsername"] as string;

        }

        private bool _loggedIn;
        public bool LoggedIn
        {
            get
            {
                return _loggedIn;
            }
            set
            {
                _loggedIn = value;
                _localSettings.Values["LoggedIn"] = _loggedIn;

                if (!_loggedIn)
                {
                    AccessToken = null;
                    TokenType = null;
                    TokenAcquireTime = DateTime.MinValue;
                    ExpireTime = DateTime.MinValue;
                    RefreshToken = null;
                    AccountUsername = null;
                }

                NotifyPropertyChanged();
            }
        }

        private string _accessToken;
        public string AccessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
                _localSettings.Values["AccessToken"] = _accessToken;
                NotifyPropertyChanged();
            }
        }

        private string _tokenType;
        public string TokenType
        {
            get
            {
                return _tokenType;
            }
            set
            {
                _tokenType = value;
                _localSettings.Values["TokenType"] = _tokenType;
                NotifyPropertyChanged();
            }
        }

        private DateTime _tokenAcquireTime;
        public DateTime TokenAcquireTime
        {
            get
            {
                return _tokenAcquireTime;
            }
            set
            {
                _tokenAcquireTime = value;
                _localSettings.Values["TokenAcquireTime"] = _tokenAcquireTime.ToUniversalTime().Ticks;
                NotifyPropertyChanged();
            }
        }

        private DateTime _expireTime;
        public DateTime ExpireTime
        {
            get
            {
                return _expireTime;
            }
            set
            {
                _expireTime = value;
                _localSettings.Values["ExpireTime"] = _expireTime.ToUniversalTime().Ticks;
                NotifyPropertyChanged();
            }
        }

        private string _refreshToken;
        public string RefreshToken
        {
            get
            {
                return _refreshToken;
            }
            set
            {
                _refreshToken = value;
                _localSettings.Values["RefreshToken"] = _refreshToken;
                NotifyPropertyChanged();
            }
        }

        private string _accountUsername;
        public string AccountUsername
        {
            get
            {
                return _accountUsername;
            }
            set
            {
                _accountUsername = value;
                _localSettings.Values["AccountUsername"] = _accountUsername;
                NotifyPropertyChanged();
            }
        }

    }
}
