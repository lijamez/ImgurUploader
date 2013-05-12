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

        public static string GetShareableUrl(FinishedUploadResult result)
        {
            string url = null;
            if (result != null)
            {
                if (result.AlbumCreateResults != null)
                {
                    url = String.Format("http://imgur.com/a/{0}", result.AlbumCreateResults.Data.ID);
                }
                else if (result.Images.SuccessfulUploads != null && result.Images.SuccessfulUploads.Count > 0)
                {
                    url = String.Format("http://imgur.com/{0}", result.Images.SuccessfulUploads[0].Result.Data.ID);
                }
            }
            return url;
        }

    }
}
