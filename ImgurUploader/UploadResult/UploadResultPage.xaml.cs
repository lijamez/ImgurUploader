using ImgurUploader.Common;
using ImgurUploader.Model;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadResultPage : LayoutAwarePage
    {
        BatchUploadResult _finishedUploadResult;

        public UploadResultPage()
        {
            this.InitializeComponent();

            AdGrid.DataContext = InAppPurchases.Instance;
            
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            string shareableUrl = BatchUploadResult.GetShareableUrl(_finishedUploadResult);

            DataRequest request = e.Request;

            if (!String.IsNullOrEmpty(shareableUrl))
            {
                request.Data.Properties.Title = "Link to Imgur pictures";
                request.Data.SetText(shareableUrl);
            }
            else
            {
                request.FailWithDisplayText("There's nothing to share here. How strange!");
            }
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

            if (arg is BatchUploadResult)
            {
                _finishedUploadResult = arg as BatchUploadResult;
                resultsControl = new UploadResultsControl(_finishedUploadResult);
            }
            else if (arg is string)
            {
                string id = arg as string;
                foreach (BatchUploadResult r in App.UploadHistoryMgr.UploadHistory)
                {
                    if (r != null && String.Equals(r.ID, id))
                    {
                        _finishedUploadResult = r;
                        resultsControl = new UploadResultsControl(_finishedUploadResult);
                    }
                }   
            }

            this.ContentGrid.DataContext = _finishedUploadResult;

            if (resultsControl != null)
            {
                ResultGrid.Children.Add(resultsControl);
            }
            else
            {
                this.Frame.GoBack();
            }

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareTextHandler;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= ShareTextHandler;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }



    }
}
