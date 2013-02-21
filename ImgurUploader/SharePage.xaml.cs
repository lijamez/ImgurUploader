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

                    Basic<UploadData> uploadResult = null;

                    if (_sharedBitmapStreamRef != null)
                    {

                        IRandomAccessStreamWithContentType stream = await _sharedBitmapStreamRef.OpenReadAsync();
                        Stream imageStream = stream.AsStreamForRead();

                        UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        UploadProgressRing.IsActive = true;
                        uploadResult = await _api.Upload(imageStream, null, null, null, null);
                        UploadProgressRing.IsActive = false;

                    }
                    else if (_sharedStorageItems != null)
                    {
                        IStorageItem item = _sharedStorageItems[0];
                        if (item.IsOfType(StorageItemTypes.File))
                        {
                            StorageFile file = (StorageFile)item;

                            UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            UploadProgressRing.IsActive = true;
                            uploadResult = await _api.Upload(file, null, null, null);
                            UploadProgressRing.IsActive = false;

                        }
                    }

                    if (uploadResult == null)
                    {
                        UploadStatus.Text = "Something terrible has happened.\nOh noes :(";
                        UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        if (uploadResult.Success)
                        {
                            UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            ImgurURL.Text = uploadResult.Data.Link;
                            ImgurURL.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                        else
                        {
                            UploadStatus.Text = "Upload failed.";
                            UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
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
