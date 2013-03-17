using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader.UploadResult
{
    public class UploadImageResult
    {
        public UploadImageResult(QueuedImage img, Basic<UploadData> result)
        {
            Image = img;
            Result = result;
        }

        public QueuedImage Image
        {
            get;
            set;
        }

        public Basic<UploadData> Result
        {
            get;
            set;
        }
    }
}
