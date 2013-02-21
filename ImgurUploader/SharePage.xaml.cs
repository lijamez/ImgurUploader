using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
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
        private ShareOperation _shareOperation;
        private string _title;
        private string _description;
        private IRandomAccessStreamReference _sharedBitmapStreamRef;
        private IReadOnlyList<IStorageItem> _sharedStorageItems;
        ImgurAPI _api = new ImgurAPI();

        public SharePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _shareOperation = (ShareOperation) e.Parameter;

            await Task.Factory.StartNew(async () =>
            {
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

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    bool operationSuccess = false;
                    string link = String.Empty;

                    UploadProgressRing.IsActive = true;
                    UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;


                    if (_sharedBitmapStreamRef != null)
                    {
                        IRandomAccessStreamWithContentType stream = await _sharedBitmapStreamRef.OpenReadAsync();
                        Stream imageStream = stream.AsStreamForRead();

                        UploadStatus.Text = "Uploading image...";

                        Basic<UploadData> uploadResult = await _api.Upload(imageStream, null, null, null, null);

                        if (uploadResult != null && uploadResult.Success)
                        {
                            operationSuccess = true;
                            link = uploadResult.Data.Link;
                        }
                        else
                        {
                            UploadStatus.Text = "Failed to upload image.";
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
                                    Basic<UploadData> uploadResult = await _api.Upload(file, null, null, null);

                                    if (uploadResult != null && uploadResult.Success)
                                    {
                                        uploadedImageIDs.Add(uploadResult.Data.ID);
                                    }
                                    else
                                    {
                                        //TODO: Log this error.
                                    }
                                }
                            }

                            UploadStatus.Text = "Creating album...";
                            Basic<AlbumCreateData> albumCreationResult = await _api.CreateAlbum(uploadedImageIDs.ToArray(), null, null, null);
                            if (albumCreationResult != null && albumCreationResult.Success)
                            {
                                operationSuccess = true;
                                link = String.Format("http://imgur.com/a/{0}", albumCreationResult.Data.ID);
                            }
                            else
                            {
                                UploadStatus.Text = "Failed to create album.";
                            }
                        }
                        else if (_sharedStorageItems.Count == 1)
                        {
                            IStorageItem item = _sharedStorageItems[0];

                            if (item.IsOfType(StorageItemTypes.File))
                            {
                                UploadStatus.Text = "Uploading image...";

                                StorageFile file = (StorageFile)item;
                                Basic<UploadData> uploadResult = await _api.Upload(file, null, null, null);

                                if (uploadResult != null && uploadResult.Success)
                                {
                                    operationSuccess = true;
                                    link = uploadResult.Data.Link;
                                }
                                else
                                {
                                    UploadStatus.Text = "Failed to upload image.";
                                }
                            }
                        }

                        
                    }

                    UploadProgressRing.IsActive = false;

                    if (operationSuccess)
                    {
                        UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ImgurURL.Text = link;
                        ImgurURL.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }

                });
            });

        }

        private void ImgurURL_GotFocus(object sender, RoutedEventArgs e)
        {
            ImgurURL.SelectAll();
        }


    }
}
