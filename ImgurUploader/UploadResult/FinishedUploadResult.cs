using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.UploadResult
{
    public class FinishedUploadResult
    {
        public FinishedUploadResult()
        {
            
        }

        public FinishedUploadResult(UploadResultCollection images, Basic<AlbumCreateData> albumCreateResults)
        {
            Images = images;
            AlbumCreateResults = albumCreateResults;
            ID = Guid.NewGuid().ToString();
        }

        public string ID
        {
            get;
            set;
        }

        public UploadResultCollection Images
        {
            get;
            set;
        }

        public Basic<AlbumCreateData> AlbumCreateResults
        {
            get;
            set;
        }

        public DateTime StartDate
        {
            get;
            set;
        }

        public DateTime FinishDate
        {
            get;
            set;
        }
    }
}
