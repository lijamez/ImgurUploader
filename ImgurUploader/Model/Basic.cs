using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.Model
{
    /// <summary>
    /// 
    /// https://api.imgur.com/models/basic
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class Basic<T>
    {
        public Basic() { }

        [DataMember(Name = "data")]
        public T Data
        {
            get;
            set;
        }

        [DataMember(Name = "success")]
        public bool Success
        {
            get;
            set;
        }

        [DataMember(Name = "status")]
        public int Status
        {
            get;
            set;
        }
    }
}
