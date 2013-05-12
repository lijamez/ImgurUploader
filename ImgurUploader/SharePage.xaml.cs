using ImgurUploader.Model;
using ImgurUploader.UploadHistory;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SharePage : Page
    {
        private List<FinishedUploadResult> _shareCharmUploadHistory;
        private ShareOperation _shareOperation;
        private string _title;
        private string _description;
        private IRandomAccessStreamReference _sharedBitmapStreamRef;
        private IReadOnlyList<IStorageItem> _sharedStorageItems;
        ImgurAPI _api = new ImgurAPI();
        CancellationTokenSource _cancellationTokenSource;
        private FinishedUploadResult _finishedResults;


        public SharePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            System.Diagnostics.Debug.WriteLine("OnNavigatingFrom");
        }

        

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _shareOperation = (ShareOperation) e.Parameter;
            _shareOperation.ReportStarted();

            _title = _shareOperation.Data.Properties.Title;
            _description = _shareOperation.Data.Properties.Description;

            if (_shareOperation.Data.Contains(StandardDataFormats.Bitmap))
            {
                _sharedBitmapStreamRef = await _shareOperation.Data.GetBitmapAsync();
            }
            else if (_shareOperation.Data.Contains(StandardDataFormats.StorageItems))
            {
                _sharedStorageItems = await _shareOperation.Data.GetStorageItemsAsync();
            }

            await ReadUploadHistory();

            _cancellationTokenSource = new CancellationTokenSource();

            bool operationSuccess = false;
            string errorMsg = null;
            UploadResultCollection uploadedItems = new UploadResultCollection();

            DateTime startTime = DateTime.UtcNow;

            if (_sharedBitmapStreamRef != null)
            {
                IRandomAccessStreamWithContentType stream = await _sharedBitmapStreamRef.OpenReadAsync();
                Stream imageStream = stream.AsStreamForRead();

                UploadStatus.Text = "Uploading image...";

                Basic<UploadData> uploadResult = await _api.Upload(imageStream, null, null, null, null, _cancellationTokenSource.Token);
                UploadImageResult uploadImageResult = new UploadImageResult(new QueuedItem(), uploadResult);
                uploadedItems.UploadedImageResults.Add(uploadImageResult);

                _finishedResults = new FinishedUploadResult(uploadedItems, null);
                _finishedResults.StartDate = startTime;
                _finishedResults.FinishDate = DateTime.UtcNow;

                if (uploadResult != null && uploadResult.Success)
                {
                    operationSuccess = true;
                }
                else
                {
                    errorMsg = "Failed to upload image.";
                }

                        
            }
            else if (_sharedStorageItems != null)
            {
                if (_sharedStorageItems.Count > 1)
                {
                    List<string> uploadedImageIDs = new List<string>();

                    int currentImageCount = 0;
                    foreach (IStorageItem item in _sharedStorageItems)
                    {
                        currentImageCount++;
                        UploadStatus.Text = String.Format("Uploading image {0} of {1}...", currentImageCount, _sharedStorageItems.Count);

                        if (item.IsOfType(StorageItemTypes.File))
                        {
                            StorageFile file = (StorageFile)item;
                            Basic<UploadData> uploadResult = await _api.Upload(file, null, null, null, _cancellationTokenSource.Token);

                            if (uploadResult != null && uploadResult.Success)
                            {
                                uploadedImageIDs.Add(uploadResult.Data.ID);
                                uploadedItems.SuccessfulUploads.Add(new UploadImageResult(null, uploadResult));
                            }
                            else
                            {
                                uploadedItems.FailedUploads.Add(new UploadImageResult(null, uploadResult));
                            }
                        }
                    }

                    UploadStatus.Text = "Creating album...";
                    Basic<AlbumCreateData> albumCreationResult = await _api.CreateAlbum(uploadedImageIDs.ToArray(), null, null, null, _cancellationTokenSource.Token);
                    _finishedResults = new FinishedUploadResult(uploadedItems, albumCreationResult);
                    _finishedResults.StartDate = startTime;
                    _finishedResults.FinishDate = DateTime.UtcNow;

                    if (albumCreationResult != null && albumCreationResult.Success)
                    {
                        operationSuccess = true;
                    }
                    else
                    {
                        errorMsg = "Failed to create album.";
                    }
                }
                else if (_sharedStorageItems.Count == 1)
                {
                    IStorageItem item = _sharedStorageItems[0];

                    if (item.IsOfType(StorageItemTypes.File))
                    {
                        UploadStatus.Text = "Uploading image...";

                        StorageFile file = (StorageFile)item;
                        Basic<UploadData> uploadResult = await _api.Upload(file, null, null, null, _cancellationTokenSource.Token);
                        UploadImageResult uploadImageResult = new UploadImageResult(new QueuedItem(), uploadResult);

                        uploadedItems.UploadedImageResults.Add(uploadImageResult);
                        _finishedResults = new FinishedUploadResult(uploadedItems, null);
                        _finishedResults.StartDate = startTime;
                        _finishedResults.FinishDate = DateTime.UtcNow;

                        if (uploadResult != null && uploadResult.Success)
                        {
                            operationSuccess = true;
                        }
                        else
                        {
                            errorMsg = "Failed to upload image.";
                        }
                    }
                }

                        
            }

            int uploadHistoryIndex = -1;
            if (_finishedResults != null)
            {
                _shareCharmUploadHistory.Insert(0, _finishedResults);
                uploadHistoryIndex = _shareCharmUploadHistory.IndexOf(_finishedResults);
            }

            UploadProgressRing.IsActive = false;
            string toastLaunch = null;
            string toastTitle = null;
            string toastBody = null;

            await WriteUploadHistory();

            if (operationSuccess)
            {
                toastLaunch = String.Format("ShowLatestResults,{0}", _finishedResults.ID);

                toastTitle = "Upload Complete";
                toastBody = "Click or tap to get links.";

                _shareOperation.ReportCompleted();
            }
            else
            {
                if (String.IsNullOrEmpty(errorMsg)) errorMsg = "Something went wrong.";

                toastTitle = errorMsg;
                toastBody = "Care to try again?";

                _shareOperation.ReportError(errorMsg);
            }

            ToastNotification toast = Toaster.MakeToast(toastTitle, toastBody, toastLaunch);
            toast.Activated += OnToastClicked;

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async Task ReadUploadHistory()
        {
            try
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(UploadHistoryManager.SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME);
                if (uploadHistoryFile != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<FinishedUploadResult>));
                    Stream fileStream = await uploadHistoryFile.OpenStreamForReadAsync();
                    _shareCharmUploadHistory = (List<FinishedUploadResult>)serializer.Deserialize(fileStream);

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully read {0} entries from upload history from {1}", _shareCharmUploadHistory.Count, uploadHistoryFile.Path));
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("An error has occurred when reading upload history.");
            }

            if (_shareCharmUploadHistory == null)
            {
                _shareCharmUploadHistory = new List<FinishedUploadResult>();
            }
        }

        private async Task WriteUploadHistory()
        {
            if (_shareCharmUploadHistory != null)
            {
                StorageFile uploadHistoryFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UploadHistoryManager.SHARE_CHARM_UPLOAD_HISTORY_FILE_NAME, CreationCollisionOption.ReplaceExisting);

                XmlSerializer serializer = new XmlSerializer(typeof(List<FinishedUploadResult>));
                using (Stream fileStream = await uploadHistoryFile.OpenStreamForWriteAsync())
                {
                    serializer.Serialize(fileStream, _shareCharmUploadHistory);

                    System.Diagnostics.Debug.WriteLine(String.Format("Successfully written {0} entries to share charm upload history file at {1}", _shareCharmUploadHistory.Count, uploadHistoryFile.Path));
                }
            }
        }

        private void OnToastClicked(ToastNotification t, object o)
        {
            System.Diagnostics.Debug.WriteLine("Toast clicked");

            XmlDocument toastXml = t.Content;
            IXmlNode toastNode = (toastXml.GetElementsByTagName("toast"))[0];
            XmlAttribute launchAttr = (XmlAttribute) toastNode.Attributes.GetNamedItem("launch");
            System.Diagnostics.Debug.WriteLine(launchAttr.Value);
        }
    }
}
