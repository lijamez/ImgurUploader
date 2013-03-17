using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.UploadResult
{
    public class UploadResultCollection
    {
        private List<UploadImageResult> _uploadedImageResults;
        public List<UploadImageResult> UploadedImageResults
        {
            get
            {
                if (_uploadedImageResults == null)
                {
                    _uploadedImageResults = new List<UploadImageResult>();
                }

                return _uploadedImageResults;
            }
        }

        public int TotalImageResults
        {
            get
            {
                return UploadedImageResults.Count;
            }
        }

        public List<UploadImageResult> SuccessfulUploads
        {
            get
            {
                List<UploadImageResult> successfulResults = new List<UploadImageResult>();
                foreach (UploadImageResult r in UploadedImageResults)
                {
                    if (r != null && r.Result.Success)
                    {
                        successfulResults.Add(r);
                    }
                }

                return successfulResults;
            }
        }

        public List<UploadImageResult> FailedUploads
        {
            get
            {
                List<UploadImageResult> failedResults = new List<UploadImageResult>();
                foreach (UploadImageResult r in UploadedImageResults)
                {
                    if (r != null && !r.Result.Success)
                    {
                        failedResults.Add(r);
                    }
                }

                return failedResults;
            }
        }

    }
}
