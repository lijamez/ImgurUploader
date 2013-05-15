using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace ImgurUploader.UploadHistory
{
    public class UploadHistoryManager
    {
        public UploadHistoryManager()
        {
            _uploadHistory = new ObservableCollection<BatchUploadResult>();
        }

        public const string UPLOAD_HISTORY_FILE_NAME = "UploadHistory.xml";
        public const string SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME = "ShareCharmUploadHistory.xml";
        private ObservableCollection<BatchUploadResult> _uploadHistory;
        public ObservableCollection<BatchUploadResult> UploadHistory
        {
            get { return _uploadHistory; }
        }
        private bool _initialized = false;

        public async Task Sync()
        {
            try
            {
                if (_initialized)
                {
                    await ReadAndDeleteShareCharmUploadHistory();
                    await WriteUploadHistory();
                }
                else
                {
                    await ReadUploadHistory();
                }
            }
            catch (UnauthorizedAccessException)
            {
                //Concurrent file access. Just ignore it.
                System.Diagnostics.Debug.WriteLine("Warning: Concurrent history file access.");
            }
        }

        private async Task ReadUploadHistory()
        {
            try
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(UPLOAD_HISTORY_FILE_NAME);

                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<BatchUploadResult>));
                ObservableCollection<BatchUploadResult> readUploadHistory = null;
                using (Stream fileStream = await uploadHistoryFile.OpenStreamForReadAsync())
                {
                    readUploadHistory = (ObservableCollection<BatchUploadResult>)serializer.Deserialize(fileStream);
                }

                _uploadHistory.Clear();
                foreach (BatchUploadResult r in readUploadHistory)
                {
                    _uploadHistory.Insert(0, r);
                }

                System.Diagnostics.Debug.WriteLine(String.Format("Successfully read {0} entries from upload history from {1}", _uploadHistory.Count, uploadHistoryFile.Path));

            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Error: An error has occurred when reading upload history.");
            }

            await ReadAndDeleteShareCharmUploadHistory();

            _initialized = true;
        }

        private async Task ReadAndDeleteShareCharmUploadHistory()
        {
            try
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME);

                XmlSerializer serializer = new XmlSerializer(typeof(List<BatchUploadResult>));
                List<BatchUploadResult> shareCharmUploadHistory = null;
                using (Stream fileStream = await uploadHistoryFile.OpenStreamForReadAsync())
                {
                    shareCharmUploadHistory = (List<BatchUploadResult>)serializer.Deserialize(fileStream);
                }

                System.Diagnostics.Debug.WriteLine(String.Format("Successfully read {0} entries from upload history from {1}", _uploadHistory.Count, uploadHistoryFile.Path));

                foreach (BatchUploadResult r in shareCharmUploadHistory)
                {
                    _uploadHistory.Insert(0, r);
                }

                await uploadHistoryFile.DeleteAsync();

            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Share charm history file not found.");
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Error: An error has occurred when reading share charm upload history.");
            }
        }

        private async Task WriteUploadHistory()
        {
            if (_uploadHistory != null)
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UPLOAD_HISTORY_FILE_NAME, CreationCollisionOption.ReplaceExisting);

                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<BatchUploadResult>));
                using (Stream fileStream = await uploadHistoryFile.OpenStreamForWriteAsync())
                {
                    serializer.Serialize(fileStream, _uploadHistory);

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully written {0} entries to upload history file at {1}", _uploadHistory.Count, uploadHistoryFile.Path));
                }
            }
        }


    }
}
