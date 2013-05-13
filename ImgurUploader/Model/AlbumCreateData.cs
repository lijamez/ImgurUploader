using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.Model
{
    [DataContract]
    public class AlbumCreateData
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
    }
}
