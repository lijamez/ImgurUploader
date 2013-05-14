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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ImgurUploader.QuickButtons
{
    public sealed partial class DeleteButton : UserControl
    {
        public DeleteButton()
        {
            this.InitializeComponent();
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            FinishedUploadResult finishedUploadResult = this.DataContext as FinishedUploadResult;

            if (finishedUploadResult == null) return;

            string deleteHash = null;
            bool isAlbum = false;
            if (finishedUploadResult.AlbumCreateResults != null)
            {
                deleteHash = finishedUploadResult.AlbumCreateResults.Data.DeleteHash;
                isAlbum = true;
            }
            else
            {
                deleteHash = finishedUploadResult.Images.SuccessfulUploads[0].Result.Data.DeleteHash;
                isAlbum = false;
            }

            if (deleteHash != null)
            {
                
                Popup popup = new Popup();

                Button delButton = (Button)this.FindName("DelButton");
                Grid delButtonGrid = (Grid) this.FindName("DelButtonGrid");
                delButtonGrid.Children.Add(popup);


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
                    Frame frame = (Frame)(Window.Current.Content);
                    if (frame != null)
                    {
                        frame.GoBack();
                    }

                    await deletionResult;
                });
                confirmation.Width = 160;
                confirmation.Height = 150;

                popup.VerticalOffset = -(confirmation.Height + 10);
                popup.HorizontalOffset = (delButton.ActualWidth - confirmation.Width) / 2;

                popup.Child = confirmation;
                popup.IsOpen = true;
            }
        }
    }
}
