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
            _uploadHistory = new ObservableCollection<FinishedUploadResult>();
        }

        public const string UPLOAD_HISTORY_FILE_NAME = "UploadHistory.xml";
        public const string SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME = "ShareCharmUploadHistory.xml";
        private ObservableCollection<FinishedUploadResult> _uploadHistory;
        public ObservableCollection<FinishedUploadResult> UploadHistory
        {
            get { return _uploadHistory; }
        }

        public async Task ReadUploadHistory()
        {
            try
            {
                _uploadHistory = new ObservableCollection<FinishedUploadResult>();

                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(UPLOAD_HISTORY_FILE_NAME);
                if (uploadHistoryFile != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<FinishedUploadResult>));
                    Stream fileStream = await uploadHistoryFile.OpenStreamForReadAsync();
                    ObservableCollection<FinishedUploadResult> readUploadHistory = (ObservableCollection<FinishedUploadResult>)serializer.Deserialize(fileStream);

                    _uploadHistory.Clear();
                    foreach (FinishedUploadResult r in readUploadHistory)
                    {
                        _uploadHistory.Insert(0, r);
                    }

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully read {0} entries from upload history from {1}", _uploadHistory.Count, uploadHistoryFile.Path));
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("An error has occurred when reading upload history.");
            }

            try
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME);
                if (uploadHistoryFile != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<FinishedUploadResult>));
                    Stream fileStream = await uploadHistoryFile.OpenStreamForReadAsync();
                    List<FinishedUploadResult> shareCharmUploadHistory = (List<FinishedUploadResult>)serializer.Deserialize(fileStream);

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully read {0} entries from upload history from {1}", _uploadHistory.Count, uploadHistoryFile.Path));

                    foreach (FinishedUploadResult r in shareCharmUploadHistory)
                    {
                        _uploadHistory.Insert(0, r);
                    }

                    await uploadHistoryFile.DeleteAsync();
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("An error has occurred when reading share charm upload history.");
            }
        }

        public async Task WriteUploadHistory()
        {
            if (_uploadHistory != null)
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UPLOAD_HISTORY_FILE_NAME, CreationCollisionOption.ReplaceExisting);

                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<FinishedUploadResult>));
                using (Stream fileStream = await uploadHistoryFile.OpenStreamForWriteAsync())
                {
                    serializer.Serialize(fileStream, _uploadHistory);

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully written {0} entries to upload history file at {1}", _uploadHistory.Count, uploadHistoryFile.Path));
                }
            }
        }


    }
}
