using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.UploadResult
{
    public class BatchUploadResult
    {
        public BatchUploadResult()
        {
            ID = Guid.NewGuid().ToString();
        }

        public BatchUploadResult(UploadResultCollection images, Basic<AlbumCreateData> albumCreateResults)
        {
            Images = images;
            AlbumCreateResults = albumCreateResults;
            ID = Guid.NewGuid().ToString();
        }

        public bool IntendedAsAlbum
        {
            get;
            set;
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

        public enum Status
        {
            SUCCESSFUL, PARTIAL, FAILED, INVALID
        }

        public static Status GetStatus(BatchUploadResult result)
        {
            if (result == null || result.Images == null)
                return Status.INVALID;

            List<UploadImageResult> Fails = result.Images.FailedUploads;
            List<UploadImageResult> Successes = result.Images.SuccessfulUploads;

            if (result.IntendedAsAlbum)
            {
                if (result.AlbumCreateResults == null)
                    return Status.FAILED;

                if (Fails.Count <= 0 && Successes.Count <= 0)
                {
                    return Status.INVALID;
                }
                else if (Fails.Count == 0)
                {
                    return Status.SUCCESSFUL;
                }
                else if (Successes.Count == 0)
                {
                    return Status.FAILED;
                }
                else
                {
                    return Status.PARTIAL;
                }
            }
            else
            {
                if (Fails.Count + Successes.Count != 1)
                {
                    return Status.INVALID;
                }
                else if (Successes.Count == 1)
                {
                    return Status.SUCCESSFUL;
                }
                else
                {
                    return Status.FAILED;
                }
            }

        }

        public static string GetShareableUrl(BatchUploadResult result)
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
