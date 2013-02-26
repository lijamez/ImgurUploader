using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.Model
{
    [DataContract]
    class RefreshedAccessToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken
        {
            get;
            set;
        }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken
        {
            get;
            set;
        }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn
        {
            get;
            set;
        }

        [DataMember(Name = "token_type")]
        public string TokenType
        {
            get;
            set;
        }

        [DataMember(Name = "account_username")]
        public string AccountUsername
        {
            get;
            set;
        }
    }
}
