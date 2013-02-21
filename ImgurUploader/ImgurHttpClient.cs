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
        private static HttpClient _instance;
        public static HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", "CLIENT_ID GOES HERE!");

                    _instance = client;
                }

                return _instance;
            }
        }
    }
}
