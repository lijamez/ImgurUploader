using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.Model
{
    [DataContract]
    public class Image
    {
        [DataMember(Name = "id")]
        public string ID
        {
            get;
            set;
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get;
            set;
        }

        [DataMember(Name = "description")]
        public string Description
        {
            get;
            set;
        }

        [DataMember(Name = "datetime")]
        public int DateTime
        {
            get;
            set;
        }

        [DataMember(Name = "type")]
        public string Type
        {
            get;
            set;
        }

        [DataMember(Name = "animated")]
        public bool Animated
        {
            get;
            set;
        }

        [DataMember(Name = "width")]
        public int Width
        {
            get;
            set;
        }

        [DataMember(Name = "Height")]
        public int Height
        {
            get;
            set;
        }

        [DataMember(Name = "size")]
        public int Size
        {
            get;
            set;
        }

        [DataMember(Name = "views")]
        public int Views
        {
            get;
            set;
        }

        [DataMember(Name = "bandwidth")]
        public int Bandwidth
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
    }
}
