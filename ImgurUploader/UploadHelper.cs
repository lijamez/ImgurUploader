using ImgurUploader.Model;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImgurUploader
{
    public class UploadHelper
    {
        private ImgurAPI _api;
        public const int RETRIES_COUNT = 0;

        public UploadHelper(ImgurAPI api)
        {
            _api = api;
        }

        public async Task<BatchUploadResult> UploadSingle(Stream imageStream, string fileName, string title, string description, string albumId, CancellationToken cancelToken)
        {
            System.Diagnostics.Debug.WriteLine("Now uploading single item...");

            UploadResultCollection uploadedItems = new UploadResultCollection();
            DateTime startTime = DateTime.UtcNow;

            UploadImageResult uploadImageResult = await this.UploadImage(imageStream, fileName, title, description, albumId, cancelToken);
            
            uploadedItems.UploadedImageResults.Add(uploadImageResult);

            BatchUploadResult batchResult = new BatchUploadResult(uploadedItems, null);

            batchResult.StartDate = startTime;
            batchResult.FinishDate = DateTime.UtcNow;

            return batchResult;
        }

        public async Task<BatchUploadResult> UploadAlbum(QueuedFile[] queuedFiles, AlbumPreferences albumPrefs, UploadProgressListener progress, CancellationToken cancelToken)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Now uploading an album with {0} items...", queuedFiles.Length));

            if (progress != null)
            {
                progress.SetMaxProgression(queuedFiles.Length);
            }

            UploadResultCollection uploadedItems = new UploadResultCollection();
            DateTime startTime = DateTime.UtcNow;

            int count = 0;
            foreach (QueuedFile queuedFile in queuedFiles)
            {
                if (cancelToken.IsCancellationRequested) { throw new TaskCanceledException(); }

                count++;
                if (progress != null) progress.NotifyProgressionMessage(String.Format("Uploading image {0} of {1}...", count, queuedFiles.Length));

                UploadImageResult uploadImageResult = null;
                using (Stream imageStream = await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(queuedFile.File))
                {
                    uploadImageResult = await UploadImage(imageStream, queuedFile.FileName, queuedFile.Title, queuedFile.Description, null, cancelToken);
                }
                uploadedItems.UploadedImageResults.Add(uploadImageResult);

                if (progress != null) progress.NotifyProgression(1);
            }

            if (cancelToken.IsCancellationRequested) { throw new TaskCanceledException(); }

            List<UploadImageResult> successfulUploads = uploadedItems.SuccessfulUploads;
            List<string> successfulUploadIds = new List<string>();

            foreach (UploadImageResult i in successfulUploads)
            {
                successfulUploadIds.Add(i.Result.Data.ID);
            }

            if (progress != null) progress.NotifyProgressionMessage(String.Format("Creating album..."));

            Basic<AlbumCreateData> createAlbumResult = await _api.CreateAlbum(successfulUploadIds.ToArray(), albumPrefs.Title, albumPrefs.Description, albumPrefs.Cover, cancelToken);

            BatchUploadResult batchUploadResult = new BatchUploadResult();
            batchUploadResult.IntendedAsAlbum = true;
            batchUploadResult.Images = uploadedItems;

            if (createAlbumResult.Success)
            {
                if (!String.Equals(albumPrefs.Privacy, AlbumPreferences.DEFAULT_PRIVACY) || !String.Equals(albumPrefs.Layout, AlbumPreferences.DEFAULT_LAYOUT))
                {
                    //Try not to make this call if not necessary
                    Basic<Boolean> albumUpdateResult = await _api.UpdateAlbum(createAlbumResult.Data.DeleteHash, null, null, null, albumPrefs.Privacy, albumPrefs.Layout, null, cancelToken);
                }

                batchUploadResult.AlbumCreateResults = createAlbumResult;
            }

            batchUploadResult.StartDate = startTime;
            batchUploadResult.FinishDate = DateTime.UtcNow;

            return batchUploadResult;
        }

        private async Task<UploadImageResult> UploadImage(Stream imageStream, string fileName, string title, string description, string albumId, CancellationToken cancelToken)
        {
            Exception ex = null;

            for (int retry = 0; retry <= RETRIES_COUNT; retry++)
            {
                try
                {
                    Basic<UploadData> uploadResult = await _api.Upload(imageStream, fileName, title, description, albumId, cancelToken);
                    UploadImageResult uploadImageResult = new UploadImageResult(new QueuedItem(), uploadResult);

                    return uploadImageResult;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine(String.Format("{0} tries remaining...", RETRIES_COUNT - retry));
                    ex = e;
                }
            }

            throw ex;
        }

    }
}
