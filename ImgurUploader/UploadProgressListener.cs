using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurUploader
{
    public interface UploadProgressListener
    {
        void NotifyProgression(int count);
        void SetMaxProgression(int max);
        void NotifyProgressionMessage(string message);
    }
}
