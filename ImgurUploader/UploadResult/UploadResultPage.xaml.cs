using ImgurUploader.Common;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadResultPage : LayoutAwarePage
    {
        FinishedUploadResult _finishedUploadResult;

        public UploadResultPage()
        {
            this.InitializeComponent();

            AdGrid.DataContext = InAppPurchases.Instance;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            object arg = e.Parameter;

            ResultGrid.Children.Clear();

            UploadResultsControl resultsControl = null;

            if (arg is FinishedUploadResult)
            {
                _finishedUploadResult = arg as FinishedUploadResult;
                resultsControl = new UploadResultsControl(_finishedUploadResult);
            }
            else if (arg is string)
            {
                string id = arg as string;
                foreach (FinishedUploadResult r in App.UploadHistoryMgr.UploadHistory)
                {
                    if (r != null && String.Equals(r.ID, id))
                    {
                        _finishedUploadResult = r;
                        resultsControl = new UploadResultsControl(_finishedUploadResult);
                    }
                }   
            }

            if (resultsControl != null)
            {
                ResultGrid.Children.Add(resultsControl);
            }
            else
            {
                this.Frame.GoBack();
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private string getUrl()
        {
            string url = null;
            if (_finishedUploadResult.AlbumCreateResults != null)
            {
                url = String.Format("http://imgur.com/a/{0}", _finishedUploadResult.AlbumCreateResults.Data.ID);
            }
            else if (_finishedUploadResult.Images.SuccessfulUploads != null && _finishedUploadResult.Images.SuccessfulUploads.Count > 0)
            {
                url = String.Format("http://imgur.com/{0}", _finishedUploadResult.Images.SuccessfulUploads[0].Result.Data.ID);
            }

            return url;
        }

        private async void ShareToRedditButton_Click(object sender, RoutedEventArgs e)
        {
            string url = getUrl();

            if (url != null)
            {
                await Launcher.LaunchUriAsync(new Uri(String.Format("{0}{1}", "http://www.reddit.com/submit?url=", url)));
            }
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            string url = getUrl();

            if (url != null)
            {
                DataPackage dpkg = new DataPackage();
                dpkg.SetText(url);
                Clipboard.SetContent(dpkg);     
            }
            
        }

        private async void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            string url = getUrl();

            if (url != null)
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }
    }
}
