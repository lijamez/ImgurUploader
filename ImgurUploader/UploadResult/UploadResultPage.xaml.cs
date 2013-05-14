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
        FinishedUploadResult _finishedUploadResult;

        public UploadResultPage()
        {
            this.InitializeComponent();

            AdGrid.DataContext = InAppPurchases.Instance;
            
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            string shareableUrl = FinishedUploadResult.GetShareableUrl(_finishedUploadResult);

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


        private async void ShareToRedditButton_Click(object sender, RoutedEventArgs e)
        {
            string url = FinishedUploadResult.GetShareableUrl(_finishedUploadResult);

            if (url != null)
            {
                await Launcher.LaunchUriAsync(new Uri(String.Format("{0}{1}", "http://www.reddit.com/submit?url=", url)));
            }
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            string url = FinishedUploadResult.GetShareableUrl(_finishedUploadResult);

            if (url != null)
            {
                DataPackage dpkg = new DataPackage();
                dpkg.SetText(url);
                Clipboard.SetContent(dpkg);     
            }
            
        }

        private async void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            string url = FinishedUploadResult.GetShareableUrl(_finishedUploadResult);

            if (url != null)
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string deleteHash = null;
            bool isAlbum = false;
            if (_finishedUploadResult.AlbumCreateResults != null)
            {
                deleteHash = _finishedUploadResult.AlbumCreateResults.Data.DeleteHash;
                isAlbum = true;
            }
            else
            {
                deleteHash = _finishedUploadResult.Images.SuccessfulUploads[0].Result.Data.DeleteHash;
                isAlbum = false;
            }

            if (deleteHash != null)
            {
                Popup popup = new Popup();
                Panel panel = (Panel)DeleteButton.Content;
                panel.Children.Add(popup);


                popup.IsLightDismissEnabled = true;
                
                popup.ChildTransitions = new TransitionCollection();
                popup.ChildTransitions.Add(new PopupThemeTransition()
                {
                });

                string message = isAlbum ? "Are you sure you want to delete this album?" : "Are you sure you want to delete this image?";

                ConfirmationPopup confirmation = new ConfirmationPopup(message, "Yep", async delegate()
                {
                    ImgurAPI _api = new ImgurAPI();
                    CancellationTokenSource ts = new CancellationTokenSource();
                    Task<Basic<Boolean>> deletionResult;
                    if (isAlbum)
                    {
                        deletionResult = _api.DeleteAlbum(deleteHash, ts.Token);
                    }
                    else
                    {
                        deletionResult = _api.DeleteImage(deleteHash, ts.Token);
                    }

                    popup.IsOpen = false;
                    this.Frame.GoBack();

                    await deletionResult;
                });
                confirmation.Width = 160;
                confirmation.Height = 150;

                popup.VerticalOffset = -(confirmation.Height + 20);
                popup.HorizontalOffset = (DeleteButton.ActualWidth - confirmation.Width) / 2;

                popup.Child = confirmation;
                popup.IsOpen = true;
            }
        }
    }
}
