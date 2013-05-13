using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader
{
    public class AlbumPreferences
    {
        public AlbumPreferences()
        {

        }

        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public const string DEFAULT_PRIVACY = ImgurAPI.ALBUM_PRIVACY_PUBLIC;

        private string _privacy = DEFAULT_PRIVACY;
        public string Privacy
        {
            get { return _privacy; }
            set { _privacy = value; }
        }

        public const string DEFAULT_LAYOUT = ImgurAPI.ALBUM_LAYOUT_BLOG;

        private string _layout = DEFAULT_LAYOUT;
        public string Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        public string Cover
        {
            get;
            set;
        }

    }
}
