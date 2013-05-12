using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.Model
{
    [DataContract]
    public class UploadData
    {
        [DataMember(Name = "id")]
        public string ID
        {
            get;
            set;
        }

        [DataMember(Name = "deletehash")]
        public string DeleteHash
        {
            get;
            set;
        }

        [DataMember(Name = "link")]
        public string Link
        {
            get;
            set;
        }

        [DataMember(Name = "error")]
        public string Error
        {
            get;
            set;
        }

        [DataMember(Name = "request")]
        public string Request
        {
            get;
            set;
        }

        [DataMember(Name = "parameters")]
        public string Parameters
        {
            get;
            set;
        }

        [DataMember(Name = "method")]
        public string Method
        {
            get;
            set;
        }
    }
}
