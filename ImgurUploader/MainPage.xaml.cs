using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
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
    public sealed partial class MainPage : Page
    {
        private ImgurAPI _api = new ImgurAPI();
        private FileOpenPicker _filePicker;
        private FileOpenPicker FilePicker
        {
            get
            {
                if (_filePicker == null)
                {
                    FileOpenPicker picker = new FileOpenPicker();
                    picker.ViewMode = PickerViewMode.Thumbnail;

                    // http://imgur.com/help/uploading#allowedtoupload
                    picker.FileTypeFilter.Add(".jpg");
                    picker.FileTypeFilter.Add(".jpeg");
                    picker.FileTypeFilter.Add(".png");
                    picker.FileTypeFilter.Add(".gif");
                    picker.FileTypeFilter.Add(".apng");
                    picker.FileTypeFilter.Add(".tiff");
                    picker.FileTypeFilter.Add(".tif");
                    picker.FileTypeFilter.Add(".bmp");
                    picker.FileTypeFilter.Add(".pdf");
                    picker.FileTypeFilter.Add(".xcf");

                    _filePicker = picker;
                }

                return _filePicker;
            }
        }
        private StorageFile SelectedFile
        {
            get;
            set;
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            


        }

        async void Browse_Click(object sender, RoutedEventArgs e)
        {
            SelectedFile = await FilePicker.PickSingleFileAsync();
            if (SelectedFile == null)
            {
                FilePath.Text = "No file selected.";
                UploadButton.IsEnabled = false;
            }
            else
            {
                FilePath.Text = SelectedFile.Name;
                BitmapImage bmpSource = new BitmapImage();
                bmpSource.UriSource = new Uri(SelectedFile.Path);
                UploadButton.IsEnabled = true;
            }
        }

        async void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UploadForm.IsEnabled = false;

                UploadProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                UploadResult.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ImageUrl.Text = String.Empty;

                Basic<UploadData> result = await _api.Upload(SelectedFile, TitleInput.Text, DescriptionInput.Text, null);

                if (result.Success)
                {
                    UploadStatus.Text = "Upload succeeded.";
                    ImageUrl.Text = result.Data.Link;
                    UploadResult.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    UploadStatus.Text = String.Format("Upload failed. Error code: {0}", result.Status);
                    UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                UploadStatus.Text = "An unknown error has occurred.";
                UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;

                throw ex;
            }
            finally
            {
                UploadProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UploadForm.IsEnabled = true;
                
            }

            
        }

        private void ImageUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            ImageUrl.SelectAll();
        }


    }
}
