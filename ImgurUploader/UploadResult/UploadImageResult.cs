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
        public UploadImageResult() { }

        public UploadImageResult(QueuedItem item, Basic<UploadData> result)
        {
            Item = item;
            Result = result;
        }

        public QueuedItem Item
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
