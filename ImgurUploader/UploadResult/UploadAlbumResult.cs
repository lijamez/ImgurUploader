using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.UploadResult
{
    public class UploadAlbumResult
    {
        public UploadAlbumResult(UploadResultCollection images, Basic<AlbumCreateData> albumCreateResults)
        {
            Images = images;
            Results = albumCreateResults;
        }

        public UploadResultCollection Images
        {
            get;
            set;
        }

        public Basic<AlbumCreateData> Results
        {
            get;
            set;
        }
    }
}
